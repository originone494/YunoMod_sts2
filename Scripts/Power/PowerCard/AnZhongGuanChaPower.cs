using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class AnZhongGuanChaPower : YunoBasePower, IOnGetLove
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnGetLove(PlayerChoiceContext ctx, Player player, int amount)
    {
        if (Owner.Player == player)
            await CardPileCmd.Draw(ctx, Amount, Owner.Player);
    }
}