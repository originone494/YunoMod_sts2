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

// 游戏王「金满而谦虚之壶」（Pot of Prosperity）：向消耗堆加入 6 张稀有卡。预知 6。
// 稀有卡灌注与「强欲并金满之壶」同款 filter 路径；预知走 ForeseeAndDraw。
public class JinManBingQianXuZhiHuCard : YunoSpecialBaseCard
{
    private const int _rareCards = 6;
    private const int _foreseeAmount = 6;

    public JinManBingQianXuZhiHuCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(YunoKeywords.Foresee),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 向消耗堆加入 6 张稀有卡（全卡池限定稀有度）
        await ToolCmd.AddRandomCardsToExhaust(Owner, _rareCards, 2f, c => c.Rarity == CardRarity.Rare);

        // 预知 6
        await ToolCmd.ForeseeAndDraw(choiceContext, Owner, _foreseeAmount, source: this);
    }
}
