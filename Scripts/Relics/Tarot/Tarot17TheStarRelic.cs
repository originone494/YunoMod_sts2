using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 星星：打出的卡费用比上一张高1点时，该卡的伤害翻倍或获得的格挡翻倍
// 逆位：正位的效果暂不生效
public class Tarot17TheStarRelic : TarotRelicBase
{
    private int? _lastCost;                                   // 上一张打出的卡的实测费用
    private CardPlay? _currentPlay;                           // 当前正在结算的打出
    private bool _doubleActive;                               // 当前打出是否符合翻倍条件
    private readonly Stack<(CardPlay? play, bool active)> _outerPlays = new();   // 嵌套打出保护（效果中自动打出另一张卡）

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override bool SupportsReversed => true;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (IsReversed) return Task.CompletedTask;            // 逆位：正位的效果暂不生效
        if (cardPlay.Card?.Owner != Owner) return Task.CompletedTask;

        // 嵌套打出：保存外层状态（如无用的钟的效果会在打出结算中自动打出另一张卡）
        _outerPlays.Push((_currentPlay, _doubleActive));
        _currentPlay = cardPlay;
        int cost = GetCost(cardPlay.Card);
        _doubleActive = _lastCost.HasValue && cost == _lastCost.Value + 1;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsReversed) return;                               // 逆位：正位的效果暂不生效
        if (cardPlay.Card?.Owner != Owner) return;

        _lastCost = GetCost(cardPlay.Card);
        if (_outerPlays.Count > 0 && _outerPlays.Peek().play == cardPlay)
        {
            (_currentPlay, _doubleActive) = _outerPlays.Pop();   // 恢复外层打出状态
        }
        else
        {
            _currentPlay = null;
            _doubleActive = false;
        }
    }

    // 伤害翻倍：只作用于本次打出的卡造成的攻击伤害
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!_doubleActive || cardPlay != _currentPlay) return 1m;
        if (dealer != Owner.Creature || cardSource != _currentPlay?.Card) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        if (target == null || !target.IsMonster) return 1m;

        return 2m;
    }

    // 格挡翻倍：只作用于本次打出的卡让自己获得的格挡
    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!_doubleActive || cardPlay != _currentPlay) return 1m;
        if (cardSource != _currentPlay?.Card) return 1m;
        if (target != Owner.Creature) return 1m;

        return 2m;
    }

    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        _lastCost = null;
        _currentPlay = null;
        _doubleActive = false;
        _outerPlays.Clear();
        return base.AfterCombatEnd(room);
    }

    private static int GetCost(CardModel card)
    {
        return card.EnergyCost.GetWithModifiers(CostModifiers.All);
    }
}
