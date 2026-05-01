using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Pool;

namespace YunoMod.Base;

[RegisterRelic(typeof(YunoRelicPool), Inherit = true)]

public abstract class AbstractTemplateBaseRelic : ModRelicTemplate
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Common;

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://YunoMod/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://YunoMod/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://YunoMod/images/relics/{GetType().Name}.png"
    );
}