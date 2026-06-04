using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Hook;

public interface IOnLingHuo
{
    Task OnLingHuo(PlayerChoiceContext ctx, Player player);
    Task LingHuoSpecial(PlayerChoiceContext ctx, Player player);

}

public static class LingHuoHook
{
    public static async Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        foreach (var model in combatState.IterateHookListeners().OfType<IOnLingHuo>())
        {
            var abstractModel = (AbstractModel)(object)model;
            ctx.PushModel(abstractModel);
            await model.OnLingHuo(ctx, player);
            ctx.PopModel(abstractModel);
        }
    }

    public static async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        if (card is IOnLingHuo listener)
        {
            await listener.LingHuoSpecial(ctx, player);
        }
    }

}
