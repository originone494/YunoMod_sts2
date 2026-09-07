using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Tool;

// 生成卡直接加入弃牌堆（CardPileCmd.AddGeneratedCardsToCombat → PileType.Discard）不经过
// CardCmd.DiscardAndDraw，LingHuoDiscardPatch 拦截不到；在此按同样的"先触发灵活、后入堆"流程处理。
// 单数版 AddGeneratedCardToCombat 内部转发到复数版，patch 复数版即可同时覆盖两个入口。
[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat))]
public class LingHuoGeneratedDiscardPatch
{
    [ThreadStatic]
    private static bool _isProcessing;

    public static bool Prefix(IEnumerable<CardModel> cards, PileType newPileType, Player? creator,
        CardPilePosition position, ref Task<IReadOnlyList<CardPileAddResult>> __result)
    {
        // 重入检查：递归调用原版入堆时直接放行
        if (_isProcessing)
            return true;

        if (newPileType != PileType.Discard)
            return true;

        if (CombatManager.Instance == null || !CombatManager.Instance.IsInProgress)
            return true;

        List<CardModel> list = cards.ToList();
        if (list.Count == 0)
            return true;

        if (!list.Any(c => c.Keywords.Contains(YunoKeywords.LingHuo) || c.Tags.Contains(YunoTags.LingHuo)))
            return true;

        PlayerChoiceContext? ctx = ResolveContext();
        if (ctx == null)
            return true;

        _isProcessing = true;
        __result = ProcessLingHuoAsync(ctx, list, creator, position);
        return false;
    }

    private static async Task<IReadOnlyList<CardPileAddResult>> ProcessLingHuoAsync(
        PlayerChoiceContext ctx, List<CardModel> list, Player? creator, CardPilePosition position)
    {
        var results = new List<CardPileAddResult>();
        try
        {
            // 与 LingHuoDiscardPatch 一致：先触发灵活效果（此时卡尚未入堆，仍是"无牌堆"状态，
            // 自动打出/珠泪融合等效果对此有原生兼容），再让未被处理的卡按原版流程入堆
            foreach (CardModel card in list)
            {
                if (!card.Keywords.Contains(YunoKeywords.LingHuo) && !card.Tags.Contains(YunoTags.LingHuo))
                    continue;

                await LingHuoHook.LingHuoSpecial(ctx, card.Owner, card);
                await LingHuoHook.OnLingHuo(ctx, card.Owner);
            }

            // 灵活效果已把卡打出/移走（已有牌堆）或标记处理的卡跳过，不再重复入堆
            var remaining = list
                .Where(c => c.Pile == null && !LingHuoHook.HandledByLingHuo.Contains(c))
                .ToList();
            if (remaining.Count > 0)
                results.AddRange(await CardPileCmd.AddGeneratedCardsToCombat(remaining, PileType.Discard, creator, position));
        }
        finally
        {
            foreach (var card in list)
                LingHuoHook.HandledByLingHuo.Remove(card);
            _isProcessing = false;
        }
        return results;
    }

    // 被 patch 的方法没有 PlayerChoiceContext 参数，从当前运行的 GameAction 还原：
    // 优先复用打牌/用药 action 自带的上下文（与调用方原本会传入的一致），其余 action 现场构造绑定；
    // 拿不到运行中的 action（如战斗外调用）时返回 null，按原版行为处理
    private static PlayerChoiceContext? ResolveContext()
    {
        var action = RunManager.Instance?.ActionExecutor?.CurrentlyRunningAction;
        if (action == null)
            return null;

        if (action is PlayCardAction play)
            return play.PlayerChoiceContext;

        if (action is UsePotionAction potion)
            return potion.PlayerChoiceContext;

        return new GameActionPlayerChoiceContext(action);
    }
}
