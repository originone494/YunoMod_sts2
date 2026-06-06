using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class BaoZaPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        int amount = 0;
        foreach (var enemy in Owner.CombatState!.HittableEnemies)
        {
            amount += 1;
        }
        if (amount == 0)
        {
            await CreatureCmd.Heal(Owner, Amount);
            await PowerCmd.Remove(this);
        }
    }


}
