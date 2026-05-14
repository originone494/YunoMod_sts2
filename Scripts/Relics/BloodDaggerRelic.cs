using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class BloodDaggerRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ZhiCanPowerCount",1),
    ];

    public override async Task AfterSideTurnStart(CombatSide combatSide, CombatState combatState)
    {
        if (combatState == null) return;
        if(combatSide != CombatSide.Player) return;

        Flash();

        await PowerCmd.Apply<ZhiCanPower>(combatState.HittableEnemies, DynamicVars["ZhiCanPowerCount"].BaseValue, base.Owner.Creature, null);
    }
}
