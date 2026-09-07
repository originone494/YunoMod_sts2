using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Tool;

// 读档修复（SL 兼容）：
// 击败精英加入的特殊卡奖励是「手动塞卡」的 CardReward（卡为玩家可变实例）。SL 读档时，
// RitsuLib 的 RewardFromSerializableExtPatch 只按卡牌 ID 从 ModelDb 取回 canonical 实例重建奖励，
// 这些卡不可变、未归属玩家，点击领取时 CardPileCmd.Add 抛 CanonicalModelException，
// 表现为「SL 后特殊卡奖励无法选择」。本补丁在读档重建之后把这些 canonical 卡替换为
// 玩家拥有的可变实例（与原版 CardFactory 生成奖励卡完全一致），保证 SL 前后行为一致。
[HarmonyPatch(typeof(Reward), nameof(Reward.FromSerializable))]
public static class SpecialCardRewardLoadPatch
{
    public static void Postfix(ref Reward __result, Player player)
    {
        if (__result is not CardReward cardReward)
        {
            return;
        }

        List<CardModel> cards = cardReward.Cards.ToList();
        if (cards.Count == 0)
        {
            return;
        }

        // 只处理本模组的特殊卡奖励：原版/其他读档场景不会出现带 YunoSpecialBaseCard 的 CardReward。
        if (!cards.Any(c => c is YunoSpecialBaseCard))
        {
            return;
        }

        // 防御：理论上重建后必为 canonical；若已是可变实例则无需处理。
        if (cards.All(c => c.IsMutable))
        {
            return;
        }

        List<CardModel> restored = new(cards.Count);
        foreach (CardModel card in cards)
        {
            // canonical → 玩家拥有的可变实例（ToMutable + 登记进 RunState + AfterCreated）。
            if (card.IsMutable)
            {
                restored.Add(card);
                continue;
            }
            CardModel mutableCard = player.RunState.CreateCard(card, player);
            // 与击杀时的发放一致：特殊卡奖励以「已升级(+1)」形态发放。RitsuLib 的旁路存档只记录卡牌
            // ID（升级等级不落盘），所以读档重建后需手动补回升级；与 Reward.FromSerializable 内部对
            // 升级等级的重放方式一致（UpgradeInternal + FinalizeUpgradeInternal）。
            if (mutableCard.IsUpgradable)
            {
                mutableCard.UpgradeInternal();
                mutableCard.FinalizeUpgradeInternal();
            }
            restored.Add(mutableCard);
        }

        // 用与原版「指定卡牌」构造器相同的参数重建奖励（等价于击杀时的初始创建）。
        __result = new CardReward(
            restored,
            CardCreationSource.Other,
            player,
            CardCreationOptions.ForRoom(player, RoomType.Elite));
    }
}
