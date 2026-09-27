using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「奥西里斯之天空龙」（Slifer the Sky Dragon）：
// 对所有敌人造成等同于手牌数×10的伤害，之后，击杀生命值在20及以下的所有敌人。
public class AoXiLiSiDeTianKongLongCard : YunoSpecialBaseCard
{
    private const int _damagePerHandCard = 10;
    private const int _executeThreshold = 20;

    public AoXiLiSiDeTianKongLongCard() : base(3, CardType.Attack, CardRarity.Ancient, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对所有敌人造成等同于手牌数×10的伤害
        int handCount = PileType.Hand.GetPile(Owner).Cards.Count;
        await DamageCmd.Attack(handCount * _damagePerHandCard)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_lightning")
            .Execute(choiceContext);

        // 击杀生命值在 20 及以下的所有敌人（伤害结算后再判定）
        var executeTargets = CombatState!.HittableEnemies
            .Where(e => e.IsAlive && e.CurrentHp <= _executeThreshold)
            .ToList();
        foreach (var enemy in executeTargets)
        {
            await CreatureCmd.Kill(enemy);
        }
    }
}
