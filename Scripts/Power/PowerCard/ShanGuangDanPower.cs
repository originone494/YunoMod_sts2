using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class ShanGuangDanPower : YunoBasePower, IOnLingHuo
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        if (player == Owner.Player)
            await PlayerCmd.GainEnergy(Amount, player);
    }
}
