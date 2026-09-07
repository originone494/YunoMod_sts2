using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「替罪羊」：召唤4只费用1~4的羊衍生物进入抽牌堆
public class TiZuiYangCard : YunoBaseCard
{
    public TiZuiYangCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<YangYanShengWuCard>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        var resultList = new List<CardPileAddResult>();
        for (int cost = 1; cost <= 4; cost++)
        {
            CardModel card = Owner.Creature.CombatState!.CreateCard<YangYanShengWuCard>(Owner);
            card.EnergyCost.SetCustomBaseCost(cost);              // 设置这张实例的费用
            card.DynamicVars["Repeat"].BaseValue = cost;          // 动态变量：格挡重复次数
            var addResult = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, cardPlay.Card.Owner);
            resultList.Add(addResult);
        }
        CardCmd.PreviewCardPileAdd(resultList, 2f);
    }
}