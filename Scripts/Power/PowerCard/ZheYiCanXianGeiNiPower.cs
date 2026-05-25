using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class ZheYiCanXianGeiNiPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 1m;

        decimal multiplier = 1m;

        if (dealer == Owner && Owner.HasPower<WeakPower>())
            multiplier *= 1m / 0.75m;

        if (target == Owner && Owner.HasPower<VulnerablePower>())
            multiplier *= 1m / 1.5m;

        return multiplier;
    }
}