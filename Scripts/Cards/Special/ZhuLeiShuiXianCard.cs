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
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

public class ZhuLeiShuiXianCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiShuiXianCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 伤害使用动态变量：驻场对随机敌人造成 23 点伤害
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(23m, ValueProp.Move),
    };

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiRongHe,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.LingHuo,
        YunoTags.ZhuChang
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuChang),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiRongHe),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, cardPlay);
        // 「检索」1张「珠泪」卡（珠泪=带珠泪标签且非珠泪融合卡），检索到的卡已加入手牌
        var retrievedList = await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.ZhuLei) && !c.Tags.Contains(YunoTags.ZhuLeiRongHe),
            p => p is YunoSpecialCardPool,
            1);
        if (retrievedList.Count == 0) return;
        CardModel retrieved = retrievedList[0];

        // 让玩家从「是」「否」中选择：是→加入手牌，否→送入弃牌堆
        if (!await ToolCmd.AskYesNo(choiceContext, Owner, RetrieveChoicePrompt))
        {
            // 选「否」：检索到的卡已加入手牌，这里丢弃——会经过灵活补丁触发被弃珠泪的灵活
            await CardCmd.Discard(choiceContext, retrieved);
        }
        // 选「是」：卡已由检索加入手牌，无需额外操作
    }

    // 驻场：回合结束时，对随机敌人造成23点伤害
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;


        Creature? enemy = Owner!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy == null) return;

        await CreatureCmd.Damage(choiceContext, enemy, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, null);
    }

    // 灵活：从抽牌堆顶将5张卡送入弃牌堆（触发效果而非打出）
    private static LocString RetrieveChoicePrompt { get; } = new("card_selection", "TO_ZHU_LEI_SHUI_XIAN_RETRIEVE");

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.DuiMu(ctx, player, 5);
    }
}
