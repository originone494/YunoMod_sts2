using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class LovePower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
    };

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        Flash();

        await CreatureCmd.GainBlock(Owner, new BlockVar((Amount + 1) / 2, ValueProp.Unpowered), null);

    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (Owner.HasPower<AiYiYiChuPower>())
        {
            return;
        }
        if (power == this && applier == Owner && cardSource != null)
        {
            while (Amount >= 10)
            {
                await PowerCmd.Apply<NiGeiWoXiaoXinDianPower>(Owner, -10, Owner, null);
                await CreatureCmd.GainBlock(Owner, new BlockVar(5, ValueProp.Unpowered), null);
            }
        }
    }
}
