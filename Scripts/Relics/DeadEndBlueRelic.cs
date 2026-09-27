using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 死亡讯息-蓝：获得时，获得所有日记。
public class DeadEndBlueRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        // 获得所有尚未持有的日记
        while (await DiaryRelics.TryGrantRandomUnobtained(Owner)) ;
    }
}
