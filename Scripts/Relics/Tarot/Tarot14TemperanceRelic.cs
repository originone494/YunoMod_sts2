using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位14-节制：获得虚弱/易伤/脆弱时，给予所有敌人相同层数的该减益
// 逆位：敌人身上没有减益且存在增益时，对你造成的伤害增加25%
public class Tarot14TemperanceRelic : TarotRelicBase
{
    private const decimal _bonusDamageMultiplier = 1.25m;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override bool SupportsReversed => true;

    // 正位：获得虚弱/易伤/脆弱时反射给所有敌人
    // AfterPowerAmountChanged 在实际生效层数确定后触发：人工制品挡成0不触发，负数消退(临时效果到期)被 amount>0 排除
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (IsReversed) return;
        if (amount <= 0) return;                          // 只反映"获得"
        if (power.Owner != Owner.Creature) return;        // 只响应自己获得（反射给敌人后不会递归）
        if (power is not (WeakPower or VulnerablePower or FrailPower)) return;

        var enemies = Owner.Creature.CombatState?.HittableEnemies;
        if (enemies == null || enemies.Count == 0) return;

        Flash();
        foreach (var enemy in enemies.ToList())
        {
            switch (power)
            {
                case WeakPower:
                    await PowerCmd.Apply<WeakPower>(choiceContext, enemy, amount, Owner.Creature, null);
                    break;
                case VulnerablePower:
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, amount, Owner.Creature, null);
                    break;
                case FrailPower:
                    await PowerCmd.Apply<FrailPower>(choiceContext, enemy, amount, Owner.Creature, null);
                    break;
            }
        }
    }

    // 逆位：无减益且有增益的敌人对你造成的伤害+25%
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!IsReversed) return 1m;
        if (dealer == null || !dealer.IsMonster) return 1m;
        if (target != Owner.Creature) return 1m;

        var powers = dealer.Powers;
        if (powers.Any(p => p.TypeForCurrentAmount == PowerType.Debuff)) return 1m;   // 身上有减益
        if (!powers.Any(p => p.TypeForCurrentAmount == PowerType.Buff)) return 1m;    // 没有增益

        return _bonusDamageMultiplier;
    }
}
