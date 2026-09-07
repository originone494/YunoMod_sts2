using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

public class ZhenZhuShiJieCard : YunoSpecialBaseCard
{
    public ZhenZhuShiJieCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ZhenZhuShiJiePower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 「检索」1张「珠泪」卡（珠泪=带珠泪标签且非珠泪融合卡），检索到的卡加入手牌
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.ZhuLei) && !c.Tags.Contains(YunoTags.ZhuLeiRongHe),
            p => p is YunoSpecialCardPool,
            1);

        // 施加能力：打出「珠泪」卡时对随机敌人造成9点伤害
        await PowerCmd.Apply<ZhenZhuShiJiePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
