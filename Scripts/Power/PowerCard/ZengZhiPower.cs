using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class ZengZhiPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 每次施加都是独立实例：同一战斗打出多张「增值」时，各实例分别追踪自己的复制体，
    // 不会按 Id 合并到已有实例上导致后续复制体失去链接。
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public Creature? Copy { get; set; }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Enemy || Copy == null || !Copy.IsAlive) return;

        // 每个敌人回合开始时重新设为眩晕，使复制品没有攻击意图且不会行动。
        await CreatureCmd.Stun(Copy);
    }

    // 玩家回合开始时，游戏会为所有敌人重掷行动并刷新意图（PrepareForNextTurn），
    // 复制体会在玩家回合内短暂显示普通攻击意图；此处在该流程之后（Hook.AfterSideTurnStart
    // 晚于 PrepareForNextTurn）重新眩晕，将意图压回眩晕状态。
    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && Copy?.IsAlive == true)
        {
            await CreatureCmd.Stun(Copy);
        }
    }

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (wasRemovalPrevented || Copy == null || !Copy.IsAlive) return;
        if (creature == Copy || !creature.IsMonster || creature.Side != Copy.Side) return;

        await CreatureCmd.Kill(Copy);
    }
}
