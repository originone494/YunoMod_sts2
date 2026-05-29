using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

public class ZhanYouYuCard : YunoBaseCard
{

    private const string _selfDamageIncreaseKey = "SelfDamageIncreasion";
    private const string _selfDamageReductionKey = "SelfDamageReduction";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_selfDamageIncreaseKey, 0.25m),
        new DynamicVar(_selfDamageReductionKey, 0.5m),
    ];

    public ZhanYouYuCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await PowerCmd.Apply<ZhanYouYuWanJiaPower>(Owner.Creature, 1, Owner.Creature, this);

        await PowerCmd.Apply<ZhanYouYuDiRenPower>(cardPlay.Target, 1, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
