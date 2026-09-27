using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位07-战车：突破敌人格挡时造成击晕（击晕=原版引擎命令，先古卡「哨子」同款）
// 逆位：一场战斗一次，怪物自身的格挡被（你的攻击）突破时，自己获得1层易伤
public class Tarot07TheChariotRelic : TarotRelicBase
{
    private bool _vulnerableUsedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (IsReversed) return;
        if (command.Attacker != Owner.Creature) return;
        if (command.CardPlay?.Card == null) return;   // 仅卡牌攻击（与搜寻日记遗物口径一致）

        // 突破格挡 = 既消耗了格挡、又有伤害打进生命值
        var piercedEnemies = command.Results
            .SelectMany(r => r)
            .Where(r => r.Receiver is { IsAlive: true })
            .Where(r => r.BlockedDamage > 0 && r.UnblockedDamage > 0)
            .Select(r => r.Receiver!)
            .Distinct()
            .ToList();

        foreach (var enemy in piercedEnemies)
        {
            Flash();
            await CreatureCmd.Stun(enemy);
        }
    }

    // 逆位：一场战斗一次，怪物突破自己的格挡时，自己获得1层易伤
    public override async Task AfterBlockBroken(PlayerChoiceContext choiceContext, Creature target, Creature? breaker)
    {
        if (!IsReversed || _vulnerableUsedThisCombat) return;
        if (target != Owner.Creature) return;   // 只有自己的格挡被打破时触发（多人下队友格挡被破不影响）

        _vulnerableUsedThisCombat = true;
        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        _vulnerableUsedThisCombat = false;
        return base.AfterCombatEnd(room);
    }
}
