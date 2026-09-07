using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Power.PowerCard;

namespace YunoMod.Scripts.Cards.Special;

// 荷鲁斯·加护：抽取手牌数量张牌；拥有能力「王之馆」的场合，这张卡免费打出。
// 抽牌数用 RitsuLib 计算动态变量（ModCardVars.Computed）实现：
// 预览（本卡在手牌中）显示当前手牌数（含自身）；结算时本卡已先移入场上堆，
// 实际值 = 当前手牌数 + 1 补回自身，两处口径一致（5 张手牌打出即抽 5 张）。
public class HeLuSiJiaHuCard : YunoSpecialBaseCard
{
    public HeLuSiJiaHuCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Computed(
            "Cards",
            1,
            // 实时值：结算时本卡已不在手牌，+1 补回自身
            static card => card?.Owner == null ? 0m : PileType.Hand.GetPile(card.Owner).Cards.Count + 1,
            // 预览值：本卡在手牌中，直接显示当前手牌数（含自身）
            static (card, mode, target, runGlobalHooks) =>
                card?.Pile?.Type == PileType.Hand ? card.Pile.Cards.Count : 1),
    ];

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
        // 读取计算值：结算时本卡已离开手牌，求值结果 = 当前手牌数 + 1，与预览口径一致
        int drawCount = (int)DynamicVars.EvaluateValueOrDefault("Cards");
        await CardPileCmd.Draw(choiceContext, drawCount, Owner);
    }
}
