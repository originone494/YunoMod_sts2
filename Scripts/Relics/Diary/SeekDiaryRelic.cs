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
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class SeekDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;


    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner) return;
        if (Owner.Creature.CombatState!.RoundNumber > 1) return;

        // 候选为抽牌堆中的技能牌；X 费技能无法设为 0 费，排除以保证描述成立
        var skillCards = PileType.Draw.GetPile(Owner).Cards
            .Where(c => c.Type == CardType.Skill && !c.EnergyCost.CostsX)
            .ToList();

        if (skillCards.Count == 0) return;

        var selected = Owner.RunState.Rng.Niche.NextItem(skillCards)!;

        if (selected == null) return;

        Flash();
        // 本场战斗费用为 0：对战斗内的卡实例挂本地费用修正，战斗结束随实例一起清除，
        // 不会影响牌组中的原卡。
        selected.EnergyCost.SetThisCombat(0);
        await CardPileCmd.Add(selected, PileType.Hand);
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
