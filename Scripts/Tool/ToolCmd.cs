using System.Globalization;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.GameInfo.Objects;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Tool;

public static class ToolCmd
{

    public static async Task AddCardToDeck<T>(Player player, int amount = 1) where T : CardModel
    {
        if (player == null || amount < 1) return;

        var resultList = new List<CardPileAddResult>();

        for (int i = 0; i < amount; i++)
        {
            CardModel card = player.RunState.CreateCard<T>(player);

            var addResult = await CardPileCmd.Add(card, PileType.Deck);
            resultList.Add(addResult);
        }
        CardCmd.PreviewCardPileAdd(resultList, 2f);

    }

    // 向消耗堆加入 amount 张随机卡（全卡池完全随机，各抽各的），并把加入的卡展示给玩家：
    // 卡从屏幕中央出现、停留 previewTime 秒后飞向消耗堆（与 DuiMu / AddCardToDeck 同款表现）。
    // filter 可选：进一步限定候选卡（如仅稀有卡）；为 null 时不限。
    public static async Task<IReadOnlyList<CardPileAddResult>> AddRandomCardsToExhaust(Player player, int amount, float previewTime = 2f, Func<CardModel, bool>? filter = null)
    {
        if (amount <= 0) return Array.Empty<CardPileAddResult>();

        var combatState = player.Creature.CombatState;
        if (combatState == null) return Array.Empty<CardPileAddResult>();

        var candidates = ModelDb.AllCards;
        if (filter != null) candidates = candidates.Where(filter);
        var candidateList = candidates.ToList();
        if (candidateList.Count == 0) return Array.Empty<CardPileAddResult>();

        var copies = new List<CardModel>();
        for (int i = 0; i < amount; i++)
        {
            var randomCanonical = player.RunState.Rng.Niche.NextItem(candidateList);
            if (randomCanonical == null) continue;
            copies.Add(combatState.CreateCard(randomCanonical, player));
        }
        if (copies.Count == 0) return Array.Empty<CardPileAddResult>();

        var results = await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Exhaust, player);

        PreviewPileAdd(results, previewTime);

