using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using STS2RitsuLib.Cards.DynamicVars;
namespace YunoMod.Scripts.Cards.Attack;

public class XiePoCard : YunoBaseCard
{

    private const string _drawCount = "DrawCount";
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
        new ComputedDynamicVar(_drawCount,0,card=> card !=null?PileType.Discard.GetPile(card.Owner).Cards
                .Where(c => c.Tags.Contains(YunoTags.YaZhi))
                .ToList().Count() : 0)
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

        await ToolCmd.AxeAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue);

        await ToolCmd.AxeStance(choiceContext, Owner, this);

        List<CardModel> selectedCards = (await ToolCmd.SelcetCardExhaust(choiceContext, Owner, PileType.Discard, this, 0, 1)).ToList();


        if (cardPlay.Target.HasPower<VulnerablePower>())
        {
            await CardPileCmd.Draw(choiceContext, PileType.Discard.GetPile(Owner).Cards
                .Where(c => c.Tags.Contains(YunoTags.YaZhi))
                .ToList().Count(), Owner);

        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
