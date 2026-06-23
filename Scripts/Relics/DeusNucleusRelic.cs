using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class DeusNucleusRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    private const string _getCount = "GetCount";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_getCount,4)
    ];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not StrengthPower) return;
        if (amount <= 0) return;
        if (power.Owner != Owner.Creature) return;

        Flash();
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, (int)(amount * DynamicVars[_getCount].IntValue), Owner.Creature, null);
    }
}
