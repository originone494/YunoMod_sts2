using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using YunoMod.Scripts.Relics;

namespace YunoMod.Scripts.Tool;


// 每局开局按模组配置发放 DeadEnd 遗物：
// - 「死亡讯息-红」：击败每层的第一个精英后获得特殊卡奖励（默认发放）
// - 「死亡讯息-粉」：首战胜利/击败Boss后获得随机日记（默认发放）
// - 「死亡讯息-黄」：持有后塔罗牌系列遗物才会出现（默认发放）
// - 「死亡讯息-蓝」：获得时获得所有日记
// - 「死亡讯息-绿」：获得时获得所有塔罗牌系列遗物
// - 「死亡讯息-紫」：获得时从所有特殊卡中选择1张加入牌组
// 各开关均可在模组设置页独立关闭，功能附着在遗物本身上（玩家可见）。
// 模组设置只对本地玩家生效：联机时各客户端仅处理自己的角色，不影响其他玩家。
public static class RunGameAddRelicReward
{
    public static void Register()
    {
        RitsuLibFramework.SubscribeLifecycle<RunStartedEvent>(evt =>
        {
            _ = GrantStartRelics(evt);
        });

        // 「死亡讯息-紫」的选卡需要稳定的房间 UI（开局时弹 overlay 会被进先古事件房前的
        // ClearScreens 清掉），等第一个房间进入完成后再执行。
        RitsuLibFramework.SubscribeLifecycle<RoomEnteredEvent>(evt =>
        {
            _ = HandlePurplePendingSelection(evt);
        });
    }

    private static async Task HandlePurplePendingSelection(RoomEnteredEvent evt)
    {
        // 只处理本地玩家（单人局即唯一玩家），与 GrantStartRelics 的匹配方式一致
        var playerList = evt.RunState.Players.ToList();
        ulong localNetId = RunManager.Instance.NetService?.NetId ?? 0;
        Player? me = playerList.FirstOrDefault(p => p.NetId == localNetId) ?? playerList.FirstOrDefault();
        if (me == null)
        {
            return;
        }

        if (me.GetRelic<DeadEndPurpleRelic>() is not { } purpleRelic)
        {
            return;
        }

        await purpleRelic.TryRunPendingSelection();
    }

    private static async Task GrantStartRelics(RunStartedEvent evt)
    {
        bool grantRed = YunoStartRelicSettings.GrantDeadEndRedBinding.Read();
        bool grantPink = YunoStartRelicSettings.GrantDeadEndPinkBinding.Read();
        bool grantBlue = YunoStartRelicSettings.GrantAllDiaryBinding.Read();
        bool grantGreen = YunoStartRelicSettings.GrantAllTarotBinding.Read();
        bool grantYellow = YunoStartRelicSettings.GrantDeadEndBlueBinding.Read();
        bool grantPurple = YunoStartRelicSettings.GrantStartSpecialCardBinding.Read();

        // 只处理本地玩家（单人局即唯一玩家）。
        // 注意时序：RunStartedEvent 触发于 RunManager.InitializeNewRun，此时 LocalContext.NetId
        // 尚未赋值（RunState.Launch 才赋值），LocalContext.GetMe 会返回 null 并静默跳过发放。
        // 因此优先用 NetService 的 NetId 匹配本地玩家，匹配不到时回退为唯一玩家（单人局）。
        var playerList = evt.RunState.Players.ToList();
        ulong localNetId = RunManager.Instance.NetService?.NetId ?? 0;
        Player? me = playerList.FirstOrDefault(p => p.NetId == localNetId) ?? playerList.FirstOrDefault();
        if (me == null)
        {
            Entry.Logger.Error("[GrantStartRelics] 未找到本地玩家，放弃发放");
            return;
        }

        async Task GrantChecked<T>(bool on, Func<RelicModel?> has) where T : RelicModel
        {
            if (!on) return;
            if (has() != null) return;
            Entry.Logger.Info($"[GrantStartRelics] 发放 {typeof(T).Name}");
            await RelicCmd.Obtain<T>(me);
        }

        await GrantChecked<DeadEndRedRelic>(grantRed, () => me.GetRelic<DeadEndRedRelic>());
        await GrantChecked<DeadEndPinkRelic>(grantPink, () => me.GetRelic<DeadEndPinkRelic>());
        await GrantChecked<DeadEndYellowRelic>(grantYellow, () => me.GetRelic<DeadEndYellowRelic>());
        await GrantChecked<DeadEndBlueRelic>(grantBlue, () => me.GetRelic<DeadEndBlueRelic>());
        await GrantChecked<DeadEndGreenRelic>(grantGreen, () => me.GetRelic<DeadEndGreenRelic>());
        await GrantChecked<DeadEndPurpleRelic>(grantPurple, () => me.GetRelic<DeadEndPurpleRelic>());

        Entry.Logger.Info("[GrantStartRelics] 开局发放完成");

        await Task.CompletedTask;
    }
}
