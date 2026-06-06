using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class FuChouPower : YunoBasePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
