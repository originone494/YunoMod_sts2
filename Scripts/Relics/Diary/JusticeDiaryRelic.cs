using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class JusticeDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    // 本场战斗被标记的敌人
    private Creature? _markedEnemy;

    // 本场战斗是否已出现第一个敌人死亡
    private bool _firstEnemyKilled;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);

            // 战斗开始时，标记一名随机敌人
            var enemies = combatState.HittableEnemies.Where(e => e.IsAlive).ToList();
            if (enemies.Count > 0)
            {
                var marked = Owner.RunState.Rng.CombatTargets.NextItem(enemies)!;
                _markedEnemy = marked;
                _firstEnemyKilled = false;

                // 给被标记的敌人挂上可见的标记能力
                await PowerCmd.Apply<JusticeDiaryMarkPower>(choiceContext, marked, 1, Owner.Creature, null);
            }
        }
    }

    // 若被标记的敌人第一个被杀死，则获得等同于持有日记数的最大生命值
    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!creature.IsMonster) return;
        if (_firstEnemyKilled) return;

        _firstEnemyKilled = true;

        if (creature != _markedEnemy) return;

        // 持有日记数按 DiaryPower 的层数计算
        decimal diaryCount = Owner.Creature.GetPower<DiaryPower>()?.Amount ?? 0m;
        if (diaryCount <= 0m) return;

        Flash();
        await CreatureCmd.GainMaxHp(Owner.Creature, diaryCount);
    }

    // 战斗结束复位标记状态
    public override Task AfterCombatEnd(CombatRoom room)
    {
        _markedEnemy = null;
        _firstEnemyKilled = false;
        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (result.UnblockedDamage <= 0) return;

        var combatState = Owner.Creature.CombatState;
        if (combatState == null || combatState.HittableEnemies.Count == 0) return;

        Flash();


        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), combatState.HittableEnemies, result.UnblockedDamage * 2, ValueProp.Unpowered, null, null, null);

    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
