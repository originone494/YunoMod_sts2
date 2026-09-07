using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
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

public class ZhuLeiLeiNuoHaTeCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiLeiNuoHaTeCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 伤害使用动态变量：造成 15 点伤害
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(15m, ValueProp.Move),
    };

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.LingHuo,

    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 造成15点伤害
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, cardPlay);

        // 「检索」1张「珠泪·雷诺哈特」以外的「珠泪」卡（珠泪=带珠泪标签且非珠泪融合卡，排除自身）
        var retrievedList = await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.ZhuLei)
                 && !c.Tags.Contains(YunoTags.ZhuLeiRongHe) && !c.Tags.Contains(YunoTags.ZhuLeiMoXian)
                 && c is not ZhuLeiLeiNuoHaTeCard,
            p => p is YunoSpecialCardPool,
            1, true);

    }

    private static LocString LingHuoChoicePrompt { get; } = new("card_selection", "TO_ZHU_LEI_LEI_NUO_HA_TE_LING_HUO");

    // 灵活：若手牌有除「珠泪·雷诺哈特」的「珠泪」卡，可以选择将这张卡加入手牌，
    // 然后选择1张除「珠泪·雷诺哈特」的「珠泪」卡丢弃，加入手牌的这张卡可以在这个回合免费打出。
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        // ① 条件：手牌有除自己外的「珠泪」卡（带珠泪标签且非珠泪融合卡）
        var handZhuLei = PileType.Hand.GetPile(player).Cards
            .Where(c => c.Tags.Contains(YunoTags.ZhuLei)
                        && !c.Tags.Contains(YunoTags.ZhuLeiRongHe)
                        && c != this)
            .ToList();
        if (handZhuLei.Count == 0) return;

        // ② 是/否询问（网格 + 提示文本，复用「是」「否」辅助卡）
        var shi = player.Creature.CombatState!.CreateCard<ShiCard>(player);
        var fou = player.Creature.CombatState!.CreateCard<FouCard>(player);
        CardModel? picked = (await CardSelectCmd.FromSimpleGrid(
            ctx,
            new List<CardModel> { shi, fou },
            player,
            new CardSelectorPrefs(LingHuoChoicePrompt, 1, 1))).FirstOrDefault();
        if (picked is not ShiCard) return; // 否/取消 → 不发动，卡照常进弃牌堆

        // ③ 加入手牌 + 本回合免费打出；标记已被灵活处理，避免补丁把刚回手的卡再弃一次
        await CardPileCmd.Add(this, PileType.Hand);
        EnergyCost.AddThisTurn(-EnergyCost.GetWithModifiers(CostModifiers.None));
        LingHuoHook.HandledByLingHuo.Add(this);

        // ④ 选择1张除自己外的「珠泪」卡丢弃（珠泪=带珠泪标签且非珠泪融合卡）
        var selected = (await CardSelectCmd.FromHandForDiscard(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            context: ctx,
            player: player,
            filter: c => c.Tags.Contains(YunoTags.ZhuLei)
                         && !c.Tags.Contains(YunoTags.ZhuLeiRongHe)
                         && c != this,
            source: this)).ToList();
        if (selected.Count == 0) return;

        CardModel discarded = selected[0];
        await CardCmd.Discard(ctx, discarded);

        // ⑤ 被丢弃的珠泪也能触发自己的灵活（补丁有防重入保护，这里手动补触发）
        if (discarded.Keywords.Contains(YunoKeywords.LingHuo) || discarded.Tags.Contains(YunoTags.LingHuo))
        {
            await LingHuoHook.LingHuoSpecial(ctx, player, discarded);
            await LingHuoHook.OnLingHuo(ctx, player);
        }
    }
}
