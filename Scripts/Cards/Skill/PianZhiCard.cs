using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Power;

public class PianZhiCard : YunoBaseCard
{
    public PianZhiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<CardModel> cards = await ToolCmd.SelcetCardExhaust(choiceContext, Owner, PileType.Discard, this);

        if (cards.Count() <= 0) return;

        CardModel selectedCard = cards.First();

        if (selectedCard != null)
        {
            int cost = selectedCard.EnergyCost.GetWithModifiers(CostModifiers.None);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, cost, Owner.Creature, this);
        }


    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
