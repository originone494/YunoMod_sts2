using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Patching.Models;
using GameHook = MegaCrit.Sts2.Core.Hooks.Hook;
using LingHuo = YunoMod.Scripts.Hook.LingHuoHook;

namespace YunoMod.Scripts.Tool;

public sealed class LingHuoDiscardPatch : IPatchMethod
{
    public static string PatchId => "yunomod_linghuo_discard";
    public static string Description => "Trigger LingHuo effects per-card when discarded";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(GameHook), nameof(GameHook.AfterCardDiscarded))];

    public static async void Postfix(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.HasModKeyword(YunoKeywords.LingHuo))
        {
            await LingHuo.LingHuoSpecial(choiceContext, card.Owner);
            await LingHuo.OnLingHuo(choiceContext, card.Owner);
        }
    }
}
