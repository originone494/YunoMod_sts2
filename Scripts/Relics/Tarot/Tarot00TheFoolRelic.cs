using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位00-愚者：卡组中所有的打击/防御费用为0
// 逆位：第一回合，打击/防御的费用增加1
public class Tarot00TheFoolRelic : TarotRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override bool SupportsReversed => true;

    // 正位：打击/防御费用为0
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (IsReversed) return false;
        if (card.Owner != Owner) return false;
        if (!IsStrikeOrDefend(card)) return false;

        modifiedCost = 0m;
        return true;
    }

    // 逆位：第一回合，打击/防御的费用增加1（本回合有效，回合结束自动恢复）
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!IsReversed) return Task.CompletedTask;
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return Task.CompletedTask;

        foreach (var card in CardPile.GetCards(Owner, PileType.Draw, PileType.Discard, PileType.Hand))
        {
            if (!IsStrikeOrDefend(card)) continue;
            int current = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            card.EnergyCost.SetThisTurn(current + 1);
        }
        return Task.CompletedTask;
    }

    private static bool IsStrikeOrDefend(CardModel card)
        => card.Tags.Contains(CardTag.Strike) || card.Tags.Contains(CardTag.Defend);
}