using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Hook;

public interface IOnForesee
{
    Task OnForesee(PlayerChoiceContext ctx, Player player, int amount, int discardedAmount);
}

public static class ForeseeHook
{
    public static async Task OnForesee(PlayerChoiceContext ctx, Player player, int amount, int discardedAmount)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;
        
        foreach (var model in combatState.IterateHookListeners().OfType<IOnForesee>())
        {
            var abstractModel = (AbstractModel)(object)model;
            ctx.PushModel(abstractModel);
            await model.OnForesee(ctx, player, amount, discardedAmount);
            ctx.PopModel(abstractModel);
        }
    }
}
