using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Cards.Skill;

public class ShengZheWeiLaiCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
    ];

    public ShengZheWeiLaiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Foresee),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    private List<CardModel> cardModels = new List<CardModel>();



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = await ToolCmd.ForeseeAndDraw(choiceContext, Owner);
        var targetCard = cards.First();
        if (targetCard.Type == CardType.Attack && targetCard.BaseReplayCount == 0)
        {
            targetCard.BaseReplayCount += 1;
            cardModels.Add(targetCard);
        }

    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == this.Owner)
        {
            if (cardModels.Contains(cardPlay.Card))
            {
                cardPlay.Card.BaseReplayCount -= 1;
                if (cardPlay.Card.BaseReplayCount == 0)
                    cardModels.Remove(cardPlay.Card);
            }
        }
        return Task.CompletedTask;
    }
}
