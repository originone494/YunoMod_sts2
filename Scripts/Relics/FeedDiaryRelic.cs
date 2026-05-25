using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class FeedDiaryRelic : YunoBaseRelic
{
    private CardModel? _fedCard;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        if (combatState.RoundNumber > 1) return;

        await PowerCmd.Apply<DiaryPower>(Owner.Creature, 1, base.Owner.Creature, null);

        List<CardModel> selected = (from c in PileType.Draw.GetPile(Owner).Cards
                                   where c.DynamicVars.ContainsKey("Damage") || c.DynamicVars.ContainsKey("Block")
                                   orderby c.Rarity, c.Id
                                   select c).ToList();
        CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, selected, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).FirstOrDefault()!;

        _fedCard = cardModel;
        Flash();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_fedCard == null || cardPlay.Card != _fedCard) return;

        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.DynamicVars.ContainsKey("Damage"))
        {
            cardPlay.Card.DynamicVars.Damage.BaseValue += 1m;
            Flash();
        }
        else if (cardPlay.Card.Type == CardType.Skill && cardPlay.Card.DynamicVars.ContainsKey("Block"))
        {
            cardPlay.Card.DynamicVars.Block.BaseValue += 1m;
            Flash();
        }
    }

    public override Task BeforeCombatStart()
    {
        _fedCard = null;
        return Task.CompletedTask;
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
