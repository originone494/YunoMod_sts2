using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Cards.Attack;

public class YanHuSheJiCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, ValueProp.Move),
        new RepeatVar(3),
        new PowerVar<WeakPower>(1m),
    ];

    public YanHuSheJiCard() : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }



    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Gun];


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Gun),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),

    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.GunAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        await PowerCmd.Apply<WeakPower>(cardPlay.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);

        await ToolCmd.GunStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
        DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
