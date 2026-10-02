using MegaCrit.Sts2.Core.Combat;
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
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「天盃龍ファドラ」（Tenpai Dragon Fadra，站内 cn_name「天杯龙 发龙」/ sc_name「天杯龙 绿发龙」）：
//   打出：造成8点伤害
//   登场：从弃牌堆将1张「天杯龙」加入手牌
//   时机：回合结束时，打出此卡，可以进行一次「同调」
public class TianBeiLongLvFaCard : YunoSpecialBaseCard, IDengChangCard
{
    public TianBeiLongLvFaCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.TianBeiLong,
        YunoTags.LongZu,
        YunoTags.SanXing,
        YunoTags.DengChang,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.TianBeiLong),
        HoverTipFactory.FromKeyword(YunoKeywords.LongZu),
        HoverTipFactory.FromKeyword(YunoKeywords.SanXing),
        HoverTipFactory.FromKeyword(YunoKeywords.DengChang),
        HoverTipFactory.FromKeyword(YunoKeywords.TongDiao),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成8点伤害
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // 登场：从弃牌堆将1张「天杯龙」加入手牌
    public async Task DengChangSpecial(PlayerChoiceContext ctx, Player player)
    {
        var discardPile = PileType.Discard.GetPile(player);
        if (!discardPile.Cards.Any(c => c.Tags.Contains(YunoTags.TianBeiLong))) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            discardPile,
            player,
            new CardSelectorPrefs(DengChangPrompt, 1, 1),
            filter: c => c.Tags.Contains(YunoTags.TianBeiLong))).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }

    // 登场选卡提示
    private static LocString DengChangPrompt { get; } = new("card_selection", "TO_TIAN_BEI_LONG_LV_FA_DENG_CHANG");

    // 时机：回合结束时，打出此卡，可以进行一次「同调」
    // 打出此卡后，返回手牌，之后可以进行一次「同调」（同名卡一回合一次）
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this) return;
        if (Owner == null) return;

        var onceKey = PerTurnOnce.Key("Play", Id.Entry);
        if (PerTurnOnce.IsUsed(Owner, onceKey)) return;
        PerTurnOnce.Mark(Owner, onceKey);

        // 打出后返回手牌；回合结束的最后由游戏机制正常清空手牌（连同它一起进弃牌堆）
        await CardPileCmd.Add(this, PileType.Hand);

        // 可以进行一次「同调」
        await TongDiao.TryTongDiao(choiceContext, Owner, this);
    }
}
