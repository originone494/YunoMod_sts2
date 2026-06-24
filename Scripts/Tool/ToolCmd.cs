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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
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

    public static async Task Foresee(PlayerChoiceContext choiceContext, Player player, int amount)
    {
        if (amount <= 0) return;

        var drawPile = PileType.Draw.GetPile(player);

        if (drawPile.Cards.Count == 0)
        {
            await CardPileCmd.ShuffleIfNecessary(choiceContext, player);
            drawPile = PileType.Draw.GetPile(player);
        }

        var cardsToScry = drawPile.Cards.Take(amount).ToList();


        if (cardsToScry.Count == 0) return;
        var prefs = new CardSelectorPrefs(
            YunoSelectorPrefs.ForeseeSelectionPrompt,
            0,
            cardsToScry.Count()
        );

        var cardsToDiscard = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cardsToScry,
            player,
            prefs
        )).ToList();
        foreach (var card in cardsToDiscard)
        {
            await CardCmd.Discard(choiceContext, card);
        }
        await ForeseeHook.OnForesee(choiceContext, player, amount, cardsToDiscard.Count);
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

    public static async Task<AttackCommand> DaggerAttack(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource, decimal damage, int repeat = 1)
    {
        var cmd = await DamageCmd.Attack(damage)
        .FromCard(cardSource)
        .Targeting(target)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);

        for (int i = 0; i < repeat; i++)
            await PowerCmd.Apply<LiuXuePower>(choiceContext, target, 1, cardSource.Owner.Creature, cardSource);


        return cmd;
    }



    public static async Task<AttackCommand> DaggerAttackAllEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, int repeat = 1)
    {
        var cmd = await DamageCmd.Attack(damage)
        .FromCard(cardSource)
        .TargetingAllOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);

        for (int i = 0; i < repeat; i++)
            foreach (Creature enemy in cardSource.Owner.Creature.CombatState!.HittableEnemies)
                await PowerCmd.Apply<LiuXuePower>(choiceContext, enemy, 1, cardSource.Owner.Creature, cardSource);

        return cmd;
    }


    public static async Task<AttackCommand> GunAttack(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource, decimal damage, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource)
        .Targeting(target)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }


    public static async Task<AttackCommand> GunAttackAllEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource)
        .TargetingAllOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }

    public static async Task<AttackCommand> GunAttackRandomEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource)
        .TargetingRandomOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }

    public static async Task<AttackCommand> AxeAttack(PlayerChoiceContext choiceContext, Creature target, CardModel cardSource, decimal damage, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
       .FromCard(cardSource)
       .Targeting(target)
       .WithHitCount(repeat)
       .WithHitFx("vfx/vfx_attack_blunt")
       .Execute(choiceContext);
    }

    public static async Task<AttackCommand> AxeAttackAllEnemy(PlayerChoiceContext choiceContext, CardModel cardSource, decimal damage, int repeat = 1)
    {
        return await DamageCmd.Attack(damage)
        .FromCard(cardSource)
        .TargetingAllOpponents(cardSource.Owner.Creature.CombatState!)
        .WithHitCount(repeat)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(choiceContext);
    }

    public static async Task<IEnumerable<CardModel>> ForeseeAndDraw(PlayerChoiceContext choiceContext, Player player, int ForeseeAmount = 3, int DrawAmount = 1)
    {
        await Foresee(choiceContext, player, ForeseeAmount);
        return await CardPileCmd.Draw(choiceContext, DrawAmount, player);

    }

    public static async Task GainLovePower(PlayerChoiceContext choiceContext, Player player, CardModel source, int amount)
    {
        await PowerCmd.Apply<LovePower>(choiceContext, player.Creature, amount, player.Creature, source);
        await LovePowerHook.OnGetLove(choiceContext, player, amount);
    }

    public static async Task RetrieverDaggerCard(PlayerChoiceContext choiceContext, Player player, int amount = 1)
    {

        CardPoolModel list = player.Character.CardPool;

        IReadOnlyList<CardModel> cards =
        list.GetUnlockedCards(
        player.UnlockState,
        player.RunState.CardMultiplayerConstraint).Where(c => c.Keywords.Contains(YunoKeywords.Dagger)).ToList();


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
        foreach (var card in selectCards) await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
    }

    public static async Task RetrieverRareCard(PlayerChoiceContext choiceContext, Player player, int amount = 1)
    {
        List<CardPoolModel> pools = player.UnlockState.CharacterCardPools.ToList();

        // 随机选择一个职业的卡池
        var randomPool = pools[Random.Shared.Next(pools.Count)];

        IReadOnlyList<CardModel> cards = randomPool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity == CardRarity.Rare)
            .ToList();

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
        foreach (var card in selectCards) await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
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