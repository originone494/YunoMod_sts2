
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Tool;

[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.DiscardAndDraw))]
public class LingHuoDiscardPatch
{
    [ThreadStatic]
    private static bool _isProcessing;

    public static bool Prefix(PlayerChoiceContext choiceContext, IEnumerable<CardModel> cardsToDiscard, int cardsToDraw, ref Task __result)
    {
        // 重入检查：如果是我们自己调用的 DiscardAndDraw，直接放行
        if (_isProcessing)
            return true;

        if (CombatManager.Instance.IsOverOrEnding)
            return true;

        List<CardModel> yunoDiscardCards = cardsToDiscard.ToList();
        if (yunoDiscardCards.Count == 0)
            return true;

        // 检查是否有灵活关键词的卡牌
        bool hasLingHuo = false;
        foreach (CardModel card in yunoDiscardCards)
        {
            if (card.Keywords.Contains(YunoKeywords.LingHuo) || card.Tags.Contains(YunoTags.LingHuo))

            {
                hasLingHuo = true;
                break;
            }
        }

        if (!hasLingHuo)
            return true;

        // 跳过原方法，用我们的 Task 替换返回值：先处理灵活效果，再执行弃牌抽牌
        _isProcessing = true;
        __result = ProcessLingHuoAsync(choiceContext, yunoDiscardCards, cardsToDraw);
        return false;
    }

    private static async Task ProcessLingHuoAsync(PlayerChoiceContext choiceContext, List<CardModel> cardsToDiscard, int cardsToDraw)
    {
        try
        {
            foreach (CardModel card in cardsToDiscard)
            {
                if (card.Keywords.Contains(YunoKeywords.LingHuo) || card.Tags.Contains(YunoTags.LingHuo))
                {
                    await LingHuoHook.LingHuoSpecial(choiceContext, card.Owner, card);
                    await LingHuoHook.OnLingHuo(choiceContext, card.Owner);
                }
            }

            // 灵活效果处理完毕，执行原本的弃牌抽牌（_isProcessing=true 放行）
            // 只弃仍然在手牌/抽牌堆中的卡：已被打出（如灵活打出）或已被灵活效果主动处理（如珠泪融合返回抽牌堆）的卡不再重复弃
            var remaining = cardsToDiscard
                .Where(c => c.Pile?.Type is PileType.Hand or PileType.Draw)
                .Where(c => !LingHuoHook.HandledByLingHuo.Contains(c))
                .ToList();
            if (remaining.Count > 0)
            {
                await CardCmd.DiscardAndDraw(choiceContext, remaining, cardsToDraw);
            }
        }
        finally
        {
            // 清理标记，避免跨战斗泄漏
            foreach (var card in cardsToDiscard)
            {
                LingHuoHook.HandledByLingHuo.Remove(card);
            }
            _isProcessing = false;
        }
    }
}
