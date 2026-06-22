using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Power;

public class XianJiCard : YunoBaseCard
{

    private const string _XianJiCount = "XianJiCount";

    public XianJiCard() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_XianJiCount,1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LiuXuePower>(),
    ];

    

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<XianJiPower>(choiceContext, Owner.Creature, DynamicVars[_XianJiCount].IntValue, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[_XianJiCount].UpgradeValueBy(1);
    }
}
