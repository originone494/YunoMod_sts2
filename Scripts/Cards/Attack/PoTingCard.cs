using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
using MegaCrit.Sts2.Core.CardSelection;
namespace YunoMod.Scripts.Cards.Attack;

public class PoTingCard : YunoBaseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(1m, ValueProp.Move),
        new RepeatVar(3),
    };

    public PoTingCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Gun];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Gun),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.GunAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        List<CardModel> drawPile = PileType.Draw.GetPile(Owner).Cards.Where(card => card.Type == CardType.Attack).ToList();
        IEnumerable<CardModel> drawCard = await CardSelectCmd.FromSimpleGrid(choiceContext, drawPile, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1, 1));
        if (drawCard.Count() > 0)
        {
            await CardPileCmd.Add(drawCard, PileType.Hand);

            List<CardModel> selectedCards = [.. await CardSelectCmd.FromHandForDiscard(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this
        )];

            int actualDiscardCount = selectedCards.Count;
            if (actualDiscardCount > 0)
            {
                await CardCmd.Discard(choiceContext, selectedCards);
            }
        }




        await ToolCmd.GunStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
