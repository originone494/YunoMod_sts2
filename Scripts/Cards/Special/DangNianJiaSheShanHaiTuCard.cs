using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class DangNianJiaSheShanHaiTuCard : YunoSpecialBaseCard
{
    public DangNianJiaSheShanHaiTuCard() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先选择一张可变化的手牌。
        CardModel? selected = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: card => card.IsTransformable,
            source: this)).FirstOrDefault();

        if (selected == null) return;

        // Player.DiscoveredCards 是本局运行期间记录的已见过卡牌，不使用跨局的图鉴解锁记录。
        List<CardModel> seenCards = Owner.DiscoveredCards
            .Distinct()
            .Select(id => ModelDb.GetById<CardModel>(id))
            .Where(card => card != null)
            .Select(card => CombatState!.CreateCard(card, Owner))
            .ToList();

        if (seenCards.Count == 0) return;

        CardModel? replacement = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            seenCards,
            Owner,
            canSkip: false);

        if (replacement == null) return;

        await CardCmd.Transform(selected, replacement);
    }
}
