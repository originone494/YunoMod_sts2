using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Scripts.Pool;

namespace YunoMod.Scripts.Base;

[RegisterCard(typeof(YunoCardPool), Inherit = true)]
public abstract class AbstractTemplateBaseCard(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary = true) : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://YunoMod/images/cards/{GetType().Name}.png"
    // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。

    // FramePath: type switch
    //{
    //  CardType.Attack => "res://RitsuTest/images/card_frame_attack.png",
    //CardType.Skill => "res://RitsuTest/images/card_frame_skill.png",
    //CardType.Power => "res://RitsuTest/images/card_frame_power.png",
    //_ => ""
    //}

    // 如果使用自定义卡池，需要改下material（TODO）
    // FramePath: "", // 卡牌背景
    // PortraitBorderPath: "", // 边框（状态牌感染使用的）
    // BannerTexturePath: "" // 横幅（不同类型）
    );
}