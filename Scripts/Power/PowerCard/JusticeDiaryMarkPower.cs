using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

// 正义日记的标记：战斗开始时被随机标记的敌人，若它第一个被击杀，日记持有者将获得最大生命值
[RegisterPower]
public class JusticeDiaryMarkPower : YunoBasePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
}