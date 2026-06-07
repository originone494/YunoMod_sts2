using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class BoomerangRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!creature.IsMonster) return;

        int weakAmount = (int)creature.GetPowerAmount<WeakPower>();
        int vulnAmount = (int)creature.GetPowerAmount<VulnerablePower>();

        if (weakAmount <= 0 && vulnAmount <= 0) return;

        var enemies = Owner.Creature.CombatState!.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        var target = enemies[Random.Shared.Next(enemies.Count)];

        if (weakAmount > 0)
        {
            Flash();
            await PowerCmd.Apply<WeakPower>(target, weakAmount, Owner.Creature, null);
        }
        if (vulnAmount > 0)
        {
            Flash();
            await PowerCmd.Apply<VulnerablePower>(target, vulnAmount, Owner.Creature, null);
        }
    }
}
