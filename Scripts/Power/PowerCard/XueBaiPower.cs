using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class XueBaiPower : YunoBasePower, IOnBleedDamage
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnBleedDamage(PlayerChoiceContext ctx, CombatState combatState, Creature target, int amount)
    {
        for (int i = 0; i < Amount; i++)
        {
            await PowerCmd.Apply<ZhiCanPower>(target, Amount, Owner, null);
        }
    }

}
