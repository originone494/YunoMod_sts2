using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 塔罗遗物基类：战斗开始掷骰逆位，逆位时切换 Reverse 图标与 .reversed 描述/标题
public abstract class TarotRelicBase : YunoBaseRelic
{
    private bool _reversedThisCombat;

    // 本场战斗是否处于逆位
    public bool IsReversed => _reversedThisCombat;

    // 子类实现了逆位效果时置 true，才会参与掷骰与换图
    protected virtual bool SupportsReversed => false;

    // 持有死亡讯息-蓝的玩家才可获得塔罗牌系列遗物（直接发放不受影响）
    public override bool IsAllowed(IRunState runState)
        => base.IsAllowed(runState) && runState.Players.Any(p => p.GetRelic<DeadEndYellowRelic>() != null);

    // 逆位图：文件名前加 Reverse（res://YunoMod/images/relics/Xxx.png → ReverseXxx.png）
    public override string? CustomIconPath => WithReversePrefix(AssetProfile.IconPath);
    public override string? CustomIconOutlinePath => WithReversePrefix(AssetProfile.IconOutlinePath);
    public override string? CustomBigIconPath => WithReversePrefix(AssetProfile.BigIconPath);

    private string? WithReversePrefix(string? path)
    {
        if (!_reversedThisCombat || path == null) return path;
        int index = path.LastIndexOf('/');
        return $"{path[..(index + 1)]}Reverse{path[(index + 1)..]}";
    }

    // 战斗开始掷骰（10%）；翻转 Status 触发 UI 重载纹理（RelicVisualPatch）
    public override Task BeforeCombatStart()
    {
        if (!SupportsReversed || _reversedThisCombat) return Task.CompletedTask;
        if (Owner == null) return Task.CompletedTask;

        if (Owner.RunState.Rng.Niche.NextDouble() < 0.1)
        {
            _reversedThisCombat = true;
            PulseStatus();
        }
        return Task.CompletedTask;
    }

    // 战斗结束恢复正位
    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (_reversedThisCombat)
        {
            _reversedThisCombat = false;
            PulseStatus();
        }
        return Task.CompletedTask;
    }

    // 翻转 Status 两次，各触发一次 StatusChanged → 补丁重载纹理
    private void PulseStatus()
    {
        Status = RelicStatus.Active;
        Status = RelicStatus.Normal;
    }
}
