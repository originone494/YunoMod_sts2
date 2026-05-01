using STS2RitsuLib.Scaffolding.Content;

namespace YunoMod.Pool;

public class YunoPotionPool : TypeListPotionPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://YunoMod/images/energy_yuno.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://YunoMod/images/energy_yuno_big.png";

    public override string EnergyColorName => "YunoMod";
}