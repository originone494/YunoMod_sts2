using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Skill;

public class DuQiCard : AbstractTemplateBaseCard
{
    private const string _poisonPerTurnKey = "PoisonPerTurn";
    private const string _basePoisonKey = "BasePoison";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_basePoisonKey, 6m),
        new DynamicVar(_poisonPerTurnKey, 1m),
    ];

    public DuQiCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>[CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 对所有敌人施加中毒
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(enemy, (int)DynamicVars[_basePoisonKey].BaseValue, Owner.Creature, this);
        }

        // 永久增加下一次使用时的基础中毒层数
        DynamicVars[_basePoisonKey].BaseValue += DynamicVars[_poisonPerTurnKey].BaseValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_poisonPerTurnKey].UpgradeValueBy(1m);
    }
}
