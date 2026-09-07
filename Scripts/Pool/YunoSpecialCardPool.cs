using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Pool;

// 必须标 [RegisterSharedCardPool]：该池不是人物卡池（人物卡池会自动进入 ModelDb.AllCardPools），
// 若不加则它不会出现在 ModelDb.AllCardPools 里，卡牌的 card.Pool 解析会抛
// “not in any card pool!” 异常，导致奖励卡的渲染/领取失败。
[RegisterSharedCardPool]
public class YunoSpecialCardPool : TypeListCardPoolModel
{
    // 卡池的ID。必须唯一防撞车。
    public override string Title => "YunoModSpecicalCardPool";

#pragma warning disable CS0618, CS0672 // CardTypes 旧式钩子：显式枚举卡池包含的特殊卡类型。
    // 若不填充，卡池的 AllCards 为空，CardFactory.CreateForReward 无法生成奖励卡，
    // 击败精英后将不会出现特殊卡奖励。这里反射收集所有 YunoSpecialBaseCard 子类。
    protected override IEnumerable<Type> CardTypes => Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => !t.IsAbstract && typeof(YunoSpecialBaseCard).IsAssignableFrom(t));
#pragma warning restore CS0618, CS0672
    public override string EnergyColorName => "YunoMod";

    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://YunoMod/images/energy_yuno.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://YunoMod/images/energy_yuno_big.png";

    // 卡池的主题色。
    public override Color DeckEntryCardColor => new(0.5f, 0.5f, 1f);
    // 能量表盘文字轮廓颜色
    public override Color EnergyOutlineColor => new(0.5f, 0.5f, 1f);

    // 如果你想用原版卡框换色，加这两行
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateReplaceHueShaderMaterial(245 / 255f, 162 / 255f, 192 / 255f);

    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    // 卡池是否是无色。例如事件、状态等卡池就是无色的。
    public override bool IsColorless => false;
}