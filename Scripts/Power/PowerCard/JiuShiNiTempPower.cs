using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Skill;

namespace YunoMod.Scripts.Power;

// 「就是你？」本回合临时力量（正面）：自己获得力量，回合结束自动消失
[RegisterPower]
public class JiuShiNiTempPower : YunoTempStrengthPower<JiuShiNiCard>
{

}

// 「就是你？」本回合临时力量（负面）：目标失去力量，回合结束自动恢复
[RegisterPower]
public class JiuShiNiTempDownPower : YunoTempStrengthPower<JiuShiNiCard>
{
    protected override bool IsPositive => false;


}
