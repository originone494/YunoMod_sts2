using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Skill;

public class QingKongDanJiaCard : AbstractTemplateBaseCard
{
    public QingKongDanJiaCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(3),
        new CardsVar(3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (Owner.Creature.HasPower<QiangXiePower>())
        {
            QiangXiePower power = Owner.Creature.GetPower<QiangXiePower>()!;

            if (power != null)
            {
                int amount = power.Amount;
                if (amount > 0)

                    if (amount > 3) await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);

                if (amount > 4) await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

                if (amount > 5) await PowerCmd.Apply<StrengthPower>(Owner.Creature, amount, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
