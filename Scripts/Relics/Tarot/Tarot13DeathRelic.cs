using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位13-死神：攻击斩杀血量低于10%的敌人并获得25金币
// 逆位：攻击未能对敌人造成伤害时，失去2金币
public class Tarot13DeathRelic : TarotRelicBase
{
    private const decimal _executeThreshold = 0.10m;
    private const decimal _goldPerExecute = 25m;
    private const decimal _goldPerMiss = 2m;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner.Creature) return;
        if (command.CardPlay?.Card == null) return;

        var results = command.Results.SelectMany(r => r).ToList();

        if (IsReversed)
        {
            // 逆位：攻击未能对敌人造成伤害（有结果但全部为0伤害），失去2金币
            if (results.Count == 0 || results.Any(r => r.TotalDamage > 0)) return;
            Flash();
            await PlayerCmd.LoseGold(_goldPerMiss, Owner, GoldLossType.Lost);
            return;
        }

        // 正位：斩杀血量低于10%的敌人并获得金币
        var hitEnemies = results
            .Where(r => r.Receiver.IsMonster)
            .Select(r => r.Receiver)
            .Distinct()
            .Where(e => e.IsAlive && e.IsHittable)
            .Where(e => e.CurrentHp < e.MaxHp * _executeThreshold)
            .ToList();
        if (hitEnemies.Count == 0) return;

        Flash();
        foreach (var enemy in hitEnemies)
        {
            await CreatureCmd.Kill(enemy);
            await PlayerCmd.GainGold(_goldPerExecute, Owner);
        }
    }
}
