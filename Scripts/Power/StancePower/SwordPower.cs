using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class SwordPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2),
    ];


    public override async Task AfterRemoved(Creature oldOwner)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner.Player!);
    }
}