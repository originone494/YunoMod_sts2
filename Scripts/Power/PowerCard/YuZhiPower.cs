using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class YuZhiPower : YunoBasePower, IOnForesee
{

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public Task OnForesee(PlayerChoiceContext ctx, Player player, int amount, int discardedAmount)
    {
        if (player.Creature != Owner)
            return Task.CompletedTask;

        foreach (var enemy in player.Creature.CombatState!.HittableEnemies)
        {
            PowerCmd.Apply<StrengthPower>(enemy, Amount, Owner, null);
            PowerCmd.Apply<StrengthPower>(enemy, -2 * Amount, Owner, null);
        }
        return Task.CompletedTask;
    }
}
