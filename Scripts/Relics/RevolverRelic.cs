using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace YunoMod.Scripts.Relics;

public class RevolverRelic : YunoBaseRelic, IOnStanceChange
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
    new DamageVar(4, ValueProp.Unpowered),
    ];

    public async Task OnStanceChange(PlayerChoiceContext ctx, Player player, Stance oldStance, Stance newStance)
    {
        if (player == Owner && oldStance == Stance.Gun)
            foreach (var enemy in player.Creature.CombatState!.HittableEnemies)
                await CreatureCmd.Damage(ctx, enemy, DynamicVars.Damage, player.Creature);
    }

}
