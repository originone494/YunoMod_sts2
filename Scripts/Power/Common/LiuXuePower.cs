using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class LiuXuePower : YunoBasePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            Flash();

            await CreatureCmd.Damage(choiceContext, base.Owner, base.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            await BleedHook.OnBleedDamage(choiceContext, Owner.CombatState!, Owner, Amount);
            await PowerCmd.Decrement(this);
        }


    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (Owner == dealer && target.IsPlayer && cardSource == null)
        {
            await CreatureCmd.Damage(choiceContext, base.Owner, base.Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            await BleedHook.OnBleedDamage(choiceContext, Owner.CombatState!, Owner, Amount);
        }
    }
}
