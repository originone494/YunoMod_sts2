using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

// 正位19-太阳：战斗开始诅咒/状态卡送去弃牌堆，抽到时消耗
// 逆位：战斗开始时，将所有诅咒牌放至抽牌堆顶端
public class Tarot19TheSunRelic : TarotRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;

        if (IsReversed)
        {
            // 逆位：所有诅咒牌移到抽牌堆顶端（从任意牌堆收集）
            var curses = CardPile.GetCards(Owner, PileType.Draw, PileType.Discard, PileType.Hand)
                .Where(c => c.Type == CardType.Curse)
                .ToList();
            foreach (var curse in curses)
            {
                await CardPileCmd.Add(curse, PileType.Draw, CardPilePosition.Top);
            }
            if (curses.Count > 0) Flash();
            return;
        }

        // 正位：诅咒/状态卡送去弃牌堆
        Flash();

        var cards = PileType.Draw.GetPile(Owner).Cards.Where(c => c.Type == CardType.Curse || c.Type == CardType.Status).ToList();

        foreach (var cardModel in cards)
        {
            await CardCmd.Discard(choiceContext, cardModel);

            CardCmd.Preview(cardModel);
        }
    }

    // 正位：抽到诅咒/状态卡时消耗（逆位关闭）
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (IsReversed) return;
        if (card.Type == CardType.Curse || card.Type == CardType.Status)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
    }
}
