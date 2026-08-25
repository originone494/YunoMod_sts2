using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class SearchDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();

        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 使用卡牌造成单体伤害时：目标获得「注视」，其他目标失去「注视」
    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner.Creature) return;   // 自己的攻击
        if (command.CardPlay?.Card == null) return;       // 必须是卡牌造成的（群体/能力伤害不触发）
        if (!command.IsSingleTargeted) return;            // 只对单体伤害生效（群体无法区分目标）

        var target = command.Results.SelectMany(r => r).FirstOrDefault()?.Receiver;
        if (target == null || !target.IsAlive) return;

        // 移除其他目标身上的注视
        foreach (var enemy in Owner.Creature.CombatState!.HittableEnemies)
        {
            if (enemy != target && enemy.HasPower<ZhuShiPower>())
            {
                await PowerCmd.Remove(enemy.GetPower<ZhuShiPower>()!);
            }
        }

        // 目标获得注视
        if (!target.HasPower<ZhuShiPower>())
            await PowerCmd.Apply<ZhuShiPower>(choiceContext, target, 1, Owner.Creature, null);
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }

}
