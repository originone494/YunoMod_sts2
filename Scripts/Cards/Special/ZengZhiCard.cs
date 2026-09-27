using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

public class ZengZhiCard : YunoSpecialBaseCard
{
    public ZengZhiCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target?.Monster == null || CombatState == null) return;

        var source = cardPlay.Target;
        var monster = source.Monster.CanonicalInstance.ToMutable();
        var copy = await CreatureCmd.Add(monster, CombatState, CombatSide.Enemy);

        await CreatureCmd.SetMaxHp(copy, source.MaxHp);
        await CreatureCmd.SetCurrentHp(copy, source.CurrentHp);
        await CreatureCmd.Stun(copy);

        var power = ModelDb.Power<ZengZhiPower>().ToMutable();
        ((ZengZhiPower)power).Copy = copy;
        await PowerCmd.Apply(choiceContext, power, Owner.Creature, 1, Owner.Creature, this);
    }
}
