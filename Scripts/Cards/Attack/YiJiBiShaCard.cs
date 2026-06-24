using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
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

    public YiJiBiShaCard() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Axe];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Axe),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.AxeAttack(choiceContext, cardPlay.Target!, this, DynamicVars.Damage.BaseValue);


        var suppressCards = PileType.Discard.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(YunoTags.YaZhi))
            .ToList();

        foreach (var card in suppressCards)
            await CardCmd.Exhaust(choiceContext, card);

        if (Owner.Creature.HasPower<AxePower>())
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }


        await ToolCmd.AxeStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}
