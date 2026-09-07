using HarmonyLib;
using STS2RitsuLib;
using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using YunoMod.Scripts.Cards.Attack;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Relics;
using Godot.Bridge;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    // 你的modid
    public const string ModId = "YunoMod";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {

        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<YanHuSheJiCard, AncientYanHuSheJiCard>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<SearchDiaryRelic, AncientSearchDiaryRelic>();

        // [RegisterSharedCardPool] 只会把 Special 卡池加入 ModelDb.AllCardPools（解决序列化/领取）。
        // 它默认不会出现在卡牌图鉴里，必须额外注册图鉴过滤器，特殊卡才能在图鉴中被浏览。
        ModContentRegistry.For(ModId)
            .RegisterCardLibraryCompendiumSharedPoolFilter<YunoSpecialCardPool>(
                "yuno_special_card_pool",                            // 图鉴过滤器 ID
                "res://yuno.png"            // 图鉴图标
                // null // 放置顺序（可选）
            );

        // 击败精英时 Special 卡 3 选 1 奖励
        EliteSpecialReward.Register();
        // 首次战斗（第2层）与击败 Boss 时随机补1本日记
        DiaryKillReward.Register();
        // 模组设置页：开局是否发放 DeadEnd 遗物（死亡讯息-红/粉）
        YunoStartRelicSettings.Register();
        RunGameAddRelicReward.Register();

        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);


        var harmony = new Harmony("com.YunoMod.patch");
        harmony.PatchAll();
    }
}
