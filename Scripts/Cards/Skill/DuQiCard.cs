using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Utils;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class DuQiCard : YunoBaseCard
{
    private const string _poisonPerTurnKey = "PoisonPerTurn";
    private const int _basePoison = 6;

    public static readonly SavedAttachedState<DuQiCard, int> PermanentBonus = new("PermanentBonus", _ => 0);

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(1m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered).WithMultiplier((card, _) =>
        {
            if (card is not DuQiCard duQiCard) return _basePoison;
            return _basePoison + PermanentBonus[duQiCard];
        }),
        new DynamicVar(_poisonPerTurnKey, 1m),

    };

    public DuQiCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        int totalPoison = _basePoison + PermanentBonus[this];

        await PowerCmd.Apply<PoisonPower>(CombatState!.HittableEnemies, totalPoison, Owner.Creature, this);

        PermanentBonus[this] += DynamicVars[_poisonPerTurnKey].IntValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_poisonPerTurnKey].UpgradeValueBy(1m);
    }
}
