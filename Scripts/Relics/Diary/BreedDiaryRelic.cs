using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class BreedDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    private bool Used = false;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 检查打出的卡牌是否是能力牌
        if (cardPlay.Card.Type != CardType.Power)
        {
            return;
        }

        if (Used) return;

        Flash();

        // 创建相同能力牌的副本并添加到手牌
        CardModel clone = cardPlay.Card.CreateClone();
        clone.AddKeyword(CardKeyword.Ethereal);
        await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, Owner);

        Used = true;

    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }

    public override Task BeforeCombatStart()
    {
        Used = false;
        return Task.CompletedTask;
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
