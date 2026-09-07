using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
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
    public override RelicRarity Rarity => RelicRarity.Uncommon;

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
        if (IsReversed) return;                           // 逆位：无法进入商店
        if (Owner.Creature.CombatState != null) return;   // 战斗中禁止切房间

        Flash();
        Used = true;
        await RunManager.Instance.EnterRoom(new MerchantRoom());
    }
}
