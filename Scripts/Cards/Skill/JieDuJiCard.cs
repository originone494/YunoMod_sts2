using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Skill;

public class JieDuJiCard : AbstractTemplateBaseCard
{
    public JieDuJiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 计算有中毒的敌人数，获得对应费用
        int poisonedEnemyCount = 0;
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            if (enemy.GetPowerAmount<PoisonPower>() > 0)
            {
                poisonedEnemyCount++;
            }
        }

        if (poisonedEnemyCount > 0)
        {
            await PlayerCmd.GainEnergy(poisonedEnemyCount, Owner);
        }

        // 若房间内只有一个敌人，抽2张牌
        if (CombatState.HittableEnemies.Count == 1)
        {
            await CardPileCmd.Draw(choiceContext, 2, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
