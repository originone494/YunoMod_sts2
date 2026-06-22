using System.Threading.Tasks;
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

        _triggered = true;
        Flash();
        await PowerCmd.Apply<BufferPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
    }
}
