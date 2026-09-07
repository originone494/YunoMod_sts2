using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class Tarot04TheEmperorRelic : TarotRelicBase
{
    private const decimal _tempAmount = 1m;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override bool SupportsReversed => true;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsAutoPlay) return;   // 自动打出的攻击/技能不触发（与正位01/06口径一致）
        if (cardPlay.Card?.Owner != Owner) return;
        if (Owner.Creature.CombatState == null) return;

        if (IsReversed)
        {
            // 逆位：攻击牌→所有敌人获得1点临时力量；技能牌→所有敌人失去1点临时力量
            var enemies = Owner.Creature.CombatState.HittableEnemies;
            if (enemies.Count == 0) return;

            if (cardPlay.Card.Type == CardType.Attack)
            {
                Flash();
                await PowerCmd.Apply<Tarot04TempStrengthPower>(choiceContext, enemies, _tempAmount, Owner.Creature, null);
            }
            else if (cardPlay.Card.Type == CardType.Skill)
            {
                Flash();
                await PowerCmd.Apply<Tarot04TempStrengthDownPower>(choiceContext, enemies, _tempAmount, Owner.Creature, null);
            }
            return;
        }

        // 正位：自己获得临时力量/临时敏捷
        if (cardPlay.Card.Type == CardType.Attack)
        {
            Flash();
            await PowerCmd.Apply<Tarot04TempStrengthPower>(choiceContext, Owner.Creature, _tempAmount, Owner.Creature, null);
        }
        else if (cardPlay.Card.Type == CardType.Skill)
        {
            Flash();
            await PowerCmd.Apply<Tarot04TempDexterityPower>(choiceContext, Owner.Creature, _tempAmount, Owner.Creature, null);
        }
    }
}
