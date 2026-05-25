using System.Net.Http.Headers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;


public class NiGeiWoXiaoXinDianPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount
    )
    {

        if (amount > 0 && canonicalPower is StrengthPower && target.IsMonster && !target.IsPlayer)
        {
            for (int i = 0; i < Amount; i++)
            {
                PowerCmd.Apply<StrengthPower>(Owner, amount, Owner, null);
            }
        }
        modifiedAmount = amount;
        return false;
    }
}