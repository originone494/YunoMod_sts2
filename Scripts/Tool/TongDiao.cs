using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Cards.Special;
using YunoMod.Scripts.Custom;

namespace YunoMod.Scripts.Tool;

// 「同调」：规则文本见 card_keywords.json 的 YUNO_MOD_KEYWORD_TONG_DIAO（白板龙的时机触发）。
// 分支一：触发卡为非「调整」→ 选手牌 1 张「调整」卡；调整 4 星 → 升龙，7 星 → 超龙。
// 分支二：触发卡为「调整」→ 选手牌 1 张拥有星级的卡；3 星 → 升龙。
// 节奏：弹选择界面之前先等一刀（等前面的打出/伤害动画落地），与珠泪融合同款。
// 产物判定：**先算产物再动卡**——星级组合不成立时什么都不处理，不移动任何卡。
// 关键点：触发卡与所选卡都走 CardCmd.Exhaust 进消耗堆（不进弃牌堆），
// 不经过 AfterCardDiscarded，因此不会触发「灵活」。
public static class TongDiao
{
    // 分支一提示：从手牌选 1 张「调整」卡
    private static LocString TunerChoicePrompt { get; } = new("card_selection", "TO_TONG_DIAO_CHOICE_TUNER");

    // 分支二提示：从手牌选 1 张拥有星级的卡
    private static LocString StarChoicePrompt { get; } = new("card_selection", "TO_TONG_DIAO_CHOICE_STAR");

    private static readonly CardTag[] StarTags =
    [
        YunoTags.SanXing,
        YunoTags.SiXing,
        YunoTags.QiXing,
        YunoTags.ShiXing,
    ];

    // 「同调」前的等待：与珠泪融合一样，等前面的打出/伤害动画走完再弹选择界面，
    // 否则选择界面会在卡还在飞、伤害还没落地时就出现，看起来"进行得太快"。
    // 也顺便给"前面那张卡落堆"留出时间（产物登场要去弃牌堆翻牌）。
    private const float WaitFastSeconds = 0.35f;
    private const float WaitStandardSeconds = 0.8f;

    // 等待只是节奏，绝不能因为它让「同调」本身失效（例如某些 FastMode 取值下
    // Cmd.CustomScaledWait 会抛 ArgumentOutOfRangeException），所以整段包起来。
    private static async Task Pace()
    {
        try
        {
            await Cmd.CustomScaledWait(WaitFastSeconds, WaitStandardSeconds);
        }
        catch (Exception e)
        {
            MegaCrit.Sts2.Core.Logging.Log.Warn($"[YunoMod] 同调等待被跳过：{e.GetType().Name}");
        }
    }

    public static async Task TryTongDiao(PlayerChoiceContext choiceContext, Player player, CardModel source)
    {
        // 等前面的操作走完（打出动画 / 伤害结算），再进入同调的选择
        await Pace();

        bool sourceIsTuner = source.Tags.Contains(YunoTags.TiaoZheng);

        // 候选 = 手牌中**除自身外**的「调整」卡（分支一）/ 拥有星级的卡（分支二）。
        // 自身必须排除：这些卡打出后会返回手牌，否则它会作为自己的搭档出现在选择列表里。
        Func<CardModel, bool> baseFilter = sourceIsTuner
            ? new Func<CardModel, bool>(c => StarTags.Any(c.Tags.Contains))
            : new Func<CardModel, bool>(c => c.Tags.Contains(YunoTags.TiaoZheng));
        Func<CardModel, bool> filter = c => c != source && baseFilter(c);
        if (!PileType.Hand.GetPile(player).Cards.Any(filter)) return;

        // 「可以进行一次」：可选，取消即不同调
        var prefs = new CardSelectorPrefs(sourceIsTuner ? StarChoicePrompt : TunerChoicePrompt, 1, 1) { Cancelable = true };
        CardModel? chosen = (await CardSelectCmd.FromHand(
            prefs: prefs,
            context: choiceContext,
            player: player,
            filter: filter,
            source: source)).FirstOrDefault();
        if (chosen == null) return;

        // 先算产物：星级组合不成立时**什么都不处理**——既不同调，也不消耗任何卡。
        // （不能先消耗再判断，否则玩家选错星级会白白损失两张卡。）
        CardModel? dragon = ResolveDragon(sourceIsTuner, chosen);
        if (dragon == null) return;

        // 将自身和选择的卡消耗（进消耗堆，不触发灵活）
        await ExhaustCard(choiceContext, source);
        await ExhaustCard(choiceContext, chosen);

        var copy = player.Creature.CombatState!.CreateCard(dragon, player);
        await CardPileCmd.Add(copy, PileType.Hand);
    }

    // 星级组合 → 同调产物；组合不成立返回 null（不做任何副作用）
    private static CardModel? ResolveDragon(bool sourceIsTuner, CardModel chosen) => (sourceIsTuner, chosen) switch
    {
        { sourceIsTuner: true } when chosen.Tags.Contains(YunoTags.SanXing)
            => ModelDb.Card<CanHuanShengLongShuangChaTianLongCard>(),
        { sourceIsTuner: false } when chosen.Tags.Contains(YunoTags.SiXing)
            => ModelDb.Card<CanHuanShengLongShuangChaTianLongCard>(),
        { sourceIsTuner: false } when chosen.Tags.Contains(YunoTags.QiXing)
            => ModelDb.Card<CanHuanChaoLongSanJiTianLongCard>(),
        _ => null,
    };

    private static async Task ExhaustCard(PlayerChoiceContext choiceContext, CardModel card)
    {
        await CardCmd.Exhaust(choiceContext, card);
    }
}
