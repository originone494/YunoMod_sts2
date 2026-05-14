using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class GraffitiDiaryRelic : YunoBaseRelic
{
    private Dictionary<Creature, bool> _diedFromPoisonMap = new Dictionary<Creature, bool>();

    private int _savedPoisonCount;

    [SavedProperty]
    public int SavedPoisonCount
    {
        get { return _savedPoisonCount; }
        set { _savedPoisonCount = value; }
    }

    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeCombatStart()
    {
        _diedFromPoisonMap.Clear();
        
        if (Owner?.Creature == null)
        {
            return;
        }
        
        Flash();
        
        // 给予自己 1 层毒雾
        await PowerCmd.Apply<NoxiousFumesPower>(Owner.Creature, 1m, Owner.Creature, null);
        
        // 给予所有敌人 1 层中毒（加上永久值）
        if (Owner.Creature.CombatState != null)
        {
            await PowerCmd.Apply<PoisonPower>(Owner.Creature.CombatState.HittableEnemies, 1m, Owner.Creature, null);
        }
    }
}
