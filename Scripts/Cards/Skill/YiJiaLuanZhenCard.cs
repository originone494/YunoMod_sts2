using System.Collections.Generic;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class YiJiaLuanZhenCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
    ];

    public YiJiaLuanZhenCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel selectedCard = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), context: choiceContext, player: base.Owner, filter: null, source: this)).FirstOrDefault();
		if (selectedCard != null)
		{
			await CardCmd.Exhaust(choiceContext, selectedCard);
		}else{
			return;
		}


        // 根据牌的类型施加不同效果
        switch (selectedCard.Type)
        {
            case CardType.Attack:
                // 攻击牌：抽3张牌
                await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
                break;

            case CardType.Skill:
                // 技能牌：立即打出该牌
                await CardCmd.AutoPlay(choiceContext, selectedCard, null);
                break;

            case CardType.Power:
                // 能力牌：获得等同于该牌费用的能量
                int cost = selectedCard.EnergyCost.GetWithModifiers(CostModifiers.None);
                await PlayerCmd.GainEnergy(cost, Owner);
                break;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
