using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Cards.Skill;

public class NiXiangMingYunCard : YunoBaseCard
{

    private const string _ForeseeCount = "ForeseeCount";
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar(_ForeseeCount,10)
    ];

    public NiXiangMingYunCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Foresee),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ToolCmd.ForeseeAndDraw(choiceContext, Owner, DynamicVars[_ForeseeCount].IntValue, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_ForeseeCount].UpgradeValueBy(5);
    }
}
