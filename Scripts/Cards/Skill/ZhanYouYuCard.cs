using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

public class ZhanYouYuCard : YunoBaseCard
{

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    public ZhanYouYuCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        List<CardModel> cardsIn = (from c in PileType.Hand.GetPile(Owner).Cards
                                   orderby c.Rarity, c.Id
                                   select c).ToList();
        CardModel cardModel = (await CardSelectCmd.FromSimpleGrid(choiceContext, cardsIn, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).FirstOrDefault();

        if (cardModel != null)
        {
            cardModel.EnergyCost.AddThisCombat(cardModel.EnergyCost.Canonical);
            cardModel.BaseReplayCount += 1;
        }

    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
