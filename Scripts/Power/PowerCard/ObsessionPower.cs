using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class ObsessionPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private const string _blockGainKey = "BlockGain";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_blockGainKey, 0m),
    };

    // 回合开始时，若执念层数大于10，则消耗10层，使这回合进入疯狂状态
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;

        if (Amount > 10)
        {
            Flash();
            await PowerCmd.Apply<ObsessionPower>(new ThrowingPlayerChoiceContext(), Owner, -10, Owner, null);
            await PowerCmd.Apply<CrazyPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
        }
    }

    // 回合结束时，获得执念层数点格挡
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        Flash();

        await CreatureCmd.GainBlock(Owner, new BlockVar(Amount, ValueProp.Unpowered), null);
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            UpdateBlockDisplay();
        }
    }

    private void UpdateBlockDisplay()
    {
        DynamicVars[_blockGainKey].BaseValue = Amount;
    }
}
