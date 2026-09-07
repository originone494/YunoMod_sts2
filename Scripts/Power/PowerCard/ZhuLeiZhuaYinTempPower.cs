using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Special;

namespace YunoMod.Scripts.Power;

// 「珠泪·爪音」本回合临时力量（负面）：目标失去力量，其回合结束自动恢复
[RegisterPower]
public class ZhuLeiZhuaYinTempDownPower : YunoTempStrengthPower<ZhuLeiZhuaYinCard>
{
    protected override bool IsPositive => false;
}
