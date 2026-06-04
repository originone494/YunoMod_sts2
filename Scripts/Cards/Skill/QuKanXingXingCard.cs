using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class QuKanXingXingCard : YunoBaseCard
{

    private const string _discardKey = "Discard";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_discardKey, 3m),
    ];

    public QuKanXingXingCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
    ];

    

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int maxDiscard = (int)DynamicVars[_discardKey].BaseValue;

        List<CardModel> selectedCards = [.. await CardSelectCmd.FromHandForDiscard(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, maxDiscard),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this
        )];

        int actualDiscardCount = selectedCards.Count;
        if (actualDiscardCount <= 0)
        {
            return;
        }

        await CardCmd.Discard(choiceContext, selectedCards);

        await ToolCmd.GainLovePower(choiceContext, Owner, this, actualDiscardCount);


        await CardPileCmd.Draw(choiceContext, actualDiscardCount, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_discardKey].UpgradeValueBy(2m);
    }
}
