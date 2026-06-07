using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class HydrangeaRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    private bool _triggered;

    public override Task BeforeCombatStart()
    {
        _triggered = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_triggered) return;
        if (cardPlay.Card.Owner != Owner) return;

        _triggered = true;
        Flash();
        await CardPileCmd.Draw(choiceContext, 1, Owner);
        await PlayerCmd.GainEnergy(1, Owner);
    }
}
