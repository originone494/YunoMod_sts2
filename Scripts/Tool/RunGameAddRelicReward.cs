using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib;
using YunoMod.Scripts.Relics;

namespace YunoMod.Scripts.Tool;


// 每局开局按模组配置发放 DeadEnd 遗物：
// - 「死亡讯息-红」：击败每层的第一个精英后获得特殊卡奖励（默认发放）
// - 「死亡讯息-粉」：首战胜利/击败Boss后获得随机日记（默认发放）
// 两个开关均可在模组设置页独立关闭。
public static class RunGameAddRelicReward
{
    public static void Register()
    {
        RitsuLibFramework.SubscribeLifecycle<RunStartedEvent>(evt =>
        {
            _ = GrantStartRelics(evt);
        });
    }

    private static async Task GrantStartRelics(RunStartedEvent evt)
    {
        bool grantRed = YunoStartRelicSettings.GrantDeadEndRedBinding.Read();
        bool grantPink = YunoStartRelicSettings.GrantDeadEndPinkBinding.Read();
        bool grantAll = YunoStartRelicSettings.GrantAllDiaryBinding.Read();
        bool grantTarot = YunoStartRelicSettings.GrantAllTarotBinding.Read();
        bool grantBlue = YunoStartRelicSettings.GrantDeadEndBlueBinding.Read();


        foreach (Player player in evt.RunState.Players)   // 所有角色（联机时每人都发）
        {
            if (grantRed && player.GetRelic<DeadEndRedRelic>() == null)
                await RelicCmd.Obtain<DeadEndRedRelic>(player);

            if (grantPink && player.GetRelic<DeadEndPinkRelic>() == null)
                await RelicCmd.Obtain<DeadEndPinkRelic>(player);

            if (grantBlue && player.GetRelic<DeadEndYellowRelic>() == null)
                await RelicCmd.Obtain<DeadEndYellowRelic>(player);

            if (grantAll)
            {
                while (await DiaryRelics.TryGrantRandomUnobtained(player)) ;
            }

            if (grantTarot)
            {
                if (player.GetRelic<Tarot00TheFoolRelic>() == null) await RelicCmd.Obtain<Tarot00TheFoolRelic>(player);
                if (player.GetRelic<Tarot01TheMagicianRelic>() == null) await RelicCmd.Obtain<Tarot01TheMagicianRelic>(player);
                if (player.GetRelic<Tarot02TheHighPriestessRelic>() == null) await RelicCmd.Obtain<Tarot02TheHighPriestessRelic>(player);
                if (player.GetRelic<Tarot03TheEmpressRelic>() == null) await RelicCmd.Obtain<Tarot03TheEmpressRelic>(player);
                if (player.GetRelic<Tarot04TheEmperorRelic>() == null) await RelicCmd.Obtain<Tarot04TheEmperorRelic>(player);
                if (player.GetRelic<Tarot05TheHierophantRelic>() == null) await RelicCmd.Obtain<Tarot05TheHierophantRelic>(player);
                if (player.GetRelic<Tarot06TheLoversRelic>() == null) await RelicCmd.Obtain<Tarot06TheLoversRelic>(player);
                if (player.GetRelic<Tarot07TheChariotRelic>() == null) await RelicCmd.Obtain<Tarot07TheChariotRelic>(player);
                if (player.GetRelic<Tarot08StrengthRelic>() == null) await RelicCmd.Obtain<Tarot08StrengthRelic>(player);
                if (player.GetRelic<Tarot09TheHermitRelic>() == null) await RelicCmd.Obtain<Tarot09TheHermitRelic>(player);
                if (player.GetRelic<Tarot10WheelOfFortuneRelic>() == null) await RelicCmd.Obtain<Tarot10WheelOfFortuneRelic>(player);
                if (player.GetRelic<Tarot11JusticeRelic>() == null) await RelicCmd.Obtain<Tarot11JusticeRelic>(player);
                if (player.GetRelic<Tarot12TheHangedManRelic>() == null) await RelicCmd.Obtain<Tarot12TheHangedManRelic>(player);
                if (player.GetRelic<Tarot13DeathRelic>() == null) await RelicCmd.Obtain<Tarot13DeathRelic>(player);
                if (player.GetRelic<Tarot14TemperanceRelic>() == null) await RelicCmd.Obtain<Tarot14TemperanceRelic>(player);
                if (player.GetRelic<Tarot15TheDevilRelic>() == null) await RelicCmd.Obtain<Tarot15TheDevilRelic>(player);
                if (player.GetRelic<Tarot16TheTowerRelic>() == null) await RelicCmd.Obtain<Tarot16TheTowerRelic>(player);
                if (player.GetRelic<Tarot17TheStarRelic>() == null) await RelicCmd.Obtain<Tarot17TheStarRelic>(player);
                if (player.GetRelic<Tarot18TheMoonRelic>() == null) await RelicCmd.Obtain<Tarot18TheMoonRelic>(player);
                if (player.GetRelic<Tarot19TheSunRelic>() == null) await RelicCmd.Obtain<Tarot19TheSunRelic>(player);
                if (player.GetRelic<Tarot20JudgementRelic>() == null) await RelicCmd.Obtain<Tarot20JudgementRelic>(player);
                if (player.GetRelic<Tarot21TheWorldRelic>() == null) await RelicCmd.Obtain<Tarot21TheWorldRelic>(player);
            }
        }

        await Task.CompletedTask;
    }
}
