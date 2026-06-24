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
        if (cardPlay.Card.Owner.Creature == base.Owner && cardPlay.Card.Type == CardType.Attack)
        {
            for (int i = 0; i < Amount; i++)
            {
                if (cardPlay.Target != null)
                {
                    await PowerCmd.Apply<LiuXuePower>(choiceContext, cardPlay.Target, Amount, Owner, null);
                }
            }
        }
    }

}