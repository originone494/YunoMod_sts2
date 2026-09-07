using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Power.PowerCard;

namespace YunoMod.Scripts.Cards.Special;

// 荷鲁斯·荣光：丢弃1张手牌，将1张「王之馆」加入手牌并抽1张牌。
// 拥有能力「王之馆」的场合，这张卡免费打出（战斗中通过 TryModifyEnergyCostInCombat 将费用置 0）。
public class HeLuSiRongGuangCard : YunoSpecialBaseCard
{
    public HeLuSiRongGuangCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.Self)
    {
    }

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
        // 丢弃1张手牌（玩家选择；手牌为空则跳过丢弃，后续效果照常执行）
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (handCards.Count > 0)
        {
            var selected = (await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
                context: choiceContext,
                player: Owner,
                filter: null,
                source: this)).ToList();
            if (selected.Count > 0)
            {
                await CardCmd.Discard(choiceContext, selected[0]);
            }
            else
            {
                return;
            }
        }

        // 将1张「王之馆」加入手牌
        CardModel wangZhiGuan = Owner.Creature.CombatState!.CreateCard<WangZhiGuanCard>(Owner);
        var addResult = await CardPileCmd.AddGeneratedCardToCombat(wangZhiGuan, PileType.Hand, Owner);
        CardCmd.PreviewCardPileAdd(addResult, 2f);

        // 抽1张牌
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
}
