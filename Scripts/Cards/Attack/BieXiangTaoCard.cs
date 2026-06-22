using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Entities.Players;
namespace YunoMod.Scripts.Cards.Attack;

public class BieXiangTaoCard : YunoBaseCard
{
    private const string _GrtCardCount = "GrtCardCount";
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m,ValueProp.Move),
        new DynamicVar(_GrtCardCount, 1),
        new BlockVar(7,ValueProp.Move)
    ];

    public BieXiangTaoCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Axe];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Axe),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.AxeAttack(choiceContext, cardPlay.Target!, this, DynamicVars.Damage.BaseValue);

        var discardPile = PileType.Discard.GetPile(Owner);
        var selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, discardPile.Cards.ToList(), Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, DynamicVars[_GrtCardCount].IntValue));

        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }

        await ToolCmd.AxeStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_GrtCardCount].UpgradeValueBy(1);
    }

}
