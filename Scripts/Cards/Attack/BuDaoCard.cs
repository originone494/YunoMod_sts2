using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

using STS2RitsuLib.Keywords;
namespace YunoMod.Scripts.Cards.Attack;

public class BuDaoCard : YunoBaseCard
{
    private const string _thresholdKey = "Threshold";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(1m, ValueProp.Move),
        new RepeatVar(3),
        new DynamicVar(_thresholdKey, 20m),
    };

    public BuDaoCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Gun];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.Gun),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.GunAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        int threshold = (int)DynamicVars[_thresholdKey].BaseValue;

        if (cardPlay.Target.IsAlive &&
            cardPlay.Target.GetPowerAmount<WeakPower>() > 0 &&
            cardPlay.Target.CurrentHp <= threshold)
        {
            await CreatureCmd.Kill(cardPlay.Target);
        }

        await ToolCmd.GunStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_thresholdKey].UpgradeValueBy(10m);
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
