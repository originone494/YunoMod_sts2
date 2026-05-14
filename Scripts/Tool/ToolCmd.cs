using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
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
        var cardsToScry = drawPile.Cards.Take(amount).ToList();


        if (cardsToScry.Count == 0) return;
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.DiscardSelectionPrompt,
            0,
            cardsToScry.Count
        );

        var cardsToDiscard = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            cardsToScry,
            player,
            prefs
        )).ToList();
        foreach (var card in cardsToDiscard) await CardCmd.Discard(choiceContext, card);
        await ForeseeHook.OnForesee(choiceContext, player, amount, cardsToDiscard.Count);
    }

    public static async Task DaggerAttack(PlayerChoiceContext choiceContext, CardPlay cardPlay, decimal damage, CardModel cardSource, Creature player)
    {
        await DamageCmd.Attack(damage)
        .FromCard(cardSource)
        .Targeting(cardPlay.Target!)
        .WithHitFx("vfx/vfx_attack_slash")
        .Execute(choiceContext);

        await PowerCmd.Apply<LiuXuePower>(cardPlay.Target!, 1, player, cardSource);
    }

    public static async Task ForeseeAndDraw(PlayerChoiceContext choiceContext, Player player, int ForeseeAmount = 5, int DrawAmount = 1)
    {
        await ToolCmd.Foresee(choiceContext, player, ForeseeAmount);
        await CardPileCmd.Draw(choiceContext, DrawAmount, player);
    }
}