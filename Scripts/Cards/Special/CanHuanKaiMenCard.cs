using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「燦幻開門」（Sangen Kaimen，站内 cn_name/sc_name「灿幻开门」）：
//   打出：是 →「检索」1张「天杯龙」；否 → 使手牌的1张「天杯龙」卡的费用为0（参考珠泪·水仙的是/否实现）
//   时机：回合结束时，触发这张卡的两个效果（先检索、后0费，让0费可以落在刚检索到的卡上）
public class CanHuanKaiMenCard : YunoSpecialBaseCard
{
    public CanHuanKaiMenCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    // 是/否：「检索」还是 0 费
    private static LocString KaiMenChoicePrompt { get; } = new("card_selection", "TO_CAN_HUAN_KAI_MEN_CHOICE");

    // 0 费选卡提示
    private static LocString ZeroCostPrompt { get; } = new("card_selection", "TO_CAN_HUAN_KAI_MEN_ZERO_COST");

    private static bool IsTianBeiLong(CardModel c) => c.Tags.Contains(YunoTags.TianBeiLong);

    // X 费卡无法设为 0 费，排除（搜查日记同款处理）
    private static bool CanBeFree(CardModel c) => IsTianBeiLong(c) && !c.EnergyCost.CostsX;

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.CanHuanMoFa,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.CanHuanMoFa),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 是 →「检索」1张「天杯龙」；否 → 使手牌的1张「天杯龙」卡的费用为0
        if (await ToolCmd.AskYesNo(choiceContext, Owner, KaiMenChoicePrompt, source: this))
        {
            await RetrieveTianBeiLong(choiceContext, Owner);
        }
        else
        {
            await MakeTianBeiLongFree(choiceContext, Owner);
        }
    }

    // 时机：回合结束时，触发这张卡的两个效果
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;

        // 先检索、后0费：让0费可以落在刚检索到的「天杯龙」上
        await RetrieveTianBeiLong(choiceContext, Owner);
        await MakeTianBeiLongFree(choiceContext, Owner);
    }

    private async Task RetrieveTianBeiLong(PlayerChoiceContext choiceContext, Player player)
    {
        await ToolCmd.RetrieverCard(
            choiceContext,
            player,
            IsTianBeiLong,
            p => p is YunoSpecialCardPool,
            1, source: this);
    }

    private async Task MakeTianBeiLongFree(PlayerChoiceContext choiceContext, Player player)
    {
        var hand = PileType.Hand.GetPile(player).Cards.Where(CanBeFree).ToList();
        if (hand.Count == 0) return;

        var selected = await CardSelectCmd.FromHand(
            prefs: CardPrefs(this, ZeroCostPrompt, 1, 1),
            context: choiceContext,
            player: player,
            filter: CanBeFree,
            source: this);

        var card = selected.FirstOrDefault();
        if (card != null)
        {
            // 本场战斗费用为 0：挂战斗内费用修正，不影响牌组原卡（搜查日记同款）
            card.EnergyCost.SetThisCombat(0);
        }
    }
}
