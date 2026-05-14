using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

public class KanPoCard : AbstractTemplateBaseCard
{

    private const string _blockKey = "GetBlock";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_blockKey, 4m),
    ];

    public KanPoCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<KanPoPower>(Owner.Creature, DynamicVars[_blockKey].BaseValue, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[_blockKey].UpgradeValueBy(2m);
    }
}
