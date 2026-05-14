using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

public class AnZhongGuanChaCard : AbstractTemplateBaseCard
{

    private const string _drawKey = "Draw";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_drawKey, 2m),
    ];

    public AnZhongGuanChaCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<NiGeiWoXiaoXinDianPower>(Owner.Creature, 1, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[_drawKey].UpgradeValueBy(1m);
    }
}
