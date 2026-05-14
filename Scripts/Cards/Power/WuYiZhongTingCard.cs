using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

public class WuYiZhongTingCard : AbstractTemplateBaseCard
{

    private const string _drawKey = "Draw";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_drawKey, 1m),
    ];

    public WuYiZhongTingCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<WuYiZhongTingPower>(Owner.Creature, 1, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[_drawKey].UpgradeValueBy(1m);
    }
}
