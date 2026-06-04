using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class ShouHuPower : YunoBasePower, IOnGetLove
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnGetLove(PlayerChoiceContext ctx, Player player, int amount)
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.CombatState!.HittableEnemies, base.Amount, ValueProp.Unpowered, null, null);

    }
}
