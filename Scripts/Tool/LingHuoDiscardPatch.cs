
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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

    public static void Postfix(PlayerChoiceContext choiceContext, IEnumerable<CardModel> cardsToDiscard, int cardsToDraw)
    {
        // 重入检查：如果是我们自己调用的 DiscardAndDraw，直接放行
        if (_isProcessing)
            return;

        if (CombatManager.Instance.IsOverOrEnding)
            return;

        List<CardModel> yunoDiscardCards = cardsToDiscard.ToList();
        if (yunoDiscardCards.Count == 0)
            return;

        // 检查是否有灵活关键词的卡牌
        bool hasLingHuo = false;
        foreach (CardModel card in yunoDiscardCards)
        {
            if (card.HasModKeyword(YunoKeywords.LingHuo))
            {
                hasLingHuo = true;
                break;
            }
        }

        if (!hasLingHuo)
            return;

        // 有灵活卡牌：跳过原方法，用我们的 Task 替换返回值，异步处理灵活效果
        _isProcessing = true;
        ProcessLingHuoAsync(choiceContext, yunoDiscardCards, cardsToDraw);
        return;
    }

    private static async void ProcessLingHuoAsync(PlayerChoiceContext choiceContext, List<CardModel> cardsToDiscard, int cardsToDraw)
    {
        try
        {
            foreach (CardModel card in cardsToDiscard)
            {
                if (card.HasModKeyword(YunoKeywords.LingHuo))
                {
                    await LingHuoHook.LingHuoSpecial(choiceContext, card.Owner, card);
                    await LingHuoHook.OnLingHuo(choiceContext, card.Owner);
                }
            }

            // 灵活效果处理完毕
        }
        finally
        {
            _isProcessing = false;  
        }
    }
}
