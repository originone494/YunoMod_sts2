using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class ChangShiMingZiCard : YunoSpecialBaseCard
{
    public ChangShiMingZiCard() : base(1, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? selected = (await CardSelectCmd.FromHand(
            prefs: CardPrefs(this, SelectionScreenPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: card => card.Type == CardType.Attack && card.DynamicVars.ContainsKey("Damage"),
            source: this)).FirstOrDefault();

        if (selected == null || CombatState == null) return;

        // 使这张卡后续造成的每段伤害增加5点。
        selected.DynamicVars.Damage.BaseValue += 5m;

        // 追溯本场战斗中这张卡已经完成的每个攻击段，对当时的目标补5点伤害。
        var previousHits = CombatManager.Instance.History.Entries
            .OfType<DamageReceivedEntry>()
            .Where(entry => entry.Dealer == Owner.Creature
                         && entry.Receiver.IsMonster
                         && entry.CardSource != null
                         && entry.CardSource.Id == selected.Id)
            .ToList();

        foreach (var hit in previousHits)
        {
            if (!hit.Receiver.IsAlive) continue;

            await CreatureCmd.Damage(
                choiceContext,
                hit.Receiver,
                5m,
                ValueProp.Unpowered,
                Owner.Creature,
                this,
                cardPlay);
        }
    }
}
