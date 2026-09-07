using MegaCrit.Sts2.Core.Entities.Relics;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 死亡讯息-蓝：持有该遗物时，塔罗牌系列遗物才会出现在获取途径中（判定在 TarotRelicBase.IsAllowed）
public class DeadEndYellowRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
