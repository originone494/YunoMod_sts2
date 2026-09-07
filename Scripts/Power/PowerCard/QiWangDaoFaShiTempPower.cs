using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Skill;
using YunoMod.Scripts.Cards.Special;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class QiXingDaoFaShiTempPower : YunoTempStrengthPower<QiXingDaoFaShiCard>
{

}

[RegisterPower]
public class QiXingDaoFaShiTempDownPower : YunoTempStrengthPower<QiXingDaoFaShiCard>
{
    protected override bool IsPositive => false;


}
