using MegaCrit.Sts2.Core.Entities.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;


public abstract class NiGeiWoXiaoXinDianPower : YunoBasePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

}