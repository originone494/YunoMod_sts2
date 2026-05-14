using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class KillingDiaryRelic : YunoBaseRelic
{
    private Dictionary<Creature, decimal> DamageMap { get; set; } = new Dictionary<Creature, decimal>();

    public override RelicRarity Rarity => RelicRarity.Common;

    public override Task BeforeCombatStart()
    {
        DamageMap.Clear();
        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == Owner.Creature && !target.IsPlayer)
        {
            if (!DamageMap.ContainsKey(target))
            {
                DamageMap[target] = 0m;
            }
            if (result.TotalDamage > target.CurrentHp)
            {
                DamageMap[target] = result.TotalDamage;
            }
        }
        await Task.CompletedTask;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (target.IsPlayer) return;
        if (!DamageMap.ContainsKey(target)) return;

        decimal damageAmount = DamageMap[target];
        if (damageAmount <= 0m) return;
        if (Owner?.Creature?.CombatState == null) return;

        Flash();

        foreach (Creature creature in Owner.Creature.CombatState.HittableEnemies)
        {
            if (creature != target)
            {
                await CreatureCmd.Damage(choiceContext, creature, damageAmount, ValueProp.Move, Owner.Creature, null);
            }
        }

        DamageMap.Remove(target);
    }
}
