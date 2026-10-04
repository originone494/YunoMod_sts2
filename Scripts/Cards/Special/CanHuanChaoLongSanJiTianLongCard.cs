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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「燦幻超龍トランセンド・ドラギオン」（Sangenpai Transcendent Dragion，站内 sc_name「灿幻超龙 三极天龙」）：
//   一回合只能打出一次，打出后返回手牌。
//   打出：造成30点伤害
//   登场：打出手牌所有的攻击卡，之后将那些卡返回手牌（登场效果不能打出无法被打出的卡，CanPlay 过滤）
//   一回合打出过3张攻击卡的情况下，在回合结束时（游戏流程清空手牌之后）将位于弃牌堆的这张卡打出，之后可以丢弃1张手牌。
//
// "一回合打出过3张攻击卡"：静态计数，PlayerTurnStarted 清零、AfterCardPlayed（广播钩子）累加本回合
// 本卡拥有者打出的攻击卡；SideTurnEndedEvent（清空手牌之后）检查计数并从弃牌堆打出。
public class CanHuanChaoLongSanJiTianLongCard : YunoSpecialBaseCard, IDengChangCard
{
    public CanHuanChaoLongSanJiTianLongCard() : base(3, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    static CanHuanChaoLongSanJiTianLongCard()
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

    // 一回合只能打出一次：按**这张卡自己**记
    private (CombatId? Combat, int Turn)? _playedThisTurn;

    // 回合结束的"打出"每回合只执行一次
    private (CombatId? Combat, int Turn)? _endPlayedTurn;

    protected override bool IsPlayable => Owner == null || _playedThisTurn != PerTurnOnce.CurrentKey(Owner);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30m, ValueProp.Move),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.LongZu,
        YunoTags.ShiXing,
        YunoTags.CanHuanGuaiShou,
        YunoTags.DengChang,
        YunoTags.TongBu,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [

    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LongZu),
        HoverTipFactory.FromKeyword(YunoKeywords.ShiXing),
        HoverTipFactory.FromKeyword(YunoKeywords.TongBu),
        HoverTipFactory.FromKeyword(YunoKeywords.CanHuanGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.DengChang),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成30点伤害
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // 登场：打出手牌所有的攻击卡，之后将那些卡返回手牌
    // （登场效果不能打出无法被打出的卡：用游戏自身的 CanPlay 过滤——含卡牌逻辑、不可打关键字等判定）
    public async Task DengChangSpecial(PlayerChoiceContext ctx, Player player)
    {
        var hand = PileType.Hand.GetPile(player).Cards
            .Where(c => c.Type == CardType.Attack && c.CanPlay())
            .ToList();
        if (hand.Count == 0) return;

        // 打出（快照后逐张：打出过程中可能被其它效果移走，落手牌之外则跳过）
        foreach (var card in hand)
        {
            if (card.Pile?.Type != PileType.Hand) continue;
            Creature? target = player.RunState.Rng.CombatTargets.NextItem(player.Creature.CombatState!.HittableEnemies);
            if (target == null) return;

            await CardPileCmd.Add(card, PileType.Play);
            await CardCmd.AutoPlay(ctx, card, target);
        }

        // 之后将那些卡返回手牌（打出后落在弃牌堆的才回；被消耗等其它去向不回）
        foreach (var card in hand)
        {
            if (card.Pile?.Type == PileType.Discard)
            {
                await CardPileCmd.Add(card, PileType.Hand);
            }
        }
    }

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
                .OfType<CanHuanChaoLongSanJiTianLongCard>()
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
}
