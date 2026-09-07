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
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.GameInfo.Objects;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Cards.Other;
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

    public static async Task<IEnumerable<CardModel>> Foresee(PlayerChoiceContext choiceContext, Player player, int amount)
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
        var prefs = new CardSelectorPrefs(
            YunoSelectorPrefs.ForeseeSelectionPrompt,
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

    public static async Task<IEnumerable<CardModel>> ForeseeAndDraw(PlayerChoiceContext choiceContext, Player player, int ForeseeAmount = 5, int DrawAmount = 0)
    {
        return await Foresee(choiceContext, player, ForeseeAmount);
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
    public static async Task<List<CardModel>> RetrieverCard(
        PlayerChoiceContext choiceContext,
        Player player,
        Func<CardModel, bool> filter,
        Func<CardPoolModel, bool>? poolFilter = null,
        int amount = 1, bool isDiscard = false)
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

        var prefs = new CardSelectorPrefs(
            YunoSelectorPrefs.RetrieverSelectionPrompt,
            0,
            amount
        );

        var selectCards = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            combatCopies,
            player,
            prefs
        )).ToList();
        if (isDiscard)
            foreach (var card in selectCards) await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, player);
        else
            foreach (var card in selectCards) await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);

        // 返回本次实际检索到并加入手牌的卡，供调用方继续处理（如丢弃、选择去向）
        return selectCards;
    }

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
    public static async Task<bool> AskYesNo(PlayerChoiceContext choiceContext, Player player, LocString prompt)
    {
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

                CardCmd.Preview(cardModel);

                list.Add(cardModel);
            }
        }

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
