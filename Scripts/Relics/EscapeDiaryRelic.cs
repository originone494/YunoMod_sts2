using System;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class EscapeDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;

        int limit = (int)Math.Sqrt(n);
        for (int i = 3; i <= limit; i += 2)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

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
        if (player != Owner) return;

        int diaryAmount = (int)Owner.Creature.GetPowerAmount<DiaryPower>();
        if (diaryAmount <= 0) return;

        // 检查是否有敌人意图攻击
        bool enemyAttacking = false;
        foreach (Creature enemy in Owner.Creature.CombatState!.HittableEnemies)
        {
            if (enemy.Monster?.IntendsToAttack == true)
            {
                enemyAttacking = true;
            }
        }

        int enemyAmount = Owner.Creature.CombatState!.HittableEnemies.Count();
        int blockAmount = enemyAttacking ? diaryAmount * 2 * enemyAmount : diaryAmount * enemyAmount;

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, blockAmount, ValueProp.Unpowered, null);
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
