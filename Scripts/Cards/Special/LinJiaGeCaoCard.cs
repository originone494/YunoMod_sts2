using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class LinJiaGeCaoCard : YunoSpecialBaseCard
{
    public LinJiaGeCaoCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 将抽牌堆的所有卡送入弃牌堆
        var drawPile = PileType.Draw.GetPile(Owner);
        var cards = drawPile.Cards.ToList();
        await CardCmd.Discard(choiceContext, cards);
    }
}
