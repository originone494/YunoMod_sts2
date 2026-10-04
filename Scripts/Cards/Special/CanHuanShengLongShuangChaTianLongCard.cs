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
//   登场：从消耗堆将1张「天杯龙」或「灿幻怪兽」加入手牌
//   时机：回合结束时，打出此卡
//   一回合打出过3张攻击卡的情况下，在回合结束时（游戏流程清空手牌之后）将位于弃牌堆的这张卡打出，之后可以丢弃1张手牌。
//
// "一回合打出过3张攻击卡"：静态计数，PlayerTurnStarted 清零、AfterCardPlayed（广播钩子）累加本回合
// 本卡拥有者打出的攻击卡；SideTurnEndedEvent（清空手牌之后）检查计数并从弃牌堆打出。
// 本卡打出后返回手牌（一回合一次，按这张卡自己记），时机打出与回合结束打出同样适用。
public class CanHuanShengLongShuangChaTianLongCard : YunoSpecialBaseCard, IDengChangCard
{
    public CanHuanShengLongShuangChaTianLongCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    static CanHuanShengLongShuangChaTianLongCard()
    {
        RitsuLibFramework.SubscribeLifecycle<PlayerTurnStartedEvent>(_ =>
        {
            _attacksPlayedThisTurn = 0;
        });
        RitsuLibFramework.SubscribeLifecycle<SideTurnEndedEvent>(evt =>
        {
            if (evt.Side != CombatSide.Player) return;
            _ = EndTurnPlay(evt.CombatState);
        });
    }

    private static int _attacksPlayedThisTurn;

    // 一回合只能打出一次：按**这张卡自己**记（文本写的是「一回合一次」，不是「同名卡一回合一次」）
    private (CombatId? Combat, int Turn)? _playedThisTurn;

    protected override bool IsPlayable => Owner == null || _playedThisTurn != PerTurnOnce.CurrentKey(Owner);

    // 回合结束的"打出"每回合只执行一次
    private (CombatId? Combat, int Turn)? _endPlayedTurn;

    // 登场：从消耗堆将1张「天杯龙」或「灿幻怪兽」加入手牌
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

    // 登场：从消耗堆将1张「天杯龙」或「灿幻怪兽」加入手牌
    public async Task DengChangSpecial(PlayerChoiceContext ctx, Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        if (!exhaustPile.Cards.Any(IsTianBeiLongOrCanHuanGuaiShou)) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            exhaustPile,
            player,
            CardPrefs(this, DengChangPrompt, 1, 1),
            filter: IsTianBeiLongOrCanHuanGuaiShou)).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }

    // 时机：回合结束时，打出此卡

    // 广播钩子：统计本回合打出的攻击卡 + 打出后返回手牌（一回合一次）
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null) return;

        if (cardPlay.Card?.Type == CardType.Attack && cardPlay.Card.Owner == Owner)
        {
            _attacksPlayedThisTurn++;
        }

        if (cardPlay.Card != this) return;
        var now = PerTurnOnce.CurrentKey(Owner);
        if (_playedThisTurn == now) return;
        _playedThisTurn = now;

        await CardPileCmd.Add(this, PileType.Hand);
    }

    // 回合结束（清空手牌之后）：一回合打出过3张攻击卡 → 将位于弃牌堆的这张卡打出，之后可以丢弃1张手牌
    private static async Task EndTurnPlay(ICombatState combatState)
    {
        var ctx = new BlockingPlayerChoiceContext();
        foreach (var player in combatState.Players)
        {
            var dragons = PileType.Discard.GetPile(player).Cards
                .OfType<CanHuanShengLongShuangChaTianLongCard>()
                .Where(c => c.Owner == player
                            && _attacksPlayedThisTurn >= 3
                            && c._endPlayedTurn != PerTurnOnce.CurrentKey(player))
                .ToList();

            foreach (var dragon in dragons)
            {
                dragon._endPlayedTurn = PerTurnOnce.CurrentKey(player);

                Creature? target = player.RunState.Rng.CombatTargets.NextItem(player.Creature.CombatState!.HittableEnemies);
                if (target == null) return;

                await LingHuoHook.AutoPlayFromDiscard(ctx, dragon, target);

                // 打出后返回手牌（一回合只能打出一次：本回合尚未打出过才回手）
                if (dragon._playedThisTurn != PerTurnOnce.CurrentKey(player))
                {
                    dragon._playedThisTurn = PerTurnOnce.CurrentKey(player);
                    await CardPileCmd.Add(dragon, PileType.Hand);
                }

                // 之后可以丢弃1张手牌（0~1，可选）
                await CardSelectCmd.FromHandForDiscard(
                    prefs: CardPrefs(dragon, DiscardNamedPrompt, 0, 1),
                    context: ctx,
                    player: player,
                    filter: null,
                    source: dragon);
            }
        }
    }

    private static bool IsTianBeiLongOrCanHuanGuaiShou(CardModel c)
        => c.Tags.Contains(YunoTags.TianBeiLong) || c.Tags.Contains(YunoTags.CanHuanGuaiShou);
}
