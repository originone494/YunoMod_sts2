using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Hook;

public interface IOnBleedDamage
{
    Task OnBleedDamage(PlayerChoiceContext ctx, CombatState combatState, Creature target, int amount);
}

public static class BleedHook
{
    public static async Task OnBleedDamage(PlayerChoiceContext ctx, CombatState combatState, Creature target, int amount)
    {
        if (combatState == null) return;

        foreach (var model in combatState.IterateHookListeners().OfType<IOnBleedDamage>())
        {
            var abstractModel = (AbstractModel)(object)model;
            ctx.PushModel(abstractModel);
            await model.OnBleedDamage(ctx, combatState, target, amount);
            ctx.PopModel(abstractModel);
        }
    }

}
