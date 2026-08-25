using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

// 注视：目标身上的单一标记，供其他卡牌联动
[RegisterPower]
public class ZhuShiPower : YunoBasePower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 被注视的目标在回合开始时失去全部格挡
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;

        if (Owner.Block > 0)
        {
            await CreatureCmd.LoseBlock(new ThrowingPlayerChoiceContext(), Owner, Owner.Block, null);
        }
    }
}
