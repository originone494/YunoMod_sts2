using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「强欲而金满之壶」（Pot of Extravagance）：向消耗堆加入 6 张稀有卡，抽 2 张牌。
// 稀有卡限定走 AddRandomCardsToExhaust 的可选 filter，随机流与灌注壶同款（Rng.Niche）。
public class QiangYuBingJinManZhiHuCard : YunoSpecialBaseCard
{
    private const int _rareCards = 6;
    private const int _drawCount = 2;

    public QiangYuBingJinManZhiHuCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 向消耗堆加入 6 张稀有卡（全卡池限定稀有度）
        await ToolCmd.AddRandomCardsToExhaust(Owner, _rareCards, 2f, c => c.Rarity == CardRarity.Rare);

        // 抽 2 张牌
        await CardPileCmd.Draw(choiceContext, _drawCount, Owner);
    }
}