        return results;
    }

    // 播放"卡入堆"预览：卡从屏幕中央出现、停留 time 秒后飞向目标牌堆（引擎自带的 PreviewCardPileAdd）。
    // 超过5张时横排会铺出屏幕，故改用凌乱布局。
    private static void PreviewPileAdd(IReadOnlyList<CardPileAddResult> results, float time)
    {
        if (results.Count == 0) return;

        var style = results.Count > 5 ? CardPreviewStyle.MessyLayout : CardPreviewStyle.HorizontalLayout;
        CardCmd.PreviewCardPileAdd(results, time, style);
    }

    public static async Task<IEnumerable<CardModel>> Foresee(PlayerChoiceContext choiceContext, Player player, int amount, CardModel? source = null)
    {
        if (amount <= 0) return Array.Empty<CardModel>(); ;

        var drawPile = PileType.Draw.GetPile(player);

        if (drawPile.Cards.Count == 0)
        {
            await CardPileCmd.ShuffleIfNecessary(choiceContext, player);
            drawPile = PileType.Draw.GetPile(player);
        }

        var cardsToScry = drawPile.Cards.Take(amount).ToList();


        if (cardsToScry.Count == 0) return Array.Empty<CardModel>();
        // 预视提示：source 非空时用带卡名的 mod 提示（文本含 {CardName}），否则用通用提示
        LocString foreseePrompt = YunoSelectorPrefs.ForeseeSelectionPrompt;
        if (source != null)
        {
            foreseePrompt = YunoSpecialBaseCard.ForeseeNamedPrompt;
            foreseePrompt.Add("CardName", source.Title);
        }
        var prefs = new CardSelectorPrefs(
            foreseePrompt,
            1,
            1
        );

        var selectedCards = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cardsToScry,
            player,
            prefs
        )).ToList();

        List<CardModel> result = new List<CardModel>();


        // 选中的1张加入手牌
        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
            result.Add(card);
        }

        // 剩余牌送入弃牌堆
        foreach (var card in cardsToScry)
        {
            if (!selectedCards.Contains(card))
            {
                await CardCmd.Discard(choiceContext, card);
            }
        }

        await ForeseeHook.OnForesee(choiceContext, player, amount, cardsToScry.Count - selectedCards.Count);

        return result;
    }

    public static async Task<IEnumerable<CardModel>> ForeseeAndDraw(PlayerChoiceContext choiceContext, Player player, int ForeseeAmount = 5, int DrawAmount = 0, CardModel? source = null)
    {
        return await Foresee(choiceContext, player, ForeseeAmount, source: source);
    }

    public static async Task GainLovePower(PlayerChoiceContext choiceContext, Player player, CardModel source, int amount)
    {
        await PowerCmd.Apply<LovePower>(choiceContext, player.Creature, amount, player.Creature, source);
        await LovePowerHook.OnGetLove(choiceContext, player, amount);
    }

    public static async Task<Stance> ExitAllStance(Player player)
    {
        Stance stance = Stance.Not;
        if (player.Creature.HasPower<DaggerPower>())
        {
            stance = Stance.Dagger;
            await PowerCmd.Remove<DaggerPower>(player.Creature);
        }

        if (player.Creature.HasPower<AxePower>())
        {
            stance = Stance.Axe;
            await PowerCmd.Remove<AxePower>(player.Creature);
        }

        if (player.Creature.HasPower<GunPower>())
        {
            stance = Stance.Gun;
            await PowerCmd.Remove<GunPower>(player.Creature);
        }

        if (player.Creature.HasPower<SwordPower>())
        {
            stance = Stance.SWord;
            await PowerCmd.Remove<SwordPower>(player.Creature);
        }

        await Cmd.CustomScaledWait(0.1f, 0.25f);

        return stance;
    }

    public static async Task DaggerStance(PlayerChoiceContext choiceContext, Player player, CardModel cardSource)
    {
        if (!player.Creature.HasPower<DaggerPower>())
        {
            Stance stance = await ExitAllStance(player);
            await PowerCmd.Apply<DaggerPower>(choiceContext, player.Creature, 1, player.Creature, cardSource);
            await StanceHook.OnStanceChange(choiceContext, player, stance, Stance.Dagger);
        }
        else
        {
            await PowerCmd.Apply<DaggerPower>(choiceContext, player.Creature, 1, player.Creature, cardSource);
        }
    }

    public static async Task AxeStance(PlayerChoiceContext choiceContext, Player player, CardModel cardSource)
    {
        if (!player.Creature.HasPower<AxePower>())
        {
            Stance stance = await ExitAllStance(player);
            await PowerCmd.Apply<AxePower>(choiceContext, player.Creature, 1, player.Creature, cardSource);
            await StanceHook.OnStanceChange(choiceContext, player, stance, Stance.Axe);
        }
        else
        {
            await PowerCmd.Apply<AxePower>(choiceContext, player.Creature, 1, player.Creature, cardSource);
        }

        var resultList = new List<CardPileAddResult>();
        CardModel card = player.Creature.CombatState!.CreateCard<YaZhiCard>(player);
        var addResult = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, player);
        resultList.Add(addResult);
        CardCmd.PreviewCardPileAdd(resultList, 2f);
    }

    public static async Task GunStance(PlayerChoiceContext choiceContext, Player player, CardModel cardSource)
    {
        if (!player.Creature.HasPower<GunPower>())
        {
            Stance stance = await ExitAllStance(player);
            await PowerCmd.Apply<GunPower>(choiceContext, player.Creature, 1, player.Creature, cardSource);
            await StanceHook.OnStanceChange(choiceContext, player, stance, Stance.Gun);
        }
        else
        {
            await PowerCmd.Apply<GunPower>(choiceContext, player.Creature, 1, player.Creature, cardSource);
        }
    }

    public static async Task SwordStance(PlayerChoiceContext choiceContext, Player player, CardModel cardSource)
    {
        if (!player.Creature.HasPower<SwordPower>())
        {
            Stance stance = await ExitAllStance(player);
            await PowerCmd.Apply<SwordPower>(choiceContext, player.Creature, 1, player.Creature, cardSource);
            await StanceHook.OnStanceChange(choiceContext, player, stance, Stance.SWord);
        }
    }

    public static async Task<AttackCommand> DaggerAttack(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource, decimal damage, CardPlay cardPlay, int repeat = 1)
    {
        var cmd = await DamageCmd.Attack(damage)
        .FromCard(cardSource, cardPlay)
        .Targeting(target)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);

        for (int i = 0; i < repeat; i++)
            await PowerCmd.Apply<LiuXuePower>(choiceContext, target, 1, cardSource.Owner.Creature, cardSource);


        return cmd;
    }



    public static async Task<AttackCommand> DaggerAttackAllEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, CardPlay cardPlay, int repeat = 1)
    {
        var cmd = await DamageCmd.Attack(damage)
        .FromCard(cardSource, cardPlay)
        .TargetingAllOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);

        for (int i = 0; i < repeat; i++)
            foreach (Creature enemy in cardSource.Owner.Creature.CombatState!.HittableEnemies)
                await PowerCmd.Apply<LiuXuePower>(choiceContext, enemy, 1, cardSource.Owner.Creature, cardSource);

        return cmd;
    }


    public static async Task<AttackCommand> GunAttack(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource, decimal damage, CardPlay cardPlay, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource, cardPlay)
        .Targeting(target)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }


    public static async Task<AttackCommand> GunAttackAllEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, CardPlay cardPlay, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource, cardPlay)
        .TargetingAllOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }

    public static async Task<AttackCommand> GunAttackRandomEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, CardPlay cardPlay, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource, cardPlay)
        .TargetingRandomOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }

    public static async Task<AttackCommand> AxeAttack(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource, decimal damage, CardPlay cardPlay, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
       .FromCard(cardSource, cardPlay)
       .Targeting(target)
       .WithHitCount(repeat)
       .WithHitFx("vfx/vfx_attack_blunt")
       .Execute(choiceContext);
    }

    public static async Task<AttackCommand> AxeAttackAllEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, CardPlay cardPlay, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource, cardPlay)
        .TargetingAllOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }

    /// <summary>
    /// 通用「检索」：以全卡池（ModelDb.AllCards）为候选源，同时按卡条件
    /// <paramref name="filter"/> 与卡池条件 <paramref name="poolFilter"/> 筛选候选卡，
    /// 生成战斗副本后让玩家从选择网格中最多选 <paramref name="amount"/> 张加入手牌。
    /// 不同检索效果只需提供各自的卡条件与卡池条件即可复用本方法。
    /// </summary>
    /// <param name="choiceContext">选择上下文。</param>
    /// <param name="player">目标玩家。</param>
    /// <param name="filter">卡条件（如标签、稀有度、排除自身）。</param>
    /// <param name="poolFilter">卡池条件（如限定某个卡池）；为 null 时不限卡池。</param>
    /// <param name="amount">最多可选择并加入手牌的数量。</param>
    /// <param name="isRandom">true = 不弹选择界面，从候选中不重复地随机抽最多 amount 张。</param>
    /// <param name="prompt">自定义提示；为 null 时用通用检索提示（source 非空时自动带卡名前缀）。</param>
    /// <param name="source">触发检索的卡（用于提示文本的 {CardName} 卡名前缀）。</param>
    public static async Task<List<CardModel>> RetrieverCard(
        PlayerChoiceContext choiceContext,
        Player player,
        Func<CardModel, bool> filter,
        Func<CardPoolModel, bool>? poolFilter = null,
        int amount = 1, bool isDiscard = false, bool isRandom = false, LocString? prompt = null,
        CardModel? source = null)
    {
        if (player == null || amount < 1) return [];

        // 候选 = 全卡池，同时满足卡条件与卡池条件（c.Pool 为该卡所属卡池）
        var cards = ModelDb.AllCards
            .Where(filter)
            .Where(c => poolFilter == null || (c.Pool != null && poolFilter(c.Pool)))
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();
        if (cards.Count == 0) return [];

        List<CardModel> combatCopies = cards
            .Select(c => player.Creature.CombatState!.CreateCard(c, player))
            .ToList();

        List<CardModel> selectCards;
        if (isRandom)
        {
            // 随机抽取：从候选中不重复地随机取最多 amount 张，不弹选择界面
            selectCards = new List<CardModel>();
            for (int i = 0; i < amount && combatCopies.Count > 0; i++)
            {
                var picked = player.RunState.Rng.Niche.NextItem(combatCopies)!;
                combatCopies.Remove(picked);
                selectCards.Add(picked);
            }
        }
        else
        {
            // 提示文本：调用方自带 > 带卡名的 mod 检索提示（source 非空）> 通用检索提示
            var usedPrompt = prompt ?? (source != null
                ? YunoSpecialBaseCard.RetrieveNamedPrompt(isDiscard)
                : YunoSelectorPrefs.RetrieverSelectionPrompt);
            if (source != null) usedPrompt.Add("CardName", source.Title);

            var prefs = new CardSelectorPrefs(
                usedPrompt,
                0,
                amount
            );

            selectCards = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                combatCopies,
                player,
                prefs
            )).ToList();
        }
        if (isDiscard)
        {
            // 「检索并丢弃」要同时满足两件事：① 保留"生成卡"记录；② 走真正的弃牌语义（触发灵活等弃牌相关效果）。
            // 做法：先按生成卡入堆（入的是弃牌堆），再手动结算"被丢弃"（见下面的注释，不用 CardCmd.Discard，
            // 否则会多出一次"弃牌堆 → 弃牌堆"的入堆和它的重复特效）。
            foreach (var card in selectCards)
            {
                var addResult = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, player);
                if (!addResult.success) continue;

                // 弃牌特效：原版对"生成卡进弃牌堆"统一用 CardCmd.PreviewCardPileAdd
                //（Anger / Turbo / Overclock / GunkUp / FightThrough 等 9 张原版卡都是这么做的）——
                // 卡从屏幕中央出现、停留片刻、带拖尾飞进弃牌堆。
                // 那一步之所以必须补，是因为 AddGeneratedCardToCombat 对"刚生成 + 目标是弃牌堆"的卡
                // 在 CardPileCmd.GetTweenForCardsChangingPiles 里既不建 NCard 节点也不播特效，全程隐形。
                //
                // PreviewCardPileAdd 是 void，拿不到"播完了"的句柄；底层 CardCmd.Preview 会返回
                // TaskCompletionSource，它在卡飞出屏幕（NCardFlyVfx 播完）时才完成，所以用它来卡住顺序：
                // 特效播完 → 才结算弃牌 → 才触发灵活 → 珠泪融合的选择界面此时才弹出。
                if (SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant)
                {
                    var previewFinished = CardCmd.Preview(card, DiscardPreviewSeconds);
                    if (previewFinished != null) await previewFinished.Task;
                }

                // 弃牌语义：照搬 CardCmd.DiscardAndDraw 内部对一张卡做的两步（记入战斗历史 + 广播 AfterCardDiscarded），
                // 但这里**不再调用 CardCmd.Discard**。
                // 因为卡此刻已经在弃牌堆里了，CardCmd.Discard 会把它再入堆一次（弃牌堆 → 弃牌堆），
                // 那一步会额外生成 NCardFlyShuffleVfx —— 表现出来就是"弃牌堆里有张牌又飞进弃牌堆"的第二个特效。
                // 注：DiscardAndDraw 里的 Sly 自动打出分支在这里不需要，检索出来的卡永远是刚生成的新卡，不可能带 Sly。
                if (CombatManager.Instance.IsOverOrEnding) continue;
                var combatState = card.CombatState ?? player.Creature.CombatState;
                if (combatState == null) continue;
                CombatManager.Instance.History.CardDiscarded(combatState, card);
                await MegaCrit.Sts2.Core.Hooks.Hook.AfterCardDiscarded(combatState, choiceContext, card);
            }
        }
        else
            foreach (var card in selectCards) await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);

        // 返回本次实际检索到并加入手牌的卡，供调用方继续处理（如丢弃、选择去向）
        return selectCards;
    }

    // 「检索并丢弃」时，卡在屏幕中央停留多久（之后才带拖尾飞进弃牌堆）。
    // 按快进模式缩放，口径与基础游戏的 Cmd.CustomScaledWait 一致；Instant 模式下整个特效都会被跳过。
    private static float DiscardPreviewSeconds =>
        SaveManager.Instance.PrefsSave.FastMode switch
        {
            FastModeType.Fast => 0.35f,
            _ => 0.8f,
        };

    /// <summary>检索「匕首」标签卡（角色自身卡池）。</summary>
    public static Task<List<CardModel>> RetrieverDaggerCard(PlayerChoiceContext choiceContext, Player player, int amount = 1)
    {
        return RetrieverCard(
            choiceContext,
            player,
            c => c.Keywords.Contains(YunoKeywords.Dagger),
            p => p.Title == player.Character.CardPool.Title,
            amount);
    }

    /// <summary>检索稀有卡（随机一名角色的卡池）。</summary>
    public static Task<List<CardModel>> RetrieverRareCard(PlayerChoiceContext choiceContext, Player player, int amount = 1)
    {
        var pools = player.UnlockState.CharacterCardPools.ToList();
        if (pools.Count == 0) return Task.FromResult(new List<CardModel>());
        var randomPool = player.RunState.Rng.Niche.NextItem(pools)!;
        return RetrieverCard(
            choiceContext,
            player,
            c => c.Rarity == CardRarity.Rare,
            p => p.Title == randomPool.Title,
            amount);
    }

    /// <summary>
    /// 通用「是/否」选择：生成「是」「否」两张选项卡弹选择网格，让玩家二选一。
    /// 返回 true = 选了「是」；false = 选了「否」。
    /// </summary>
    /// <param name="choiceContext">选择上下文。</param>
    /// <param name="player">做选择的玩家。</param>
    /// <param name="prompt">选择界面的提示文本（card_selection 本地化）。</param>
    /// <param name="source">触发询问的卡（用于提示文本的 {CardName} 卡名前缀）。</param>
    public static async Task<bool> AskYesNo(PlayerChoiceContext choiceContext, Player player, LocString prompt, CardModel? source = null)
    {
        if (source != null) prompt.Add("CardName", source.Title);

        var shi = player.Creature.CombatState!.CreateCard<ShiCard>(player);
        var fou = player.Creature.CombatState!.CreateCard<FouCard>(player);

        CardModel? picked = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            new List<CardModel> { shi, fou },
            player,
            new CardSelectorPrefs(prompt, 1, 1))).FirstOrDefault();

        return picked is ShiCard;
    }

    public static async Task SelectCardFromDraw2Discard(Player player, PlayerChoiceContext choiceContext)
    {
        CardPile pile = PileType.Discard.GetPile(player);
        CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, pile.Cards, player, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1, 1))).FirstOrDefault()!;
        bool flag = cardModel != null;
        bool flag2 = flag;
        if (flag2)
        {
            bool flag3;
            switch (cardModel!.Pile?.Type)
            {
                case PileType.Draw:
                case PileType.Discard:
                    flag3 = true;
                    break;
                default:
                    flag3 = false;
                    break;
            }
            flag2 = flag3;
        }
        if (flag2)
        {
            await CardCmd.Discard(choiceContext, cardModel!);
        }
    }

    public static async Task<IEnumerable<CardModel>> DuiMu(PlayerChoiceContext choiceContext, Player player, int amount)
    {

        List<CardModel> list = new List<CardModel>();
        var results = new List<CardPileAddResult>();

        for (int i = 0; i < amount; i++)
        {
            CardModel cardModel = PileType.Draw.GetPile(player).Cards.ToList().FirstOrDefault()!;
            if (cardModel == null)
            {
                await CardPileCmd.ShuffleIfNecessary(choiceContext, player);
                cardModel = PileType.Draw.GetPile(player).Cards.ToList().FirstOrDefault()!;
            }
            if (cardModel != null)
            {
                await CardCmd.Discard(choiceContext, cardModel);

                // 弃牌后自行组装入堆结果，仅用于播放预览（PreviewCardPileAdd 只读取 success 与 cardAdded）
                results.Add(new CardPileAddResult
                {
                    success = true,
                    cardAdded = cardModel,
                    targetPile = PileType.Discard,
                });

                list.Add(cardModel);
            }
        }

        // 展示被送入弃牌堆的卡（与 AddRandomCardsToExhaust 同款表现）
        PreviewPileAdd(results, 2f);

        return list;
    }

    public static async Task<IEnumerable<CardModel>> SelcetCardExhaust(PlayerChoiceContext choiceContext, Player player, PileType pileType, CardModel cardSource,
    int min = 1, int max = 1)
    {
        IEnumerable<CardModel> res = new List<CardModel>();
        if (pileType != PileType.Hand)
        {
            List<CardModel> cardsIn = (from c in pileType.GetPile(player).Cards
                                       orderby c.Rarity, c.Id
                                       select c).ToList();
            if (cardsIn.Count <= 0) return res;
            CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, cardsIn, player, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, min, max))).FirstOrDefault()!;

            if (cardModel != null)
            {
                await CardCmd.Exhaust(choiceContext, cardModel!);
                res = res.Append(cardModel);
            }
        }
        else if (pileType == PileType.Hand)
        {
            CardModel cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, min, max), context: choiceContext, player: player, filter: null, source: cardSource)).FirstOrDefault()!;
            if (cardModel != null)
            {
                await CardCmd.Exhaust(choiceContext, cardModel);
                res = res.Append(cardModel);
            }
        }
        return res;
    }
}
