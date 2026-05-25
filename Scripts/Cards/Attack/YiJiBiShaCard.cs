using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Attack;

public class YiJiBiShaCard : YunoBaseCard
{


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(36m, ValueProp.Move),
        new EnergyVar(3)
    };

    public YiJiBiShaCard() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }


    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Axe];


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Axe),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");


        await ToolCmd.AxeAttack(choiceContext, cardPlay.Target!, this, DynamicVars.Damage.BaseValue);

        if (Owner.Creature.HasPower<AxePower>())
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

        await ToolCmd.AxeStance(choiceContext, Owner, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}
