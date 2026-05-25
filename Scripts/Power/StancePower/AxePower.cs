using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Power;

public class AxePower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private const string _maxCount = "MaxCount";
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Unpowered | ValueProp.Unblockable),
        new DynamicVar(_maxCount, 8),
    ];

    public override async Task AfterRemoved(Creature oldOwner)
    {
        // 找到生命值最高的敌人
        var targetEnemy = oldOwner.CombatState?.HittableEnemies
            .OrderByDescending(enemy => enemy.CurrentHp)
            .FirstOrDefault();

        if (targetEnemy != null)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), targetEnemy, DynamicVars.Damage.BaseValue, DynamicVars.Damage.Props, base.Owner, null);
        }
    }

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

}
