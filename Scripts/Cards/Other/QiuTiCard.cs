using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Other;

public class QiuTiCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered).WithMultiplier((card, _) =>
        {
            if (card?.Owner?.Creature == null) return 1m;
            int count = card.Owner.Creature.GetPowerAmount<DiaryPower>();
            return count > 0 ? count : 1;
        }),
        new CardsVar(1)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public QiuTiCard() : base(0, CardType.Attack, CardRarity.Token, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<DiaryPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            int repeatCount = Owner.Creature.GetPowerAmount<DiaryPower>();

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(Owner.Creature.CombatState!)
                .WithHitCount(repeatCount)
                .WithHitFx("vfx/vfx_dramatic_stab")
                .Execute(choiceContext);


            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}
