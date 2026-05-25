using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Power;

public class CuiXueCard : YunoBaseCard
{

    private const string powerCount = "powerCount";
    public CuiXueCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(powerCount, 2)
    ];

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Dagger];

    

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LiuXuePower>(),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<CuiXuePower>(Owner.Creature, DynamicVars[powerCount].BaseValue, Owner.Creature, this);

        await ToolCmd.DaggerStance(choiceContext, Owner, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[powerCount].UpgradeValueBy(1);
    }
}
