using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「盃満ちる燦幻荘」（Sangen Summoning，站内 cn_name「杯满的灿幻庄」）：
//   一回合只能打出一次（打出过一次后，本卡变成"不可打出"，回合刷新后恢复），打出后，返回手牌。
//   打出：「检索」1张「天杯龙」，之后，丢弃1张手牌。
//   这张卡在回合结束阶段被丢弃/消耗的情况下：选择手牌1张龙族同调卡（升龙/超龙），使其伤害翻倍。
//
// 回合结束阶段窗口：与双叉天龙同款（SideTurnEndingEvent 开窗 → SideTurnEndedEvent/回合开始关窗）。
// "打出后返回手牌"：AfterCardPlayed 是广播钩子，本卡打出结算完成后还在打牌区时移回手牌，
// 流水线末尾的落堆步骤因卡已不在打牌区而自动跳过（与双叉天龙回归同款手法）。
public class BeiManDeCanHuanZhuangCard : YunoSpecialBaseCard
{
    public BeiManDeCanHuanZhuangCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    static BeiManDeCanHuanZhuangCard()
    {
        RitsuLibFramework.SubscribeLifecycle<SideTurnEndingEvent>(evt =>
        {
            if (evt.Side != CombatSide.Player) return;
            _endPhaseWindow = true;
        });
        RitsuLibFramework.SubscribeLifecycle<SideTurnEndedEvent>(evt =>
        {
            if (evt.Side != CombatSide.Player) return;
            _endPhaseWindow = false;
        });
        RitsuLibFramework.SubscribeLifecycle<PlayerTurnStartedEvent>(_ =>
        {
            _endPhaseWindow = false;
        });
    }

    private static bool _endPhaseWindow;

    // 一回合只能打出一次：按**这张卡自己**记
    //（文本写的是「一回合只能打出一次」，不是「同名卡一回合一次」→ 每份拷贝各自一次）
    private (CombatId? Combat, int Turn)? _playedThisTurn;

    private bool PlayedThisTurn => Owner != null && _playedThisTurn == PerTurnOnce.CurrentKey(Owner);

    // 龙族同调卡：天杯龙族的同调产物
    private static Func<CardModel, bool> IsDragonSynchroCard
        => c => c.Tags.Contains(YunoTags.LongZu)
                && (c is CanHuanShengLongShuangChaTianLongCard || c is CanHuanChaoLongSanJiTianLongCard);

    // 回合结束阶段回归后的增益提示
    private static LocString BuffPrompt { get; } = new("card_selection", "TO_BEI_MAN_DE_CAN_HUAN_ZHUANG_BUFF");

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.CanHuanMoFa,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.CanHuanMoFa),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    // 一回合只能打出一次：本回合已经打出过 → 直接把这张卡变成"不可打出"
    //（UI 会灰掉、点了也打不出去），而不是"能打出去但没效果"。
    protected override bool IsPlayable => !PlayedThisTurn;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null) return;

        // 兜底：万一被效果绕过 IsPlayable 打出来，第二次也不结算
        if (PlayedThisTurn) return;

        // 「检索」1张「天杯龙」
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.TianBeiLong),
            p => p is YunoSpecialCardPool,
            1, source: this);

        // 之后，丢弃1张手牌
        await CardSelectCmd.FromHandForDiscard(
            prefs: CardPrefs(this, DiscardNamedPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this);
    }

    // 打出后，返回手牌（一回合一次，按这张卡自己记）
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this) return;
        if (Owner == null || PlayedThisTurn) return;
        _playedThisTurn = PerTurnOnce.CurrentKey(Owner);

        await CardPileCmd.Add(this, PileType.Hand);
    }

    // 回合结束阶段被丢弃：选择手牌1张龙族同调卡，使其伤害翻倍
    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        await base.AfterCardDiscarded(choiceContext, card);
        if (card != this || !_endPhaseWindow) return;
        await BuffLongDragon(choiceContext);
    }

    // 回合结束阶段被消耗：同上
    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        await base.AfterCardExhausted(choiceContext, card, causedByEthereal);
        if (card != this || !_endPhaseWindow) return;
        await BuffLongDragon(choiceContext);
    }

    private async Task BuffLongDragon(PlayerChoiceContext choiceContext)
    {
        if (Owner == null || CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Any(IsDragonSynchroCard)) return;

        CardModel? target = (await CardSelectCmd.FromHand(
            prefs: CardPrefs(this, BuffPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: IsDragonSynchroCard,
            source: this)).FirstOrDefault();

        if (target == null) return;

        // 伤害翻倍
        target.DynamicVars.Damage.BaseValue *= 2m;
    }
}
