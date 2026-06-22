using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class ShiHengCard : YunoBaseCard
{
    public ShiHengCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        int strengthAmount = Owner.Creature.GetPowerAmount<StrengthPower>();
        int dexterityAmount = Owner.Creature.GetPowerAmount<DexterityPower>();
        int higherAmount = Math.Max(strengthAmount, dexterityAmount);

        int strengthDiff = higherAmount - strengthAmount;
        if (strengthDiff != 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strengthDiff, Owner.Creature, this);
        }

        int dexterityDiff = higherAmount - dexterityAmount;
        if (dexterityDiff != 0)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, dexterityDiff, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
