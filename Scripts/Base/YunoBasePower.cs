using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace YunoMod.Scripts.Base;

[RegisterPower(Inherit = true)]
public abstract class YunoBasePower : ModPowerTemplate
{
    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Test/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Test/images/powers/{GetType().Name}.png"
    );

}