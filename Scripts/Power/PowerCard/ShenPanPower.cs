using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

// 审判：施加在玩家身上时生效，施加在敌人身上时仅作为标记
[RegisterPower]
public class ShenPanPower : YunoBasePower
{
    private const decimal _judgedMultiplier = 1.5m;   // 对拥有审判的敌人
    private const decimal _unjudgedMultiplier = 0.5m; // 对没有审判的敌人

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 只有玩家身上的审判参与伤害计算，且仅作用于卡牌攻击伤害（与追踪/虚弱的口径一致）
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {

        if (dealer != Owner || !Owner.IsPlayer) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        if (target == null || !target.IsMonster) return 1m;


        return target.HasPower<ShenPanPower>() ? _judgedMultiplier : _unjudgedMultiplier;

    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!creature.IsMonster) return;
        if (creature != Owner) return;

        var randomEnemy = CombatState.RunState.Rng.CombatTargets.NextItem(base.Owner.CombatState!.HittableEnemies);

        if (randomEnemy == null) return;


        await PowerCmd.Apply<ShenPanPower>(choiceContext, randomEnemy, 1, Owner, null);

    }
}
