using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

// 异解·审判：能力。洗牌时，从消耗堆取回1张「异解」卡加入手牌；
// 打出「异解」卡时，若抽牌堆数量为0，所有敌人在本回合失去99点力量。
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

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得「异解·审判」能力：洗牌时取回「异解」卡、打出「异解」卡且抽牌堆为0时让所有敌人本回合失去99点力量
        await PowerCmd.Apply<YiJieShenPanPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
