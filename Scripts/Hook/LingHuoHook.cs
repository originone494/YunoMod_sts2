using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Hook;

// 「灵活」：卡牌**因效果被送入弃牌堆**时触发。
// 触发点用原版唯一的弃牌事件 AfterCardDiscarded（CardCmd.DiscardAndDraw 内部、入堆之后，CardCmd.cs:192-194），
// 因此「回合结束清空手牌」「正常/自动打出后的落堆」「纯搬运（Add 到弃牌堆）」都不会触发。
//
// 两种处理，与关键字文案一一对应：
//   ① 卡自己实现 ILingHuoCard（= 灵活后注有效果）→ 触发那些效果，不打出；
//   ② 没实现（= 灵活后注没有效果）→ 默认把它打出。
public interface ILingHuoCard
{
    // 这张卡自身的灵活效果（由"被丢弃的那张卡"实现）
    Task LingHuoSpecial(PlayerChoiceContext ctx, Player player);
}

// 全局观察者（能力/遗物）：任意一张灵活卡被丢弃时收到通知；trigger 是触发的那张卡，便于按标签筛选
public interface ILingHuoObserver
{
    Task OnLingHuo(PlayerChoiceContext ctx, Player player, CardModel trigger);
}

public static class LingHuoHook
{
    // 连锁深度上限：正常构筑 3~5 层，这里只是防极端循环（A 被丢弃→触发丢弃 B→B 又触发…）的安全阀
    private const int MaxChainDepth = 32;
    private static int _chainDepth;

    // 由卡牌基类的 AfterCardDiscarded 调用，card == 被丢弃的那张卡
    public static async Task OnCardDiscarded(PlayerChoiceContext ctx, CardModel card)
    {
        if (!HasLingHuo(card)) return;

        var player = card.Owner;
        if (player is null) return;
        if (CombatManager.Instance.IsOverOrEnding) return;

        if (_chainDepth >= MaxChainDepth)
        {
            Log.Warn($"[YunoMod] 灵活连锁已达 {MaxChainDepth} 层，停止继续触发：{card.Id}");
            return;
        }

        _chainDepth++;
        try
        {
            if (card is ILingHuoCard self)
            {
                // 「灵活后注有效果」→ 不打出，改为触发那些效果
                await self.LingHuoSpecial(ctx, player);
            }
            else
            {
                // 「灵活后注没有效果」→ 关键字默认行为：把这张卡打出
                // （当前所有灵活卡都实现 ILingHuoCard，所以这条分支暂时不生效，只为将来补上缺口）
                await AutoPlayFromDiscard(ctx, card, null);
            }
            await Broadcast(ctx, player, card);
        }
        finally
        {
            _chainDepth--;
        }
    }

    // 与原实现同判据：带灵活关键字或灵活标签
    public static bool HasLingHuo(CardModel card)
    {
        return card.Keywords.Contains(YunoKeywords.LingHuo) || card.Tags.Contains(YunoTags.LingHuo);
    }

    private static async Task Broadcast(PlayerChoiceContext ctx, Player player, CardModel trigger)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        foreach (var model in combatState.IterateHookListeners().OfType<ILingHuoObserver>().ToList())
        {
            var abstractModel = (AbstractModel)(object)model;
            ctx.PushModel(abstractModel);
            try
            {
                await model.OnLingHuo(ctx, player, trigger);
            }
            finally
            {
                ctx.PopModel(abstractModel);
            }
        }
    }

    // 从弃牌堆打出：AutoPlay 只在 card.Pile == null 时才把牌搬进 Play（CardCmd.cs:114-117），
    // 已在牌堆里的卡必须先自己搬——与官方 CardPileCmd.AutoPlayFromDrawPile 同款做法（CardPileCmd.cs:1171-1178）。
    public static async Task AutoPlayFromDiscard(PlayerChoiceContext ctx, CardModel card, Creature? target, bool exhaustOnPlay = false)
    {
        if (card.Pile?.Type == PileType.Discard)
        {
            await CardPileCmd.Add(card, PileType.Play);
        }
        if (exhaustOnPlay)
        {
            card.ExhaustOnNextPlay = true;
        }
        await CardCmd.AutoPlay(ctx, card, target);
    }
}
