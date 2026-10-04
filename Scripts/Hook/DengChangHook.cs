using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Hook;

// 「登场」：卡牌**因效果加入手牌**时触发。
//
// 判定为什么这么绕：原版没有"加入手牌"专用钩子，只有通用的 AfterCardChangedPiles（任何换堆都会触发，
// 新堆看 card.Pile）。而抽牌走的也是同一条入堆路径 —— CardPileCmd.DrawInternal 里就是
// `await Add(card, hand)`（CardPileCmd.cs:1061），紧接着才 Hook.AfterCardDrawn。所以必须把"抽牌"这一类排除，
// 否则每抽一张牌都会触发登场。
//
// 排除方式不能用 oldPileType（抽牌堆 → 手牌 的"检索"效果同样满足），也不能靠"等 AfterCardDrawn 打标记"
// （钩子是 await 的：我们的 AfterCardChangedPiles 不返回，Draw 就不会继续调用 AfterCardDrawn）。
// 因此改用 DrawWindowPatch 给 CardPileCmd.Draw 打一个"抽牌窗口"括号：窗口内的入堆 = 抽牌，窗口外的 = 因效果加入。
public interface IDengChangCard
{
    // 卡自己"后注效果"的登场效果。实现了它就替换掉默认的"将其打出，然后返回手牌"。
    Task DengChangSpecial(PlayerChoiceContext ctx, Player player);
}

public static class DengChangHook
{
    // 防循环②：跨卡连锁上限（A 的登场效果检索 B、B 的登场效果又检索 A …），与 LingHuoHook 同款安全阀
    private const int MaxChainDepth = 32;
    private static int _chainDepth;

    // 防循环①（核心）：正在结算登场的卡实例。
    // "将其打出，然后返回手牌"里的"返回手牌"本身就是一次"因效果加入手牌"，
    // 不屏蔽就会 打出 → 返回手牌 → 再触发 → 再打出 …… 无限循环。
    // 用实例级集合，所以只屏蔽"同一张卡自己正在结算时的再次加入"，
    // 不会误伤另一张同名卡（同名卡的另一个实例照常触发）。
    private static readonly HashSet<CardModel> Resolving = new();

    // 关键字 或 标签 都算（标签用于"卡面不显示登场关键字、只在悬停里给说明"的写法，与「应对」一致）
    public static bool HasDengChang(CardModel card)
        => card.Keywords.Contains(YunoKeywords.DengChang) || card.Tags.Contains(YunoTags.DengChang);

    // 由卡牌基类的 AfterCardChangedPiles 调用（card == 刚换堆的那张卡）
    public static async Task OnCardAddedToHand(CardModel card)
    {
        if (card.Pile?.Type != PileType.Hand) return;
        if (!HasDengChang(card)) return;

        // 抽牌窗口内的入堆 = 抽牌，不算"因效果加入手牌"
        if (DrawWindowPatch.IsInsideDraw(card.Owner)) return;

        await Trigger(card);
    }

    private static async Task Trigger(CardModel card)
    {
        var player = card.Owner;
        if (player is null) return;
        if (CombatManager.Instance.IsOverOrEnding) return;

        // 防循环①：这张卡正在结算登场 → 这次的"加入手牌"就是它自己的返回动作，直接放过
        if (!Resolving.Add(card)) return;

        if (_chainDepth >= MaxChainDepth)
        {
            Resolving.Remove(card);
            Log.Warn($"[YunoMod] 登场连锁已达 {MaxChainDepth} 层，停止继续触发：{card.Id}");
            return;
        }

        _chainDepth++;
        try
        {
            // 登场只触发后注效果；没有后注的卡不做任何事
            if (card is not IDengChangCard self) return;

            // 同名卡一回合一次：所有「登场」效果统一在这里限制（按卡名记，回合刷新）
            string onceKey = PerTurnOnce.Key("DengChang", card.Id.Entry);
            if (PerTurnOnce.IsUsed(player, onceKey)) return;
            PerTurnOnce.Mark(player, onceKey);

            // AfterCardChangedPiles 没有 PlayerChoiceContext，而打出的卡可能弹选择界面，
            // 所以用原版专为这种场合准备的 BlockingPlayerChoiceContext：
            // 它会挡住后续队列直到玩家选完（原版 Hellraiser 把抽到的牌自动打出时就是这么做的）。
            var ctx = new BlockingPlayerChoiceContext();

            await self.DengChangSpecial(ctx, player);
        }
        catch (Exception e)
        {
            Log.Warn($"[YunoMod] 登场结算异常，已跳过本次：{card.Id} {e.Message}");
        }
        finally
        {
            _chainDepth--;
            Resolving.Remove(card);
        }
    }
}
