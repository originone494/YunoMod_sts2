using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

// 正位20-审判：战斗开始时随机敌人获得审判（详见审判能力）
// 逆位：正位的效果暂不生效
public class Tarot20JudgementRelic : TarotRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (IsReversed) return;
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;

        var randomEnemy = base.Owner.RunState.Rng.CombatTargets.NextItem(base.Owner.Creature.CombatState!.HittableEnemies);

        Flash();
        await PowerCmd.Apply<ShenPanPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, null);
        if (randomEnemy != null)
            await PowerCmd.Apply<ShenPanPower>(choiceContext, randomEnemy, 1, base.Owner.Creature, null);
    }
}
