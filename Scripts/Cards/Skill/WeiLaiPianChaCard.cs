using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class WeiLaiPianChaCard : YunoBaseCard
{
    public WeiLaiPianChaCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {

    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ComputedDynamicVar("CalculatedDamage", 0m, card =>
        {
            if (card == null || !card.IsMutable || card.Owner == null) return 0m;
            var creature = card.Owner.Creature;
            return (int)(creature.CurrentHp > creature.MaxHp * 0.4 ? creature.CurrentHp - creature.MaxHp * 0.4 : 0);
        }),
        new PowerVar<StrengthPower>(3),
        new PowerVar<DexterityPower>(3)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<BaoZaPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        if (Owner.Creature.CurrentHp > Owner.Creature.MaxHp * 0.4)
        {

            int damage = (int)(Owner.Creature.CurrentHp - Owner.Creature.MaxHp * 0.4);

            await CreatureCmd.Damage(choiceContext, Owner.Creature, damage, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this);

            await PowerCmd.Apply<BaoZaPower>(choiceContext, Owner.Creature, damage, Owner.Creature, this);

        }
        else
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars.Dexterity.IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
