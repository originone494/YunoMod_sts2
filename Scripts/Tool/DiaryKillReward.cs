using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib;
using YunoMod.Scripts.Relics;

namespace YunoMod.Scripts.Tool;

// 击败奖励机制：
// 1) 每局第2层（开局后的第一场怪物战斗）胜利时，奖励栏出现1本未持有的随机普通日记；
// 2) 每次击败 Boss 时，同样在奖励栏出现1本未持有的随机普通日记。
// 不会给予先古「跟踪日记（新）」，池子统一来自 DiaryRelics.ObtainableByAncientSearch。
// 以 RelicReward 加入房间额外奖励，玩家可在战斗结束的奖励栏自行决定是否拿取。
public static class DiaryKillReward
{
    public static void Register()
    {
        RitsuLibFramework.SubscribeLifecycle<CombatVictoryEvent>(evt =>
        {
            _ = HandleVictory(evt);
        });
    }

    private static async Task HandleVictory(CombatVictoryEvent evt)
    {
        if (evt.CombatState == null) return;
        if (evt.Room is not CombatRoom combatRoom) return;

        bool isBoss = combatRoom.RoomType == RoomType.Boss;
        // 第2层：开局起点之后进入的第一个地图点，必然是玩家的第一场战斗
        bool isFirstBattle = evt.RunState.TotalFloor == 2;
        if (!isBoss && !isFirstBattle) return;

        foreach (Player player in evt.RunState.Players)
        {
            if (player.GetRelic<DeadEndPinkRelic>() == null) return;

            RelicModel? diary = DiaryRelics.PickRandomUnobtained(player);
            if (diary == null) continue;

            // 持有先古跟踪日记时，不发放原版跟踪日记，改抽其他未持有的日记
            // （反复抽到同款说明只剩它未持有，放弃本次奖励）
            if (diary is SearchDiaryRelic && player.GetRelic<AncientSearchDiaryRelic>() != null)
            {
                for (int i = 0; i < 100 && diary is SearchDiaryRelic; i++)
                {
                    diary = DiaryRelics.PickRandomUnobtained(player);
                }
                if (diary is SearchDiaryRelic) return;
            }

            // 加入战斗结束的奖励栏（与卡牌奖励并列显示，可拿取或跳过）
            combatRoom.AddExtraReward(player, new RelicReward(diary, player));
        }

        await Task.CompletedTask;
    }
}
