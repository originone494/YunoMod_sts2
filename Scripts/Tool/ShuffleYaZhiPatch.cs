using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Custom;

namespace YunoMod.Scripts.Tool;

[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Hooks.Hook), nameof(MegaCrit.Sts2.Core.Hooks.Hook.ModifyShuffleOrder))]
public class ShuffleYaZhiPatch
{
    [HarmonyPrefix]
    static bool Prefix(ICombatState combatState, Player player, List<CardModel> cards, bool isInitialShuffle)
    {
        cards.RemoveAll(c => c.Tags.Contains(YunoTags.YaZhi));
        return true;
    }
}
