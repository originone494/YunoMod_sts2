using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace YunoMod.Scripts.Base;

// 注册power并设置Inherit = true，使得继承这个类的power自动被注册
[RegisterPower(Inherit = true)]
public abstract class YunoTempStrengthPower<T> : ModTemporaryAppliedPowerTemplate<T, StrengthPower> where T : AbstractModel
{
    // 自定义图标路径。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Test/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Test/images/powers/{GetType().Name}.png"
    );

    // protected override bool IsPositive => false; // 正面效果还是负面

    // protected override bool UntilEndOfOtherSideTurn => false; // 为 true 时，在另一方回合结束时过期；否则在拥有者一方回合结束时过期。

    // protected override int LastForXExtraTurns => 0; // 额外持续回合数

    // 推荐重载描述，以达到多个power共享一条文本的效果
    // 例如这里的文本需要在powers.json中写"TEST_POWER_TEMP_POWER.description"和"TEST_POWER_TEMP_POWER_DOWN.description"
    public override LocString Description => new("powers", IsPositive ? "YUNO_MOD_POWER_TEMP_POWER.description" : "YUNO_MOD_POWER_TEMP_POWER_DOWN.description");
}
