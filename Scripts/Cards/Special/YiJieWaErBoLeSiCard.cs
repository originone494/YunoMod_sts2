using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

// 异解·瓦尔波勒斯：获得被动（回合开始时向消耗堆加入5张随机卡，之后可将1张「异解」卡加入手牌）
public class YiJieWaErBoLeSiCard : YunoSpecialBaseCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.YiJie),
        HoverTipFactory.FromKeyword(YunoKeywords.YiJieMoXian),
    ];
    public YiJieWaErBoLeSiCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [YunoTags.YiJie, YunoTags.YiJieMoXian];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<YiJieWaErBoLeSiPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
