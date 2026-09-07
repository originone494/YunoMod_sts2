using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

public class XingQiuGaiZaoCard : YunoSpecialBaseCard
{
    public XingQiuGaiZaoCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 「检索」1张能力牌；poolFilter 传 null 即不限卡池，候选为 ModelDb.AllCards（含所有职业的卡池）
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Type == CardType.Power,
            null,
            1);
    }
}
