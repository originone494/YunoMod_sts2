using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

public class XueJianCard : YunoBaseCard
{


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move)
    ];

    public XueJianCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
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

        bool getBlock = cardPlay.Target.GetPowerAmount<VulnerablePower>() > 0;

        var damage = DynamicVars.Damage.BaseValue;
        AttackCommand attackCommand = await ToolCmd.AxeAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue);

        if (getBlock) await CreatureCmd.GainBlock(Owner.Creature, attackCommand.Results.SelectMany((List<DamageResult> r) => r).Sum((DamageResult r) => r.TotalDamage + r.OverkillDamage), ValueProp.Move, cardPlay);

        await ToolCmd.AxeStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
