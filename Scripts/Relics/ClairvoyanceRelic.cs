using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class ClairvoyanceRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side == base.Owner.Creature.Side)
        {
            if (combatState.RoundNumber <= 1)
            {
                Flash();
                await PowerCmd.Apply<DiaryPower>(Owner.Creature, 1, base.Owner.Creature, null);


                bool haveAttack = false;

                foreach (var enemy in Owner.Creature.CombatState!.HittableEnemies)
                {
                    if (enemy.Monster.IntendsToAttack)
                    {
                        haveAttack = true;
                    }
                }
                if (haveAttack)
                    await PowerCmd.Apply<DexterityPower>(Owner.Creature, 1, Owner.Creature, null);
                else
                    await PowerCmd.Apply<StrengthPower>(Owner.Creature, 1, Owner.Creature, null);
            }

            if (combatState.RoundNumber == 2)
            {
                bool haveAttack = false;

                foreach (var enemy in Owner.Creature.CombatState!.HittableEnemies)
                {
                    if (enemy.Monster.IntendsToAttack)
                    {
                        haveAttack = true;
                    }
                }
                if (haveAttack)
                    await PowerCmd.Apply<DexterityPower>(Owner.Creature, 1, Owner.Creature, null);
                else
                    await PowerCmd.Apply<StrengthPower>(Owner.Creature, 1, Owner.Creature, null);
            }

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
