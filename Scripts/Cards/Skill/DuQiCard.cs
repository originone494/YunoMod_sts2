using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class DuQiCard : YunoBaseCard
{
    private const string _poisonIncreaseKey = "PoisonPerTurn";
    private const string _totalPoisonKey = "TotalPoison";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_poisonIncreaseKey, 2m),
        new DynamicVar(_totalPoisonKey, 4m)
    };

    public DuQiCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<PoisonPower>(choiceContext, CombatState!.HittableEnemies, DynamicVars[_totalPoisonKey].BaseValue, Owner.Creature, this);

        DynamicVars[_totalPoisonKey].BaseValue += DynamicVars[_poisonIncreaseKey].BaseValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_poisonIncreaseKey].UpgradeValueBy(1m);
    }

}
