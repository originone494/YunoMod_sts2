using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class Tarot00TheFoolRelic : TarotRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override bool SupportsReversed => true;

    public override async Task AfterObtained()
    {
        // 1. 加 1 张铁波斩入卡组
        var added = await CardPileCmd.Add(Owner.RunState.CreateCard<IronWave>(Owner), PileType.Deck);
        CardCmd.PreviewCardPileAdd(added, 2f);

        // 2. 卡组中所有打击/防御变化为铁波斩
        // 永恒卡在卡组中不可变换（CardCmd.Transform 会抛异常），先过滤
        var transformations = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CardTag.Defend))
            .Where(c => c.IsTransformable)
            .Select(c => new CardTransformation(c, CreateIronWaveFromOriginal(c)))
            .ToList();

        await CardCmd.Transform(transformations, null);
    }

    // 逆位：第一回合铁波斩的费用增加1（本回合有效，回合结束自动恢复）
    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!IsReversed) return;
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;

        foreach (var card in CardPile.GetCards(Owner, PileType.Draw, PileType.Discard, PileType.Hand))
        {
            if (card is not IronWave) continue;
            int current = card.EnergyCost.GetWithModifiers(CostModifiers.All);
            card.EnergyCost.SetThisTurn(current + 1);
        }
    }

    // 参照原版 Claws：替换卡保留原卡的升级状态与附魔
    private CardModel CreateIronWaveFromOriginal(CardModel original)
    {
        CardModel replacement = Owner.RunState.CreateCard<IronWave>(Owner);

        if (original.IsUpgraded && replacement.IsUpgradable)
        {
            CardCmd.Upgrade(replacement);
        }

        if (original.Enchantment != null)
        {
            var enchantment = (EnchantmentModel)original.Enchantment.MutableClone();
            if (enchantment.CanEnchant(replacement))
            {
                CardCmd.Enchant(enchantment, replacement, enchantment.Amount);
            }
        }

        return replacement;
    }
}
