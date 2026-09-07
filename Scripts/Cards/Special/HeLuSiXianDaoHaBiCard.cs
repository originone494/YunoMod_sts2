using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Power.PowerCard;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 荷鲁斯·先导哈比：获得16点格挡，从弃牌堆或消耗堆将2张卡加入手牌。
// 是/否选择参考珠泪·水仙人鱼与别想逃：「是」从消耗堆选、「否」从弃牌堆选；
// 拥有能力「王之馆」的场合，这张卡免费打出。
public class HeLuSiXianDaoHaBiCard : YunoSpecialBaseCard
{
    private const string _retrieveCardCount = "RetrieveCardCount";

    public HeLuSiXianDaoHaBiCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(16m, ValueProp.Move),
        new DynamicVar(_retrieveCardCount, 2),
    };

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.HeLuSiGuaiShou,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.HeLuSiGuaiShou),
    ];

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || originalCost <= 0m) return false;
        if (!Owner.Creature.HasPower<WangZhiGuanPower>()) return false;
        modifiedCost = 0m;
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得16点格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);

        // 「是」→ 从消耗堆选，「否」→ 从弃牌堆选
        bool fromExhaust = await ToolCmd.AskYesNo(choiceContext, Owner, ChoicePrompt);

        var candidates = fromExhaust
            ? PileType.Exhaust.GetPile(Owner).Cards.ToList()
            : PileType.Discard.GetPile(Owner).Cards.ToList();
        if (candidates.Count == 0) return;

        // 将2张卡加入手牌（牌堆不足2张时按实际数量选）
        var selectedCards = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, Math.Min(DynamicVars[_retrieveCardCount].IntValue, candidates.Count)));

        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    private static LocString ChoicePrompt { get; } = new("card_selection", "TO_HE_LU_SI_XIAN_DAO_HA_BI_CHOICE");
}
