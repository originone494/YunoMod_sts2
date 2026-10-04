using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「天盃龍チュンドラ」（Tenpai Dragon Chundra，站内 cn_name「天杯龙 中龙」/ sc_name「天杯龙 红中龙」）：
//   打出：造成7点伤害
//   时机：回合结束时，检索除自身以外的1张「天杯龙」卡，之后打出此卡，可以进行一次「同调」
public class TianBeiLongHongZhongCard : YunoSpecialBaseCard
{
    public TianBeiLongHongZhongCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.TianBeiLong,
        YunoTags.LongZu,
        YunoTags.SiXing,
        // 「调整」：红中就是那张 4 星调整——同调分支一（非调整 + 4星调整 → 升龙）
        // 与分支二（调整 + 3星 → 升龙）都靠它成立。之前只有悬停提示、漏了这个标签，
        // 导致同调永远走"非调整"分支去找手牌里的调整卡（全模组只有升龙有），于是静默不触发。
        YunoTags.TiaoZheng,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.TianBeiLong),
        HoverTipFactory.FromKeyword(YunoKeywords.LongZu),
        HoverTipFactory.FromKeyword(YunoKeywords.SiXing),
        HoverTipFactory.FromKeyword(YunoKeywords.TiaoZheng),
        HoverTipFactory.FromKeyword(YunoKeywords.TongDiao),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成7点伤害
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // 一回合只能打出一次：按**这张卡自己**记（文本写的是「一回合一次」，不是「同名卡一回合一次」）
    private (CombatId? Combat, int Turn)? _playedThisTurn;

    protected override bool IsPlayable => Owner == null || _playedThisTurn != PerTurnOnce.CurrentKey(Owner);

    // 打出此卡后：「检索」除自身以外的1张「天杯龙」卡，返回手牌，之后可以进行一次「同调」
    //（一回合一次）
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this) return;
        if (Owner == null) return;

        var now = PerTurnOnce.CurrentKey(Owner);
        if (_playedThisTurn == now) return;
        _playedThisTurn = now;

        // 「检索」除自身以外的 1 张「天杯龙」卡（加入手牌）
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.TianBeiLong) && c.Id != Id,
            p => p is YunoSpecialCardPool,
            1, source: this);

        // 打出后返回手牌
        await CardPileCmd.Add(this, PileType.Hand);

        // 可以进行一次「同调」
        await TongDiao.TryTongDiao(choiceContext, Owner, this);
    }
}
