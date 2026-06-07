using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class WuYiZhongTingPower : YunoBasePower, IOnStanceChange
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnStanceChange(PlayerChoiceContext ctx, Player player,Stance oldStance, Stance newStance)
    {
        if (player == Owner.Player)
            await CardPileCmd.Draw(ctx, Amount, player);
    }
}