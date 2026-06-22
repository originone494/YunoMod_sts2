using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class CuiXuePower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(
    PlayerChoiceContext choiceContext,
    CardPlay cardPlay
)
    {
        if (cardPlay.Card.Owner.Creature == base.Owner && cardPlay.Card.Keywords.Contains(YunoKeywords.Dagger) && cardPlay.Card.Type == CardType.Attack)
        {
            for (int i = 0; i < Amount; i++)
            {
                if (cardPlay.Target != null)
                {
                    if (cardPlay.Target.HasPower<LiuXuePower>())
                    {
                        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), base.Owner, cardPlay.Target.GetPowerAmount<LiuXuePower>(), ValueProp.Unblockable | ValueProp.Unpowered, null, null);
                        await PowerCmd.Decrement(this);
                    }
                }
            }
        }
    }

}