using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「强欲而谦虚之壶」（Pot of Duality）：预知 3。
// 与「金满并谦虚之壶」（预知6+灌注）同族的低配版，预知走 ForeseeAndDraw。
public class QiangYuBingQianXuZhiHuCard : YunoSpecialBaseCard
{
    private const int _foreseeAmount = 3;

    public QiangYuBingQianXuZhiHuCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(YunoKeywords.Foresee),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 预知 3
        await ToolCmd.ForeseeAndDraw(choiceContext, Owner, _foreseeAmount, source: this);
    }
}
