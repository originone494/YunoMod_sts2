using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class DiamondRingRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    private bool _triggered;

    public override Task BeforeCombatStart()
    {
        _triggered = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (_triggered) return;
        if (delta >= 0) return;
        if (creature != Owner.Creature) return;
        // 战斗外掉血（事件、诅咒等）不触发：此时 CombatState 为 null，且效果本就只应在战斗内结算
        if (!CombatManager.Instance.IsInProgress) return;

        _triggered = true;
        Flash();
        await PowerCmd.Apply<BufferPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), Owner.Creature.CombatState!.HittableEnemies, 1, Owner.Creature, null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), Owner.Creature.CombatState!.HittableEnemies, 1, Owner.Creature, null);

    }
}
