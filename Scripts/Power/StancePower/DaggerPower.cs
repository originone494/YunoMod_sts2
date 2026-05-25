using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class DaggerPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private const string _maxCount = "MaxCount";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4, ValueProp.Unpowered),
        new DynamicVar(_maxCount, 12),
    ];


    public override bool TryModifyPowerAmountReceived(
    PowerModel canonicalPower,  // Power 原型
    Creature target,            // 目标
    decimal amount,             // 原数值
    Creature? applier,          // 施加者
    out decimal modifiedAmount  // 修改后数值
)
    {
        if (canonicalPower == this && target == applier)
        {
            if (amount + Amount >= DynamicVars[_maxCount].BaseValue)
            {
                modifiedAmount = DynamicVars[_maxCount].BaseValue - Amount;
            }
            else
            {
                modifiedAmount = amount;
            }

            return true;
        }
        modifiedAmount = amount;
        return false;
    }

    public override async Task AfterRemoved(Creature oldOwner)  
    {
        for (int i = 0; i < Amount; i++)
        {
            await Cmd.CustomScaledWait(0.1f, 0.2f);
            Creature creature = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.Owner.CombatState.HittableEnemies);
            if (creature != null)
            {
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), creature, DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props, base.Owner);
            }
        }
    }

}