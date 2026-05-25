using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Other;

public class ZhiShiYuanBoCard : YunoBaseCard
{



    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [

    ];

    public ZhiShiYuanBoCard() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Retriever)

    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ToolCmd.RetrieverAnyCard(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
