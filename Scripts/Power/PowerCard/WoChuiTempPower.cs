using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Skill;

namespace YunoMod.Scripts.Power;

// 我锤的临时力量（负面）：目标失去力量，其回合结束自动恢复
[RegisterPower]
public class WoChuiTempStrengthDownPower : YunoTempStrengthPower<WoChuiCard>
{
    protected override bool IsPositive => false;
}
