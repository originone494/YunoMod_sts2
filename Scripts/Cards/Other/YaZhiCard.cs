using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace YunoMod.Scripts.Cards.Other;

public class YaZhiCard : YunoBaseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12,ValueProp.Move)
    ];


    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.YaZhi
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Axe];



    public YaZhiCard
    () : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(YunoKeywords.Axe),
        HoverTipFactory.FromKeyword(YunoKeywords.YaZhi),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.AxeAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue);

        await ToolCmd.AxeStance(choiceContext, Owner, this);
    }

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card == this && base.CombatState != null)
        {
            Creature? creature = Owner!.RunState.Rng.CombatTargets.NextItem(Owner.Creature.CombatState!.HittableEnemies);
            if (creature != null)
            {
                await CardCmd.AutoPlay(choiceContext, this, creature);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
