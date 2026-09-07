using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Skill;

// 我锤：移除所有敌人的格挡；这个回合内所有敌人的力量变为负数（敌方回合结束自动还原）
public class WoChuiCard : YunoBaseCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public WoChuiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = Owner.Creature.CombatState?.HittableEnemies;
        if (enemies == null) return;

        foreach (var enemy in enemies)
        {
            // 移除格挡
            if (enemy.Block > 0)
            {
                await CreatureCmd.LoseBlock(choiceContext, enemy, enemy.Block, this.Owner.Creature);
            }

            // 力量变为负数：按当前正数力量 S 施加 2S 点临时力量损失（S → -S），敌方回合结束自动 +2S 还原
            var strength = enemy.GetPower<StrengthPower>();
            if (strength is { Amount: > 0 })
            {
                await PowerCmd.Apply<WoChuiTempStrengthDownPower>(choiceContext, enemy, 2 * strength.Amount, this.Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
