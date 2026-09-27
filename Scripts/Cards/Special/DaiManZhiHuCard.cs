using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「怠慢之壶」（Pot of Sloth）：战斗开始时加入手牌，依据敌人数量抽取卡牌。
// "战斗开始时加入手牌"用引擎原生关键字 Innate（固有）：首回合抽牌前把本卡移到抽牌堆顶
// 并保证起手抽到它，无需自定义钩子。
public class DaiManZhiHuCard : YunoSpecialBaseCard
{
    public DaiManZhiHuCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 依据敌人数量抽取卡牌
        int enemyCount = CombatState!.HittableEnemies.Count;
        if (enemyCount > 0)
        {
            await CardPileCmd.Draw(choiceContext, enemyCount, Owner);
        }
    }
}
