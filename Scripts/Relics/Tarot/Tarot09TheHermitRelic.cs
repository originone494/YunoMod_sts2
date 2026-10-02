using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interactions.RightClick;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 右键遗物进入商店（右键接口与无差别日记 RadarDiaryRelic 同款）
// 逆位：失去正位的效果，无法通过该遗物进入商店
public class Tarot09TheHermitRelic : TarotRelicBase, IModRightClickableRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override bool SupportsReversed => true;

    // 仅一次（随存档持久化，读档不会重置）
    private bool _used;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool Used
    {
        get => _used;
        set
        {
            AssertMutable();
            _used = value;
        }
    }

    // 用掉后遗物呈"已耗尽"状态
    public override bool IsUsedUp => Used;

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (Used) return;
        if (IsReversed) return;                        // 逆位：无法进入商店
        // 只有"战斗真正进行中"才禁止切房间。不能用 Creature.CombatState 判断：
        // 战斗结束后（含战斗奖励界面）该字段要等离开战斗房间时才被清空，会把奖励界面上的右键一并挡掉。
        if (CombatManager.Instance.IsInProgress) return;

        Flash();
        Used = true;
        // 必须走 EnterRoomDebug 而不是 EnterRoom：EnterRoom 只做 ExitCurrentRooms + EnterRoomInternal，
        // 不会调用 ClearScreens()，全屏地图界面会继续盖在新生成的商店之上（表现为"进不去商店"）。
        // EnterRoomDebug 会补齐 NetLoadingHandle / ClearScreens / ExitCurrentRooms / 同步 / 淡入淡出。
        await RunManager.Instance.EnterRoomDebug(RoomType.Shop, MapPointType.Shop, null, true);
    }
}
