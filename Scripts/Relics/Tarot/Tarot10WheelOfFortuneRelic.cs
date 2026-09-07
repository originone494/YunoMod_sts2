using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class Tarot10WheelOfFortuneRelic : TarotRelicBase
{
    private const double _intangibleChance = 0.01;        // 逆位：每个敌人1%获得无实体

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    // 正位：回合开始随机获得1-6点格挡；逆位：每个敌人1%概率获得1层无实体
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        if (IsReversed)
        {
            var enemies = Owner.Creature.CombatState?.HittableEnemies;
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (Owner.RunState.Rng.Niche.NextDouble() >= _intangibleChance) continue;
                Flash();
                await PowerCmd.Apply<IntangiblePower>(choiceContext, enemy, 1, Owner.Creature, null);
            }
            return;
        }

        int amount = Owner.RunState.Rng.Niche.NextInt(1, 7);   // 1-6
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Unpowered, null);
    }

    // 供 WheelOfFortuneDamagePatch 在判定命中时调用，遗物闪光提示
    public void NotifyProc()
    {
        Flash();
    }
}
