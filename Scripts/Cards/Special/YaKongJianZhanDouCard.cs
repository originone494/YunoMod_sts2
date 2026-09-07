using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class YaKongJianZhanDouCard : YunoSpecialBaseCard
{
    public YaKongJianZhanDouCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 从抽牌堆选择最多3张卡
        var drawPile = PileType.Draw.GetPile(Owner);
        int amount = drawPile.Cards.ToList().Count() < 3 ? drawPile.Cards.ToList().Count() : 3;
        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, amount, amount);
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, drawPile.Cards.ToList(), Owner, prefs)).ToList();

        if (selected == null) return;

        foreach (var card in selected)
        {
            if (card.EnergyCost.GetWithModifiers(CostModifiers.None) >= 2)
            {
                // 费用大于等于2：加入手牌
                await CardPileCmd.Add(card, PileType.Hand);
            }
            else
            {
                // 费用小于2：送去弃牌堆并受到1点伤害
                await CardCmd.Discard(choiceContext, card);
                await CreatureCmd.Damage(choiceContext, Owner.Creature, 1m, ValueProp.Unpowered, Owner.Creature, this, cardPlay);
            }
        }
    }
}
