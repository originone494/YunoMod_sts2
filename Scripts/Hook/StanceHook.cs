using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Hook;

public interface IOnStanceChange
{
    Task OnStanceChange(PlayerChoiceContext ctx, Player player, Stance oldStance,Stance newStance);
}

public static class StanceHook
{
    public static async Task OnStanceChange(PlayerChoiceContext ctx, Player player, Stance oldStance, Stance newStance)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        foreach (var model in combatState.IterateHookListeners().OfType<IOnStanceChange>())
        {
            var abstractModel = (AbstractModel)(object)model;
            ctx.PushModel(abstractModel);
            await model.OnStanceChange(ctx, player, oldStance,newStance);
            ctx.PopModel(abstractModel);
        }
    }
}
