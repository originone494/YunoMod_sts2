using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Pool;

namespace YunoMod.Scripts.Relics;

// 死亡讯息-紫：获得时，从所有特殊卡中选择 1 张加入牌组。
// 时序说明：开局时 AfterObtained 会被调用两次（RunStartedEvent 里 RelicCmd.Obtain 一次 +
// 游戏 StartRun 中 FinalizeStartingRelics 对所有开局遗物再调一次），因此用标志位保证每局只启动一次。
// 选卡不能在 AfterObtained 里立即执行：
// 1) 此时 NRun 场景尚未创建，且绝不能在 AfterObtained 内 await 等待（第二次调用被游戏 StartRun
//    await，会反过来阻塞场景加载，造成死锁式 60 秒超时）；
// 2) 即使等 NRun 就绪后立即弹选卡 overlay，也会撞上进入开局先古事件房前的 RunManager.ClearScreens()
//    （清空 NOverlayStack），选卡界面 _ExitTree 时取消等待的 Task，异常被 fire-and-forget 链吞掉。
// 因此这里只标记"待选"，由 RunGameAddRelicReward 在第一个 RoomEnteredEvent（房间 UI 已就绪、
// 到下一次进房前不会再 ClearScreens 的稳定窗口）调用 TryRunPendingSelection 完成选卡。
public class DeadEndPurpleRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    private int _selectionPending;

    public override Task AfterObtained()
    {
        // 同一局只标记一次（RelicCmd.Obtain 与 FinalizeStartingRelics 会各调一次）
        Interlocked.Exchange(ref _selectionPending, 1);
        return Task.CompletedTask;
    }

    // 第一个房间进入后由 RunGameAddRelicReward 调用；标志位保证整个流程只执行一次。
    public async Task TryRunPendingSelection()
    {
        if (Interlocked.Exchange(ref _selectionPending, 0) == 0)
        {
            return;
        }

        try
        {
            // 候选 = 特殊卡池全部卡牌（按Id去重），创建为该玩家的运行时副本
            List<CardModel> candidates = ModelDb.AllCards
                .Where(c => c.Pool is YunoSpecialCardPool)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .Select(c => Owner.RunState.CreateCard(c, Owner))
                .ToList();
            Entry.Logger.Info($"[DeadEndPurple] 候选卡数量：{candidates.Count}");
            if (candidates.Count == 0) return;

            CardModel? picked = (await CardSelectCmd.FromSimpleGrid(
                new BlockingPlayerChoiceContext(),
                candidates,
                Owner,
                new CardSelectorPrefs(YunoSelectorPrefs.StartSpecialCardSelectionPrompt, 1, 1))).FirstOrDefault();

            if (picked != null)
            {
                await CardPileCmd.Add(picked, PileType.Deck);
                Entry.Logger.Info($"[DeadEndPurple] 已选卡加入牌组：{picked.Id}");
            }
            else
            {
                Entry.Logger.Info("[DeadEndPurple] 未选择任何卡");
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[DeadEndPurple] 选卡流程异常：{ex}");
        }
    }
}
