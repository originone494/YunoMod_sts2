using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace YunoMod.Scripts.Tool;

// 模组配置：开局是否自动发放 DeadEnd 遗物（死亡讯息-红 / 死亡讯息-粉）。
// 只影响新开局的发放；已在局内获得的遗物不会因开关被移除。
public static class YunoStartRelicSettings
{
    /// <summary>配置数据在 DataStore 中的持久化键（需防撞）。</summary>
    public const string DataKey = "start_dead_end_relic";

    public sealed class Model
    {
        public bool GrantDeadEndRed { get; set; } = true;
        public bool GrantDeadEndPink { get; set; } = true;
        public bool GrantAllDiary { get; set; } = false;
        public bool GrantAllTarot { get; set; } = false;
        public bool GrantDeadEndBlue { get; set; } = true;
    }

    public static readonly ModSettingsValueBinding<Model, bool> GrantDeadEndRedBinding = new(
        Entry.ModId, DataKey, SaveScope.Global,
        static m => m.GrantDeadEndRed,
        static (m, v) => m.GrantDeadEndRed = v);

    public static readonly ModSettingsValueBinding<Model, bool> GrantDeadEndPinkBinding = new(
        Entry.ModId, DataKey, SaveScope.Global,
        static m => m.GrantDeadEndPink,
        static (m, v) => m.GrantDeadEndPink = v);

    public static readonly ModSettingsValueBinding<Model, bool> GrantAllDiaryBinding = new(
        Entry.ModId, DataKey, SaveScope.Global,
        static m => m.GrantAllDiary,
        static (m, v) => m.GrantAllDiary = v);

    public static readonly ModSettingsValueBinding<Model, bool> GrantAllTarotBinding = new(
        Entry.ModId, DataKey, SaveScope.Global,
        static m => m.GrantAllTarot,
        static (m, v) => m.GrantAllTarot = v);

    public static readonly ModSettingsValueBinding<Model, bool> GrantDeadEndBlueBinding = new(
        Entry.ModId, DataKey, SaveScope.Global,
        static m => m.GrantDeadEndBlue,
        static (m, v) => m.GrantDeadEndBlue = v);

    public static void Register()
    {
        // 注册 DataStore：配置值持久化到 profile 全局文件。
        ModDataStore.For(Entry.ModId).Register<Model>(
            key: DataKey,
            fileName: "start_dead_end_relic.json",
            scope: SaveScope.Global,
            defaultFactory: () => new Model(),
            autoCreateIfMissing: true);

        // 注册模组设置页 UI（主菜单与局内暂停均可打开；开关改动即时持久化，下一局开局生效）。
        RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page


            .WithVisibleOnHostSurfaces(ModSettingsHostSurface.MainMenu | ModSettingsHostSurface.RunPause)
            .AddSection("start_relics", section => section
                .WithTitle(ModSettingsText.Literal("开局遗物"))
                .AddToggle(
                    "grant_dead_end_pink",
                    ModSettingsText.Literal("开局获得「死亡讯息-粉」"),
                    GrantDeadEndPinkBinding,
                    ModSettingsText.Literal("开启后，每局开局发放「死亡讯息-粉」，首战胜利或击败Boss后获得随机日记。"))
                .AddToggle(
                    "grant_dead_end_red",
                    ModSettingsText.Literal("开局获得「死亡讯息-红」"),
                    GrantDeadEndRedBinding,
                    ModSettingsText.Literal("开启后，每局开局发放「死亡讯息-红」，击败精英后获得特殊卡牌奖励。"))
                .AddToggle(
                "grant_dead_end_blue",
                ModSettingsText.Literal("开局是否获得「死亡讯息-蓝」"),
                GrantDeadEndBlueBinding,
                ModSettingsText.Literal("开启后，每局开局发放「死亡讯息-蓝」。持有该遗物时，塔罗牌系列遗物才会出现在游戏中。"))
                .AddToggle(
                    "grant_all_diary",
                    ModSettingsText.Literal("开局获得所有日记"),
                    GrantAllDiaryBinding,
                    ModSettingsText.Literal("开局获得所有日记"))
                .AddToggle(
                    "grant_all_tarot",
                    ModSettingsText.Literal("开局获得所有塔罗牌系列遗物"),
                    GrantAllTarotBinding,
                    ModSettingsText.Literal("开启后，开局获得所有塔罗牌系列遗物。"))

                ));

    }
}
