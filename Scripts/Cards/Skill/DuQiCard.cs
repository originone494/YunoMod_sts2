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
    private const string _poisonPerTurnKey = "PoisonPerTurn";
    private const string _totalPoisonKey = "TotalPoison";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_totalPoisonKey, 1m),
        new DynamicVar(_poisonPerTurnKey, 1m),
    };

    public DuQiCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        int totalPoison = DynamicVars[_totalPoisonKey].IntValue;

        await PowerCmd.Apply<PoisonPower>(choiceContext, CombatState!.HittableEnemies, totalPoison, Owner.Creature, this);

        int increaseAmount = DynamicVars[_poisonPerTurnKey].IntValue;
        BuffFromPlay(increaseAmount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_poisonPerTurnKey].UpgradeValueBy(1m);
    }

    private void BuffFromPlay(int extraAmount)
    {
        int newValue = DynamicVars[_totalPoisonKey].IntValue + extraAmount;
        DynamicVars[_totalPoisonKey].BaseValue = newValue;

        // 同步到牌组版本（DeckVersion 是独立实例，需要单独更新 DynamicVar）
        if (DeckVersion != null)
        {
            DeckVersion.DynamicVars[_totalPoisonKey].BaseValue = newValue;
        }
    }
}
