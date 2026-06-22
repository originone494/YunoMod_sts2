using HarmonyLib;
using STS2RitsuLib;
using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib.Interop;
using YunoMod.Scripts.Cards.Attack;
using YunoMod.Scripts.Relics;
using Godot.Bridge;

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

        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);


        var harmony = new Harmony("com.YunoMod.patch");
        harmony.PatchAll();
    }
}
