using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Relics;

namespace YunoMod.Scripts.Tool;

// 塔罗遗物的逐玩家获取限制：
// 引擎的 RelicModel.IsAllowed(IRunState) 在共享遗物卡包（多人共用一个 RelicGrabBag）上做全局过滤，
// 拿不到"这次奖励是发给谁的"，因此 IsAllowed 只能决定塔罗遗物是否进池（见 TarotRelicBase）。
// 奖励与商店的抽取最终都汇入 RelicFactory 的 (Player, Rarity, Filter) 核心重载，此处用 Harmony 前缀
// 把「未持死亡讯息-蓝的玩家抽不到塔罗遗物」包进 filter：被过滤的塔罗遗物会留在共享卡包中
// 供持蓝的队友抽取，不会烧池。直接发放（模组设置等）不走工厂，不受影响。
// 判定只依赖同步的玩家状态，所有客户端一致，不影响锁步。
[HarmonyPatch]
public static class TarotRelicPlayerGatePatch
{
    [HarmonyPatch(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromFront),
        new[] { typeof(Player), typeof(RelicRarity), typeof(Func<RelicModel, bool>) })]
    [HarmonyPrefix]
    public static void FrontPullPrefix(Player player, ref Func<RelicModel, bool> filter)
    {
        if (player.GetRelic<DeadEndYellowRelic>() != null) return;

        var original = filter;
        filter = relic => relic is not TarotRelicBase && original(relic);
    }

    [HarmonyPatch(typeof(RelicFactory), nameof(RelicFactory.PullNextRelicFromBack),
        new[] { typeof(Player), typeof(RelicRarity), typeof(Func<RelicModel, bool>) })]
    [HarmonyPrefix]
    public static void BackPullPrefix(Player player, ref Func<RelicModel, bool> filter)
    {
        if (player.GetRelic<DeadEndYellowRelic>() != null) return;

        var original = filter;
        filter = relic => relic is not TarotRelicBase && original(relic);
    }
}
