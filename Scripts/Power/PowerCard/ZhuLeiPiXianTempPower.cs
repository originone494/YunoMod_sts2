using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Special;

namespace YunoMod.Scripts.Power;

// 「珠泪·劈弦」驻场：打出珠泪怪兽时，所有敌人在本回合失去力量（其回合结束自动恢复）
[RegisterPower]
public class ZhuLeiPiXianTempDownPower : YunoTempStrengthPower<ZhuLeiPiXianCard>
{
    protected override bool IsPositive => false;
}
