using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·反响：「检索」1张「珠泪怪兽」卡；若从手牌消耗1张相同费用的卡，则检索的卡可以在这个回合免费打出。
// 灵活：从消耗堆将1张「珠泪陷阱」卡加入手牌。
public class ZhuLeiFanXiangCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiFanXiangCard() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.LingHuo,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiXianJing),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ① 「检索」1张「珠泪怪兽」卡加入手牌
        var retrieved = await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.ZhuLeiGuaiShou),
            p => p is YunoSpecialCardPool,
            1);

        CardModel? retrievedCard = retrieved.FirstOrDefault();
        if (retrievedCard == null) return;

        // ② 从手牌消耗1张与检索卡费用相同的卡（可以不选）
        int cost = retrievedCard.EnergyCost.GetWithModifiers(CostModifiers.None);
        bool hasSameCostCard = PileType.Hand.GetPile(Owner).Cards
            .Any(c => c != retrievedCard && c.EnergyCost.GetWithModifiers(CostModifiers.None) == cost);
        if (!hasSameCostCard) return;

        CardModel? toExhaust = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, 1),
            context: choiceContext,
            player: Owner,
            filter: c => c != retrievedCard && c.EnergyCost.GetWithModifiers(CostModifiers.None) == cost,
            source: this)).FirstOrDefault();

        if (toExhaust == null) return;

        await CardCmd.Exhaust(choiceContext, toExhaust);

        // ③ 检索的卡在这个回合可以免费打出
        retrievedCard.EnergyCost.AddThisTurn(-retrievedCard.EnergyCost.GetWithModifiers(CostModifiers.None));
    }

    // 灵活：从消耗堆将1张「珠泪陷阱」卡加入手牌
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        if (!exhaustPile.Cards.Any(c => c.Tags.Contains(YunoTags.ZhuLeiXianJing))) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            exhaustPile,
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            filter: c => c.Tags.Contains(YunoTags.ZhuLeiXianJing))).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }
}
