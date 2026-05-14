using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Skill;

public class WeiLaiPianChaCard : AbstractTemplateBaseCard
{
    public WeiLaiPianChaCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(12m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => (int)(Owner.Creature.CurrentHp > Owner.Creature.MaxHp * 0.4 ? Owner.Creature.CurrentHp - Owner.Creature.MaxHp * 0.4 : 0))
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {


        if (DynamicVars.CalculatedDamage.BaseValue > 0)
        {
            await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.CalculatedDamage.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);

            await PowerCmd.Apply<BaoZaPower>(Owner.Creature, DynamicVars.CalculatedDamage.BaseValue, Owner.Creature, this);

        }
        else
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, 3, Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(Owner.Creature, 3, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
