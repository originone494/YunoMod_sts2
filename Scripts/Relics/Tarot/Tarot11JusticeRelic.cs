using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Rooms;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位11-正义：卡牌单体伤害对相邻敌人造成50%伤害
// 逆位：敌人获得减益时，你获得1层相应的减益（一场战斗一次）
public class Tarot11JusticeRelic : TarotRelicBase
{
    private const decimal _splashRatio = 0.5m;

    private bool _mirroredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (IsReversed) return;
        if (command.Attacker != Owner.Creature) return;
        if (command.CardPlay?.Card == null) return;   // 仅卡牌伤害
        if (!command.IsSingleTargeted) return;

        var results = command.Results.SelectMany(r => r).ToList();
        var mainTarget = results.FirstOrDefault()?.Receiver;
        if (mainTarget == null || !mainTarget.IsMonster) return;

        decimal damageDealt = results
            .Where(r => r.Receiver == mainTarget)
            .Sum(r => r.TotalDamage);
        if (damageDealt <= 0) return;

        // 相邻敌人 = 敌人排列（CombatState.Enemies 顺序即战场位置）中目标左右两侧的存活敌人
        var enemies = Owner.Creature.CombatState?.Enemies;
        if (enemies == null) return;
        int index = -1;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == mainTarget) { index = i; break; }
        }
        if (index < 0) return;

        var neighbors = new List<Creature>();
        if (index - 1 >= 0) neighbors.Add(enemies[index - 1]);
        if (index + 1 < enemies.Count) neighbors.Add(enemies[index + 1]);
        if (neighbors.Count == 0) return;

        decimal splash = damageDealt * _splashRatio;
        Flash();
        foreach (var neighbor in neighbors)
        {
            if (!neighbor.IsHittable) continue;
            await CreatureCmd.Damage(choiceContext, neighbor, splash, ValueProp.Move, command.CardPlay.Card, command.CardPlay);
        }
    }

    // 逆位：敌人获得减益时，你获得1层相应的减益（一场战斗一次）
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!IsReversed || _mirroredThisCombat) return;
        if (amount <= 0) return;
        if (power.Owner == null || !power.Owner.IsMonster) return;   // 敌人获得的
        if (power.Type != PowerType.Debuff) return;

        _mirroredThisCombat = true;
        Flash();
        var mutable = ModelDb.DebugPower(power.GetType()).ToMutable();
        await PowerCmd.Apply(choiceContext, mutable, Owner.Creature, 1, Owner.Creature, null);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _mirroredThisCombat = false;
        return base.AfterCombatEnd(room);
    }
}
