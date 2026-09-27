using System.Collections.Generic;
using System.Linq;
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
// 选卡界面依赖 NOverlayStack.Instance -> NRun.Instance，而获得发生在运行界面加载前，
// 需先等待运行界面就绪（与原开局选特殊卡功能同款时序处理）。
public class DeadEndPurpleRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        // 等待运行界面加载（最多约60秒，异常情况下放弃，避免死等）
        Entry.Logger.Info("[DeadEndPurple] AfterObtained：等待运行界面加载…");
        for (int i = 0; i < 600 && NRun.Instance == null; i++)
        {
            await Task.Delay(100);
        }
        if (NRun.Instance == null)
        {
            Entry.Logger.Error("[DeadEndPurple] 等待运行界面超时，放弃选卡");
            return;
        }

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
}
