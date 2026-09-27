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

    public async Task OnForesee(PlayerChoiceContext ctx, Player player, int amount, int discardedAmount)
    {
        if (player.Creature != Owner)
            return;

        foreach (var enemy in player.Creature.CombatState!.HittableEnemies)
        {
            // 逐个 await，保证施加顺序在锁步下两端一致（不使用 fire-and-forget）
            await PowerCmd.Apply<JiuShiNiTempDownPower>(new ThrowingPlayerChoiceContext(), enemy, 4 * Amount, Owner, null);
        }
    }
}
