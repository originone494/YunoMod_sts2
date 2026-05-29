using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class DuQiCard : YunoBaseCard
{
    private const string _poisonPerTurnKey = "PoisonPerTurn";
    private const string _basePoisonKey = "TotalPoison";
    private const int _basePoison = 1;

    private int _currentPoison = 1;
    private int _increasedPoison;

    [SavedProperty]
    public int CurrentPoison
    {
        get => _currentPoison;
        set
        {
            AssertMutable();
            _currentPoison = value;
            DynamicVars[_basePoisonKey].BaseValue = _currentPoison;
        }
    }

    [SavedProperty]
    public int IncreasedPoison
    {
        get => _increasedPoison;
        set
        {
            AssertMutable();
            _increasedPoison = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_basePoisonKey, CurrentPoison),
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

        int totalPoison = DynamicVars[_basePoisonKey].IntValue;

        await PowerCmd.Apply<PoisonPower>(CombatState!.HittableEnemies, totalPoison, Owner.Creature, this);

        int increaseAmount = DynamicVars[_poisonPerTurnKey].IntValue;
        BuffFromPlay(increaseAmount);
        (DeckVersion as DuQiCard)?.BuffFromPlay(increaseAmount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_poisonPerTurnKey].UpgradeValueBy(1m);
    }

    private void BuffFromPlay(int extraAmount)
    {
        IncreasedPoison += extraAmount;
        CurrentPoison = _basePoison + IncreasedPoison;
    }
}
