using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class FeedDiaryRelic : YunoBaseRelic
{

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    private const string _increaseKey = "IncreaseCount";

    private bool _isFirstPlay = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_increaseKey,2)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_isFirstPlay) return;

        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.DynamicVars.ContainsKey("Damage"))
        {
            cardPlay.Card.DynamicVars.Damage.BaseValue += DynamicVars[_increaseKey].BaseValue;
            Flash();
            _isFirstPlay = false;
        }
        else if (cardPlay.Card.Type == CardType.Skill && cardPlay.Card.DynamicVars.ContainsKey("Block"))
        {
            cardPlay.Card.DynamicVars.Block.BaseValue += DynamicVars[_increaseKey].BaseValue;
            Flash();
            _isFirstPlay = false;
        }
        await Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side)
        {
            _isFirstPlay = true;
        }
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }


    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
