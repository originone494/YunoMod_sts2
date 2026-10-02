using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Special;

namespace YunoMod.Scripts.Power;

// 「珠泪·哀唱」驻场：被攻击前让攻击者在本回合失去力量（其回合结束自动恢复）
[RegisterPower]
public class ZhuLeiAiChangTempDownPower : YunoTempStrengthPower<ZhuLeiAiChangCard>
{
    protected override bool IsPositive => false;
}
