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

    private const string _blockGainKey = "BlockGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_blockGainKey, 0m),
    };

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        Flash();

        await CreatureCmd.GainBlock(Owner, new BlockVar((Amount + 1) / 2, ValueProp.Unpowered), null);
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            UpdateBlockDisplay();

            if (Owner.HasPower<AiYiYiChuPower>())
                return;

            if (applier == Owner && cardSource != null)
            {
                while (Amount >= 10)
                {
                    await PowerCmd.Apply<LovePower>(Owner, -10, Owner, null);
                    await CreatureCmd.GainBlock(Owner, new BlockVar(5, ValueProp.Unpowered), null);
                }
            }
        }
    }

    private void UpdateBlockDisplay()
    {
        DynamicVars[_blockGainKey].BaseValue = (Amount + 1) / 2;
    }
}
