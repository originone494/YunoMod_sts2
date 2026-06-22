using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

using STS2RitsuLib.Keywords;
namespace YunoMod.Scripts.Cards.Attack;

public class XiePoCard : YunoBaseCard
{
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
    };

    public XiePoCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Axe];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.Axe),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");


        if (cardPlay.Target.HasPower<VulnerablePower>())
        {
            await ToolCmd.AxeAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue, 2);
        }
        else
        {
            await ToolCmd.AxeAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue);
        }

        await ToolCmd.AxeStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
