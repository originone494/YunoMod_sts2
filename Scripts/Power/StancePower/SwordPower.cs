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

    private bool _isExited = false;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2),
        new CardsVar(2)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _isExited = false;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        if (Owner.Player != null && !_isExited)
        {
            await PlayerCmd.GainEnergy((int)DynamicVars.Energy.BaseValue, Owner.Player);
            await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), DynamicVars.Cards.BaseValue, Owner.Player);
            _isExited = true;
        }
    }
}