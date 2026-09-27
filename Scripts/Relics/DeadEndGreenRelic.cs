using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 死亡讯息-绿：获得时，获得所有塔罗牌系列遗物。
public class DeadEndGreenRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        // 获得所有尚未持有的塔罗牌系列遗物
        if (Owner.GetRelic<Tarot00TheFoolRelic>() == null) await RelicCmd.Obtain<Tarot00TheFoolRelic>(Owner);
        if (Owner.GetRelic<Tarot01TheMagicianRelic>() == null) await RelicCmd.Obtain<Tarot01TheMagicianRelic>(Owner);
        if (Owner.GetRelic<Tarot02TheHighPriestessRelic>() == null) await RelicCmd.Obtain<Tarot02TheHighPriestessRelic>(Owner);
        if (Owner.GetRelic<Tarot03TheEmpressRelic>() == null) await RelicCmd.Obtain<Tarot03TheEmpressRelic>(Owner);
        if (Owner.GetRelic<Tarot04TheEmperorRelic>() == null) await RelicCmd.Obtain<Tarot04TheEmperorRelic>(Owner);
        if (Owner.GetRelic<Tarot05TheHierophantRelic>() == null) await RelicCmd.Obtain<Tarot05TheHierophantRelic>(Owner);
        if (Owner.GetRelic<Tarot06TheLoversRelic>() == null) await RelicCmd.Obtain<Tarot06TheLoversRelic>(Owner);
        if (Owner.GetRelic<Tarot07TheChariotRelic>() == null) await RelicCmd.Obtain<Tarot07TheChariotRelic>(Owner);
        if (Owner.GetRelic<Tarot08StrengthRelic>() == null) await RelicCmd.Obtain<Tarot08StrengthRelic>(Owner);
        if (Owner.GetRelic<Tarot09TheHermitRelic>() == null) await RelicCmd.Obtain<Tarot09TheHermitRelic>(Owner);
        if (Owner.GetRelic<Tarot10WheelOfFortuneRelic>() == null) await RelicCmd.Obtain<Tarot10WheelOfFortuneRelic>(Owner);
        if (Owner.GetRelic<Tarot11JusticeRelic>() == null) await RelicCmd.Obtain<Tarot11JusticeRelic>(Owner);
        if (Owner.GetRelic<Tarot12TheHangedManRelic>() == null) await RelicCmd.Obtain<Tarot12TheHangedManRelic>(Owner);
        if (Owner.GetRelic<Tarot13DeathRelic>() == null) await RelicCmd.Obtain<Tarot13DeathRelic>(Owner);
        if (Owner.GetRelic<Tarot14TemperanceRelic>() == null) await RelicCmd.Obtain<Tarot14TemperanceRelic>(Owner);
        if (Owner.GetRelic<Tarot15TheDevilRelic>() == null) await RelicCmd.Obtain<Tarot15TheDevilRelic>(Owner);
        if (Owner.GetRelic<Tarot16TheTowerRelic>() == null) await RelicCmd.Obtain<Tarot16TheTowerRelic>(Owner);
        if (Owner.GetRelic<Tarot17TheStarRelic>() == null) await RelicCmd.Obtain<Tarot17TheStarRelic>(Owner);
        if (Owner.GetRelic<Tarot18TheMoonRelic>() == null) await RelicCmd.Obtain<Tarot18TheMoonRelic>(Owner);
        if (Owner.GetRelic<Tarot19TheSunRelic>() == null) await RelicCmd.Obtain<Tarot19TheSunRelic>(Owner);
        if (Owner.GetRelic<Tarot20JudgementRelic>() == null) await RelicCmd.Obtain<Tarot20JudgementRelic>(Owner);
        if (Owner.GetRelic<Tarot21TheWorldRelic>() == null) await RelicCmd.Obtain<Tarot21TheWorldRelic>(Owner);
    }
}
