using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Ui.Toast;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Relics;

public class RadarDiaryRelic : YunoBaseRelic, IModRightClickableRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    private bool _isRighted = false;


    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }


    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }

    // 右键遗物：使玩家回到1层
    // 注意：EnterAct 仅在点击者的客户端执行，多人下的同步行为未经验证（塔罗09-隐者的 EnterRoomDebug
    // 才是 RitsuLib 文档化带同步的切房接口），联机使用前需实测。
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (_isRighted) return;
        if (CombatManager.Instance.IsInProgress) return;

        _isRighted = true;
        Entry.Logger.Info($"[RadarDiary] 右键触发：回到第1层（当前 TotalFloor={Owner.RunState.TotalFloor}, Act={Owner.RunState.CurrentActIndex}）");
        await RunManager.Instance.EnterAct(0);

        // EnterAct(0) 只重置 CurrentActIndex/地图，不回滚章节切换同步器的
        // _lastTransitioningActIndex（首次过Boss后为 0）。二次通关第一章时
        // 防重复推进保护会拦截切换（"we last transitioned from 0. Ignoring"），
        // 导致奖励后续接流程卡死。此处手动回滚该记录，恢复初始值。
        var synchronizer = RunManager.Instance.ActChangeSynchronizer;
        if (synchronizer != null)
        {
            Traverse.Create(synchronizer).Field("_lastTransitioningActIndex").SetValue(-1);
        }
        Entry.Logger.Info($"[RadarDiary] EnterAct(0) 完成：Act={Owner.RunState.CurrentActIndex}, TotalFloor={Owner.RunState.TotalFloor}");
    }
}
