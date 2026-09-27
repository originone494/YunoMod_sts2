using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

public class DaiZuiADaiCard : YunoSpecialBaseCard
{
    public DaiZuiADaiCard() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target?.Monster == null || CombatState == null) return;

        var original = cardPlay.Target;
        var copy = await CreatureCmd.Add(
            original.Monster.CanonicalInstance.ToMutable(),
            CombatState,
            CombatSide.Enemy);

        await CreatureCmd.SetMaxHp(copy, original.MaxHp);
        await CreatureCmd.SetCurrentHp(copy, original.CurrentHp);
        await CreatureCmd.Stun(copy);

        var power = ModelDb.Power<DaiZuiADaiPower>().ToMutable();
        var daiZuiPower = (DaiZuiADaiPower)power;
        daiZuiPower.Copy = copy;
        daiZuiPower.Original = original;
        await PowerCmd.Apply(choiceContext, power, Owner.Creature, 1, Owner.Creature, this);
    }
}
