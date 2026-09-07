using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using YunoMod.Scripts.Relics;

namespace YunoMod.Scripts.Tool;

// 遗物视觉补丁：
// 1) 遗物状态变化时重载纹理，使 CustomIconPath 的动态路径（逆位换图）能实时生效
// 2) 塔罗遗物逆位时，把悬浮描述/标题切换为 .reversed.* 本地化 key
public class RelicVisualPatch
{
    // ---- 图标刷新：NRelicInventoryHolder.OnStatusChanged 之后补调 NRelic.Reload() ----
    [HarmonyPatch(typeof(NRelicInventoryHolder), "OnStatusChanged")]
    public class RelicIconRefreshPatch
    {
        public static void Postfix(NRelicInventoryHolder __instance)
        {
            var relic = __instance.Relic;
            if (relic == null) return;
            AccessTools.Method(typeof(NRelic), "Reload")?.Invoke(relic, null);
        }
    }

    // ---- 描述切换：逆位时读 .reversed.description ----
    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.DynamicDescription), MethodType.Getter)]
    public class RelicReversedDescriptionPatch
    {
        public static void Postfix(RelicModel __instance, ref LocString __result)
        {
            if (__instance is TarotRelicBase { IsReversed: true })
            {
                __result = new LocString("relics", __instance.Id.Entry + ".reversed.description");
            }
        }
    }

    // ---- 标题切换：逆位时读 .reversed.title ----
    [HarmonyPatch(typeof(RelicModel), "Title", MethodType.Getter)]
    public class RelicReversedTitlePatch
    {
        public static void Postfix(RelicModel __instance, ref LocString __result)
        {
            if (__instance is TarotRelicBase { IsReversed: true })
            {
                __result = new LocString("relics", __instance.Id.Entry + ".reversed.title");
            }
        }
    }
}
