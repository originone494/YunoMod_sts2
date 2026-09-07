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

public class YouShiDuanLiCard : YunoSpecialBaseCard
{
    public YouShiDuanLiCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ① 从弃牌堆选择1张卡加入手牌（弃牌堆为空或未选择则停止）
        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0) return;

        var retrieved = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, discardPile.Cards.ToList(), Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).ToList();
        if (retrieved.Count == 0) return;

        await CardPileCmd.Add(retrieved[0], PileType.Hand);

        // ② 选择1张手牌消耗（可选，未选择则结束）
        var exhausted = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this)).ToList();
        if (exhausted.Count == 0) return;

        await CardCmd.Exhaust(choiceContext, exhausted[0]);

        // ③ 从消耗堆选择1张卡送入弃牌堆（消耗堆为空则跳过）
        var exhaustPile = PileType.Exhaust.GetPile(Owner);
        if (exhaustPile.Cards.Count == 0) return;

        var moved = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, exhaustPile.Cards.ToList(), Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).ToList();
        if (moved.Count == 0) return;

        await CardPileCmd.Add(moved[0], PileType.Discard);
    }
}
