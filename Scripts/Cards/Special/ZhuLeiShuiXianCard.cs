using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·水仙人鱼（融合怪兽）
//   打出：造成14点伤害
//   登场：「检索」1张除「珠泪融合怪兽」以外的「珠泪」卡，选择加入手牌或丢弃
//         （由 DengChangHook 在"本卡因效果加入手牌"时触发；有后注效果 → 不打出，改为触发它）
//   驻场：回合结束时，可以将弃牌堆1张「珠泪」卡加入手牌，之后，丢弃这张卡
//   灵活：从抽牌堆顶将5张卡送入弃牌堆
public class ZhuLeiShuiXianCard : YunoSpecialBaseCard, ILingHuoCard, IDengChangCard
{
    public ZhuLeiShuiXianCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 伤害使用动态变量：造成 14 点伤害
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(14m, ValueProp.Move),
    };

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiRongHeGuaiShou,
        YunoTags.LingHuo,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.DengChang,
    ];

    // 「登场」不进关键字列表（卡面用纯文本写"登场：…"，关键字只在悬停里给说明），与「应对」同一套写法
    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.DengChang),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiRongHeGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
    ];

    // 「检索」后问是否加入手牌（是→加入手牌，否→丢弃）
    private static LocString RetrieveChoicePrompt { get; } = new("card_selection", "TO_ZHU_LEI_SHUI_XIAN_RETRIEVE");

    // 驻场：从弃牌堆选1张「珠泪」卡
    private static LocString ZhuChangRetrievePrompt { get; } = new("card_selection", "TO_ZHU_LEI_SHUI_XIAN_ZHU_CHANG");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 造成14点伤害
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, cardPlay);
    }

    // 登场：同名卡一回合一次。「检索」1张除「珠泪融合怪兽」以外的「珠泪」卡，选择加入手牌或丢弃
    public async Task DengChangSpecial(PlayerChoiceContext ctx, Player player)
    {
        // 「检索」本身是 0~1 的选择（RetrieverCard 内部用 min 0）：玩家可以先决定不检索，
        // 此时整段结束，不会有任何卡进入手牌。
        // 选中的卡此时已按生成卡加入了手牌（RetrieverCard 的默认形态）。
        var retrievedList = await ToolCmd.RetrieverCard(
            ctx,
            player,
            ZhuLeiFilter.IsCardExceptFusionMonster,
            p => p is YunoSpecialCardPool,
            1, source: this);
        if (retrievedList.Count == 0) return;

        CardModel retrieved = retrievedList[0];

        // 「选择加入手牌或丢弃」= 是/否，必须选一个（是 → 留在手牌，否 → 丢弃）
        if (!await ToolCmd.AskYesNo(ctx, player, RetrieveChoicePrompt, source: this))
        {
            // 选「否」→ 丢弃（走真正的弃牌语义，会触发被弃卡的灵活）
            await CardCmd.Discard(ctx, retrieved);
        }
    }

    // 驻场：回合结束时（用 BeforeSideTurnEnd：它在"清空手牌"之前），
    //       可以将弃牌堆1张「珠泪」卡加入手牌，之后，丢弃这张卡
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;

        var player = Owner;
        if (player is null) return;

        // ① 可以：从弃牌堆选1张「珠泪」卡加入手牌（min 0 → 可以什么都不选）
        var options = PileType.Discard.GetPile(player).Cards.Where(ZhuLeiFilter.IsCard).ToList();
        if (options.Count == 0) return;

        CardModel? picked = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            options,
            player,
            CardPrefs(this, ZhuChangRetrievePrompt, 0, 1))).FirstOrDefault();

        // 玩家没选 → 后续不处理（不丢弃这张卡）
        if (picked == null) return;

        await CardPileCmd.Add(picked, PileType.Hand);

        // ② 之后：丢弃这张卡（真正的弃牌，会触发本卡的灵活：送墓5张）
        await CardCmd.Discard(choiceContext, this);
    }

    // 灵活：从抽牌堆顶将5张卡送入弃牌堆
    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.DuiMu(ctx, player, 5);
    }
}
