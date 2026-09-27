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

    public async Task OnBleedDamage(PlayerChoiceContext ctx, Creature target, int amount)
    {
        // 只结算自己身上的流血（多人下队友流血不触发；施加到敌人身上时同样只对该敌人本身生效）
        if (target != Owner) return;

        for (int i = 0; i < Amount; i++)
        {
            await PowerCmd.Apply<ZhiCanPower>(ctx, target, Amount, Owner, null);
        }
    }

}
