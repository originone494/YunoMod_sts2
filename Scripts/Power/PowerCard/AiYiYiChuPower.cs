using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class AiYiYiChuPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    // {
    //     if (target != base.Owner)
    //     {
    //         return 1m;
    //     }
    //     if (props.HasFlag(ValueProp.Unpowered))
    //     {
    //         return 1m;
    //     }
    //     if (cardSource == null)
    //     {
    //         return 1m;
    //     }
    //     return 0m;
    // }

}