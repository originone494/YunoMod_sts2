using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class TheWatcherRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        if (combatState.RoundNumber > 1) return;

        await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);


        var otherPools = Owner.UnlockState.CharacterCardPools
            .Where(p => p is not YunoCardPool)
            .ToList();

        if (otherPools.Count == 0) return;

        var eligibleCards = otherPools
            .SelectMany(pool => pool.GetUnlockedCards(
                Owner.UnlockState,
                Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Rarity == CardRarity.Uncommon || c.Rarity == CardRarity.Rare)
            .ToList();

        if (eligibleCards.Count == 0) return;

        var template = Owner.RunState.Rng.CombatTargets.NextItem(eligibleCards);
        if (template == null) return;

        var card = combatState.CreateCard(template, Owner);
        card.EnergyCost.AddThisTurn(-card.EnergyCost.GetWithModifiers(CostModifiers.None));
        card.AddKeyword(CardKeyword.Ethereal);


        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
