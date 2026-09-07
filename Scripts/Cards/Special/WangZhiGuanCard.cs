using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power.PowerCard;

namespace YunoMod.Scripts.Cards.Special;

// 王之馆：回合开始时「检索」1张「荷鲁斯怪兽」卡送入弃牌堆，之后将弃牌堆的荷鲁斯怪兽加入手牌；
// 「荷鲁斯怪兽」卡进入弃牌堆时，对随机敌人造成8点伤害。效果由 WangZhiGuanPower 承载。
public class WangZhiGuanCard : YunoSpecialBaseCard
{
    public WangZhiGuanCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WangZhiGuanPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<WangZhiGuanPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
