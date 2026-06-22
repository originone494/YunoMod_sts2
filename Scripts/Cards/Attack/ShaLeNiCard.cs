using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
namespace YunoMod.Scripts.Cards.Attack;

public class ShaLeNiCard : YunoBaseCard
{
    private const string _HealKey = "Heal";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new RepeatVar(2),
        new DynamicVar(_HealKey,4)
    ];

    public ShaLeNiCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Dagger),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Dagger];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await CreatureCmd.Damage(choiceContext, Owner.Creature, new DamageVar(2, ValueProp.Unpowered | ValueProp.Unblockable), this);

        bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());

        AttackCommand attackCommand = await ToolCmd.DaggerAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        if (shouldTriggerFatal && attackCommand.Results.SelectMany((List<DamageResult> r) => r).Any((DamageResult r) => r.WasTargetKilled))
        {
            await CreatureCmd.Heal(Owner.Creature, DynamicVars[_HealKey].IntValue);
        }

        await ToolCmd.DaggerStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
        DynamicVars[_HealKey].UpgradeValueBy(2);
    }
}
