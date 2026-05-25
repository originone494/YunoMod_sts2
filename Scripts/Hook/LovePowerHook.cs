using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Hook;

public interface IOnGetLove
{
    Task OnGetLove(PlayerChoiceContext ctx, Player player,int amount);
}

public static class LovePowerHook
{
    public static async Task OnGetLove(PlayerChoiceContext ctx, Player player,int amount)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;
        
        foreach (var model in combatState.IterateHookListeners().OfType<IOnGetLove>())
        {
            var abstractModel = (AbstractModel)(object)model;
            ctx.PushModel(abstractModel);
            await model.OnGetLove(ctx, player, amount);
            ctx.PopModel(abstractModel);
        }
    }
}
