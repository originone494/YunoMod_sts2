using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class ChangShiDuanLiCard : YunoSpecialBaseCard
{
    public ChangShiDuanLiCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ① 消耗弃牌堆最多5张卡（玩家选择，可选0张）
        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0) return;

        var exhausted = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, discardPile.Cards.ToList(), Owner, new CardSelectorPrefs(SelectionScreenPrompt, 0, 5))).ToList();

        foreach (var card in exhausted)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        // ② 以消耗的数量，从消耗堆选择卡送入弃牌堆
        int count = exhausted.Count;
        if (count == 0) return;

        var exhaustPile = PileType.Exhaust.GetPile(Owner);
        if (exhaustPile.Cards.Count == 0) return;

        var moved = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, exhaustPile.Cards.ToList(), Owner, new CardSelectorPrefs(SelectionScreenPrompt, count, count))).ToList();

        foreach (var card in moved)
        {
            await CardPileCmd.Add(card, PileType.Discard);
        }
    }
}
