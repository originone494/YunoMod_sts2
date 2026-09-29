using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class CiShiBiKeZhiRenCard : YunoSpecialBaseCard
{
    private CardModel? _chosenCard;

    public CiShiBiKeZhiRenCard() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _chosenCard = null;
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (hand.Count == 0) return;

        _chosenCard = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this)).FirstOrDefault();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || _chosenCard == null || CombatState == null) return;

        var selected = _chosenCard;
        if (PileType.Hand.GetPile(Owner).Cards.Contains(selected))
        {
            // 暂存选择，自动打出导致离开手牌时由 AfterCardChangedPiles 处理回手与群体伤害。
            await CardCmd.AutoPlay(choiceContext, selected, null);
        }

        _chosenCard = null;
    }

    // 目标卡离开手牌时立刻返回手牌，并对所有敌人造成8点伤害。
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card != _chosenCard || oldPileType != PileType.Hand || CombatState == null) return;
        if (PileType.Hand.GetPile(Owner).Cards.Contains(card)) return;

        await CardPileCmd.Add(card, PileType.Hand);

        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count > 0)
        {
            await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                enemies,
                8,
                ValueProp.Move,
                Owner.Creature,
                this,
                null);
        }
    }
}
