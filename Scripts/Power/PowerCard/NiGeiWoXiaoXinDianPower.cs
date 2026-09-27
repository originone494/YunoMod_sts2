using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;


public class NiGeiWoXiaoXinDianPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 任一怪物获得力量时，自己镜像获得等量力量×层数。
    // 原实现写在 TryModifyPowerAmountReceived（同步数值钩子）里 fire-and-forget 施加，
    // 会在能力数值管线内嵌套改写能力集合，时序脆弱；改为在数值生效后的异步钩子中结算。
    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0) return;
        if (power is not StrengthPower) return;
        // 只响应怪物获得力量（自己/队友获得力量、以及自身镜像获得时都不触发，避免递归）
        if (!power.Owner.IsMonster) return;

        for (int i = 0; i < Amount; i++)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, amount, Owner, null);
        }
    }
}
