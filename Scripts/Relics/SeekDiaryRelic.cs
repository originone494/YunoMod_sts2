using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class SeekDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (player != Owner) return;
        if (Owner.Creature.CombatState!.RoundNumber > 1) return;

        await PowerCmd.Apply<DiaryPower>(Owner.Creature, 1, base.Owner.Creature, null);

        var skillCards = PileType.Draw.GetPile(Owner).Cards
            .Where(c => c.Type == CardType.Skill)
            .ToList();

        if (skillCards.Count == 0) return;

        var prefs = new CardSelectorPrefs(
            SelectionScreenPrompt,
            1,
            1
        );

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            skillCards,
            Owner,
            prefs
        )).ToList();

        if (selected.Count == 0) return;

        Flash();
        await CardPileCmd.Add(selected[0], PileType.Hand);
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
