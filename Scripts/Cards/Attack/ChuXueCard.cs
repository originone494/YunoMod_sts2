using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Attack;

public class ChuXueCard : AbstractTemplateBaseCard
{
    private const string _LiuXuePowerKey = "LiuXueCount";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new DynamicVar(_LiuXuePowerKey, 4m),
    ];

    public ChuXueCard() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {

    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Dagger];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.DaggerAttack(choiceContext, cardPlay, DynamicVars.Damage.BaseValue, this, Owner.Creature);

        await PowerCmd.Apply<LiuXuePower>(cardPlay.Target!, DynamicVars[_LiuXuePowerKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_LiuXuePowerKey].UpgradeValueBy(2m);
    }
}
