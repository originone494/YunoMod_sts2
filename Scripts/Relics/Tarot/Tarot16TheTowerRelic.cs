using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位16-塔：回合开始可选消耗1张手牌；三牌堆总数<3时杀死所有敌人
// 逆位：战斗开始时，将抽牌堆里的牌各复制一张加入弃牌堆
public class Tarot16TheTowerRelic : TarotRelicBase
{
    private const int _collapseThreshold = 3;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override bool SupportsReversed => true;

    // 逆位：战斗开始，抽牌堆每张牌复制一张加入弃牌堆
    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!IsReversed) return;
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;

        var drawCards = PileType.Draw.GetPile(Owner).Cards.ToList();
        if (drawCards.Count == 0) return;

        // 副本要放进「弃牌堆」这个战斗牌堆，必须用 CombatState.CreateCard 登记，
        // 不能用 RunState.CreateCard：后者造出的是牌组域的卡，不进 CombatState._allCards。
        // 入堆那一刻它的 Pile 还在牌组域、IsInCombat 为 false，能绕过 CardPileCmd.Add
        // 的「必须在 CombatState 中」校验；但落进弃牌堆后 IsInCombat 变成 true 而仍未被
        // 登记，就成了「游离卡牌」——之后任意一次洗牌（Shuffle → CardPileCmd.Add）都会抛
        // "XXX must be added to a CombatState before adding it to this pile"，
        // 导致回合循环死亡、战斗卡死（表现为没有结束回合按钮）。
        var addResults = new List<CardPileAddResult>();
        foreach (var card in drawCards)
        {
            var copy = combatState.CreateCard(card.CanonicalInstance, Owner);
            addResults.Add(await CardPileCmd.Add(copy, PileType.Discard));
        }
        CardCmd.PreviewCardPileAdd(addResults, 2f);
    }

    // 正位：回合开始可选消耗1张手牌（0或1张），然后判定塔之坍塌（逆位关闭）
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (IsReversed) return;
        if (player != Owner) return;

        // 1. 可选消耗：从手牌选0或1张消耗，取消即不消耗
        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 1) { Cancelable = true };
        var selected = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this);
        foreach (var card in selected)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        // 2. 塔之坍塌：抽牌堆+弃牌堆+手牌总数小于3时，杀死所有敌人
        int totalCards = CardPile.GetCards(Owner, PileType.Draw, PileType.Discard, PileType.Hand).Count();
        if (totalCards >= _collapseThreshold) return;

        var enemies = Owner.Creature.CombatState?.HittableEnemies.ToList();
        if (enemies == null || enemies.Count == 0) return;

        Flash();
        await CreatureCmd.Kill(enemies);
    }
}
