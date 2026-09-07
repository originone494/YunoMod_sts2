using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Relics;

namespace YunoMod.Scripts.Tool;

// 命运之轮：受到伤害时 1% 概率将伤害降为 0
// 所有伤害重载最终都汇入 CreatureCmd.Damage 的 7 参数核心方法，在此入口改写 amount 即真·归零
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    new[]
    {
        typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal),
        typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay)
    })]
public class WheelOfFortuneDamagePatch
{
    private const double _fortuneChance = 0.01;

    public static void Prefix(IEnumerable<Creature> targets, ref decimal amount)
    {
        if (amount <= 0) return;

        foreach (var target in targets)
        {
            var relic = target.Player?.Relics.OfType<Tarot10WheelOfFortuneRelic>().FirstOrDefault();
            if (relic == null) continue;

            // 逆位时 1% 免伤关闭
            if (relic.IsReversed) continue;

            // 每次伤害事件只掷一次骰（用持有者的杂项随机流，与存档/联机确定性一致）
            if (target.Player!.RunState.Rng.Niche.NextDouble() < _fortuneChance)
            {
                amount = 0;
                relic.NotifyProc();
            }
            break;
        }
    }
}
