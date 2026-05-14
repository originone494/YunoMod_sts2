using MegaCrit.Sts2.Core.Entities.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class KanPoPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

}