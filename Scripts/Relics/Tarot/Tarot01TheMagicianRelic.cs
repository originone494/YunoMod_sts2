using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class Tarot01TheMagicianRelic : TarotRelicBase
{
    private const int _cleanseThreshold = 3;  // 3种类型：移除自身所有减益
    private const int _stripThreshold = 4;    // 4种类型：移除敌人所有增益
    private const int _energyThreshold = 5;   // 5种类型：下回合获得999点能量
    private const decimal _energyNextTurn = 999m;
    private const int _reversedThreshold = 2; // 逆位：类型数小于2
    private const decimal _reversedBlockLoss = 5m;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Creature.Side || !participants.Contains(Owner.Creature)) return;

        int typeCount = CountDistinctCardTypesPlayedThisTurn();

        if (IsReversed)
        {
            // 逆位：类型数小于2，失去5点格挡
            if (typeCount >= _reversedThreshold) return;
            Flash();
            await CreatureCmd.LoseBlock(new ThrowingPlayerChoiceContext(), Owner.Creature, _reversedBlockLoss, null);
            return;
        }

        if (typeCount < _cleanseThreshold) return;

        Flash();

        // 3种及以上：移除自身所有减益
        foreach (var power in Owner.Creature.Powers
                     .Where(p => p.TypeForCurrentAmount == PowerType.Debuff)
                     .ToList())
        {
            await PowerCmd.Remove(power);
        }

        // 4种及以上：移除所有敌人的所有增益
        if (typeCount < _stripThreshold) return;
        foreach (var enemy in Owner.Creature.CombatState!.HittableEnemies.ToList())
        {
            foreach (var power in enemy.Powers
                         .Where(p => p.TypeForCurrentAmount == PowerType.Buff)
                         .ToList())
            {
                await PowerCmd.Remove(power);
            }
        }

        // 5种及以上：下回合获得999点能量（原版能量下回合能力，充电电池/汇聚同款）
        if (typeCount < _energyThreshold) return;
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, _energyNextTurn, Owner.Creature, null);
    }

    // 直接查战斗历史，无需手动计数；自动打出的卡不计（与佩尔之眼/正位21-世界的口径一致）
    private int CountDistinctCardTypesPlayedThisTurn()
    {
        return CombatManager.Instance.History.CardPlaysFinished
            .Where(e => e.Actor == Owner.Creature
                     && e.HappenedThisTurn(Owner.Creature.CombatState)
                     && !e.CardPlay.IsAutoPlay)
            .Select(e => e.CardPlay.Card.Type)
            .Where(t => t is CardType.Attack or CardType.Skill or CardType.Power or CardType.Status or CardType.Curse)
            .Distinct()
            .Count();
    }
}
