using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

using STS2RitsuLib.Keywords;
namespace YunoMod.Scripts.Cards.Attack;

public class BaoHuNiCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(11m),
        new CalculationExtraVar(1m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => ( card.Owner != null) ? card.Owner.Creature.GetPowerAmount<LovePower>() : 0),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => ( card.Owner != null) ? card.Owner.Creature.GetPowerAmount<LovePower>() : 0)
    ];


    public BaoHuNiCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Dagger];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.Dagger),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.DaggerAttack(choiceContext, cardPlay.Target, this, DynamicVars.CalculatedDamage.BaseValue);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.CalculatedBlock.Calculate(cardPlay.Target), DynamicVars.CalculatedBlock.Props, cardPlay);

        await ToolCmd.DaggerStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationExtra.UpgradeValueBy(1m);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}
