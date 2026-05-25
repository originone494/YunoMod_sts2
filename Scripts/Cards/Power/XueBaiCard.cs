using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Attack;

public class XueBaiCard : YunoBaseCard
{
    private const string _ZhiCanPowerCount = "ZhiCanPowerCount";

    public XueBaiCard() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_ZhiCanPowerCount,1)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LiuXuePower>(),
        HoverTipFactory.FromPower<ZhiCanPower>(),
    ];

    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<XueBaiPower>(Owner.Creature, DynamicVars[_ZhiCanPowerCount].BaseValue, Owner.Creature, this);


    }

    protected override void OnUpgrade()
    {
        DynamicVars[_ZhiCanPowerCount].UpgradeValueBy(1);
    }
}
