using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位15-恶魔：首次死亡时复活满血，代价是每回合开始受诅咒，恩赐是攻击附带目标上限百分比伤害
// 逆位：敌人死亡后恢复至满血，且在敌人的回合开始时受到50%最大生命值的伤害，一场战斗仅触发一次
public class Tarot15TheDevilRelic : TarotRelicBase
{
    private const decimal _cursePercent = 0.49m;   // 正位：回合开始受到的最大生命值伤害
    private const decimal _bonusPercent = 0.10m;   // 正位：卡牌攻击附带的目标最大生命值伤害
    private const decimal _enemyCursePercent = 0.50m; // 逆位：被复活敌人回合开始受到的伤害

    // 契约是否已触发（一场游戏一次，随存档持久化）
    private bool _pactUsed;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool PactUsed
    {
        get => _pactUsed;
        set
        {
            AssertMutable();
            _pactUsed = value;
        }
    }

    private Creature? _pendingReviveCreature;   // ShouldDie 已标记待复活（死亡防止管线中血量仍≤0 会被补杀，需在钩子内处理）
    private Creature? _cursedEnemy;             // 逆位：被复活的敌人
    private bool _enemyCurseUsed;               // 逆位：一场战斗仅触发一次

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override bool SupportsReversed => true;

    public override bool ShouldDie(Creature creature)
    {
        if (IsReversed)
        {
            // 逆位：第一个死亡的敌人防止死亡（改为复活满血+受诅咒）
            if (!_enemyCurseUsed && creature.IsMonster && _pendingReviveCreature == null)
            {
                _pendingReviveCreature = creature;
                return false;
            }
            return base.ShouldDie(creature);
        }

        // 正位：自己首次死亡时防止死亡（雪白 XueBaiCard 同款防死管线）
        if (creature == Owner.Creature && !PactUsed && _pendingReviveCreature == null)
        {
            _pendingReviveCreature = creature;
            return false;
        }
        return base.ShouldDie(creature);
    }

    // 防死管线中血量仍≤0 会被直接补杀，必须在此回满血
    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != _pendingReviveCreature) return;
        _pendingReviveCreature = null;

        if (IsReversed)
        {
            _enemyCurseUsed = true;
            _cursedEnemy = creature;
            Flash();
            await CreatureCmd.Heal(creature, creature.MaxHp);
            return;
        }

        PactUsed = true;
        Flash();
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp);
    }

    // 正位诅咒：契约生效后，回合开始受到49%最大生命值的伤害（不可格挡）
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (IsReversed || !PactUsed) return;

        await CreatureCmd.Damage(choiceContext, Owner.Creature, Owner.Creature.MaxHp * _cursePercent, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }

    // 正位恩赐：契约生效后，卡牌攻击附带目标10%最大生命值的伤害（数值确定，预览正确显示）
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (IsReversed || !PactUsed) return 0m;
        if (dealer != Owner.Creature) return 0m;
        if (cardSource == null) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        if (target == null || !target.IsMonster) return 0m;

        return target.MaxHp * _bonusPercent;
    }

    // 逆位诅咒：敌人的回合开始时，被复活的敌人受到50%最大生命值的伤害（不可格挡）
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!IsReversed || _cursedEnemy == null) return;
        if (side == CombatSide.Player) return;
        if (!_cursedEnemy.IsAlive) return;

        Flash();
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), _cursedEnemy, _cursedEnemy.MaxHp * _enemyCursePercent, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        _cursedEnemy = null;   // 诅咒只结算一次
    }

    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        _pendingReviveCreature = null;
        _cursedEnemy = null;
        _enemyCurseUsed = false;
        return base.AfterCombatEnd(room);
    }
}
