using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
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
using STS2RitsuLib;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「燦幻昇龍バイデント・ドラギオン」（Sangenpai Bident Dragion，站内 sc_name「灿幻升龙 双戟天龙」）：
//   打出：造成20点伤害
//   登场：从弃牌堆将1张「天杯龙」或「灿幻怪兽」加入手牌
//   时机：回合结束时，打出此卡
//   回合结束阶段打出过3张攻击卡的情况：将位于弃牌堆的这张卡加入手牌，之后可以丢弃1张手牌。
//
// "回合结束阶段打出过3张攻击卡"的统计窗口（静态状态，跨实例共享）：
//   - SideTurnEndingEvent（RitsuLib 在 Hook.BeforeSideTurnEnd 扇出前的前缀）开启窗口并清零计数；
//   - AfterCardPlayed 是广播钩子（对所有卡牌模型触发），在此统计窗口内打出的攻击卡，
//     计数达到 3 的那一刻立即回归（与扇出顺序无关）；
//   - SideTurnEndedEvent / PlayerTurnStartedEvent 关闭窗口（战斗中途获胜跳出时由后者兜底重置）。
// 若"第3张攻击卡"就是本卡自己：AfterCardPlayed 时它还在打牌区（落堆发生在 AfterCardPlayed 之后），
// 此时移回手牌后，出牌流水线的落堆步骤会因卡已不在打牌区而自动跳过，回归不会被覆盖。
public class CanHuanShengLongShuangChaTianLongCard : YunoSpecialBaseCard, IDengChangCard
{
    public CanHuanShengLongShuangChaTianLongCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    static CanHuanShengLongShuangChaTianLongCard()
    {
        RitsuLibFramework.SubscribeLifecycle<SideTurnEndingEvent>(evt =>
        {
            if (evt.Side != CombatSide.Player) return;
            _endPhaseWindow = true;
            _attacksPlayedInEndPhase = 0;
            _returnedInEndPhase = false;
        });
        RitsuLibFramework.SubscribeLifecycle<SideTurnEndedEvent>(evt =>
        {
            if (evt.Side != CombatSide.Player) return;
            _endPhaseWindow = false;
        });
        RitsuLibFramework.SubscribeLifecycle<PlayerTurnStartedEvent>(_ =>
        {
            // 兜底：回合结束阶段中途获胜跳出时窗口可能未正常关闭
            _endPhaseWindow = false;
        });
    }

    private static bool _endPhaseWindow;
    private static int _attacksPlayedInEndPhase;
    private static bool _returnedInEndPhase;

    // 登场：从弃牌堆将1张「天杯龙」或「灿幻怪兽」加入手牌
    private static LocString DengChangPrompt { get; } = new("card_selection", "TO_CAN_HUAN_SHENG_LONG_DENG_CHANG");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.LongZu,
        YunoTags.QiXing,
        YunoTags.TiaoZheng,
        YunoTags.CanHuanGuaiShou,
        YunoTags.DengChang,
        YunoTags.TongBu,

    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LongZu),
        HoverTipFactory.FromKeyword(YunoKeywords.QiXing),
        HoverTipFactory.FromKeyword(YunoKeywords.TongBu),
        HoverTipFactory.FromKeyword(YunoKeywords.TiaoZheng),
        HoverTipFactory.FromKeyword(YunoKeywords.CanHuanGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.DengChang),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成20点伤害
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // 登场：从弃牌堆将1张「天杯龙」或「灿幻怪兽」加入手牌
    public async Task DengChangSpecial(PlayerChoiceContext ctx, Player player)
    {
        var discardPile = PileType.Discard.GetPile(player);
        if (!discardPile.Cards.Any(IsTianBeiLongOrCanHuanGuaiShou)) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            discardPile,
            player,
            new CardSelectorPrefs(DengChangPrompt, 1, 1),
            filter: IsTianBeiLongOrCanHuanGuaiShou)).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }

    // 时机：回合结束时，打出此卡
    // 广播钩子：统计回合结束阶段打出的攻击卡，达到3张时从弃牌堆回归
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出此卡后，返回手牌（同名卡一回合一次）
        if (cardPlay.Card == this && Owner != null)
        {
            var onceKey = PerTurnOnce.Key("Play", Id.Entry);
            if (!PerTurnOnce.IsUsed(Owner, onceKey))
            {
                PerTurnOnce.Mark(Owner, onceKey);
                await CardPileCmd.Add(this, PileType.Hand);
            }
        }

        if (!_endPhaseWindow) return;
        if (cardPlay.Card?.Type != CardType.Attack) return;
        if (cardPlay.Card.Owner != Owner) return;

        _attacksPlayedInEndPhase++;
        if (_attacksPlayedInEndPhase < 3 || _returnedInEndPhase) return;
        _returnedInEndPhase = true;

        // 将位于弃牌堆的这张卡加入手牌。
        // 若本卡自己就是第3张攻击卡：此刻它还在打牌区，移入手牌后，
        // 出牌流水线末尾的落堆步骤（仅当卡仍在打牌区才执行）会自动跳过，回归不会被覆盖。
        var pileType = Pile?.Type;
        // 只有确实位于弃牌堆（或它是第3张攻击卡、此刻还在打牌区）才回归；
        // 已经回到手牌的情况不再算"从弃牌堆回归"。
        if (pileType != PileType.Discard && pileType != PileType.Play) return;
        await CardPileCmd.Add(this, PileType.Hand);

        // 之后可以丢弃1张手牌（0~1，可选）
        await CardSelectCmd.FromHandForDiscard(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this);
    }

    private static bool IsTianBeiLongOrCanHuanGuaiShou(CardModel c)
        => c.Tags.Contains(YunoTags.TianBeiLong) || c.Tags.Contains(YunoTags.CanHuanGuaiShou);
}
