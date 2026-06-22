using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class NaQiWeiLaiCard : YunoBaseCard
{
    private const string _addKey = "Add";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_addKey, 3m),
    };

    public NaQiWeiLaiCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<WeiLaiXunXiCard>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),

    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int addCount = (int)DynamicVars[_addKey].BaseValue;

        var resultList = new List<CardPileAddResult>();
        for (int i = 0; i < addCount; i++)
        {
            CardModel card = Owner.Creature.CombatState!.CreateCard<WeiLaiXunXiCard>(Owner);
            var addResult = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, cardPlay.Card.Owner);
            resultList.Add(addResult);
        }
        CardCmd.PreviewCardPileAdd(resultList, 2f);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_addKey].UpgradeValueBy(1m);
    }
}
