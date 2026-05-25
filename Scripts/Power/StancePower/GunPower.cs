using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class GunPower : YunoBasePower
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    private const string _maxCount = "MaxCount";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Unpowered),
        new DynamicVar(_maxCount, 6),
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
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.CombatState!.HittableEnemies, DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props, base.Owner);
        }
    }
}