using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;

namespace YunoMod.Scripts.Cards.Special;

// 异解·审判：效果未定，占位卡（先古/能力/异解·异解魔陷）
public class YiJieShenPanCard : YunoSpecialBaseCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.YiJie),
        HoverTipFactory.FromKeyword(YunoKeywords.YiJieMoXian),
    ];
    public YiJieShenPanCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [YunoTags.YiJie, YunoTags.YiJieMoXian];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 效果未定，先放着
        return Task.CompletedTask;
    }
}
