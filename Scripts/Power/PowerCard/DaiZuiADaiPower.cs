using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
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

[RegisterPower]
public class DaiZuiADaiPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 每次施加都是独立实例：同一战斗打出多张「代罪阿呆」时，各实例分别追踪自己的复制体，
    // 不会按 Id 合并到已有实例上导致后续复制体失去链接。
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public Creature? Copy { get; set; }
    public Creature? Original { get; set; }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Enemy && Copy?.IsAlive == true)
        {
            await CreatureCmd.Stun(Copy);
        }
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

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Copy || Original == null || !Original.IsAlive || result.TotalDamage <= 0) return;

        await CreatureCmd.Damage(
            choiceContext,
            Original,
            result.TotalDamage,
            ValueProp.Unpowered,
            null,
            null);
    }
}
