using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Entities.Players;
using YunoMod.Scripts.Cards.Other;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using YunoMod.Scripts.Power;
namespace YunoMod.Scripts.Cards.Attack;

public class BieXiangTaoCard : YunoBaseCard
{
    private const string _GrtCardCount = "GrtCardCount";
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m,ValueProp.Move),
        new DynamicVar(_GrtCardCount, 1)
    ];

    public BieXiangTaoCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Axe];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Axe),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.AxeAttack(choiceContext, cardPlay.Target!, this, DynamicVars.Damage.BaseValue, cardPlay);

        await ToolCmd.AxeStance(choiceContext, Owner, this);


        // 从弃牌堆、消耗堆中选取牌加入手牌
        var candidates1 = PileType.Discard.GetPile(Owner).Cards
            .ToList();

        var candidates2 = PileType.Exhaust.GetPile(Owner).Cards
            .ToList();

        var candidates = candidates1;


        // 「注视」：可以从消耗堆中选择
        if (cardPlay.Target.HasPower<ZhuShiPower>() && await ToolCmd.AskYesNo(choiceContext, Owner, ChoicePrompt))
        {
            candidates = candidates2;
            if (candidates2.Count() == 0) return;
        }
        if (candidates1.Count() == 0) return;


        var selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, DynamicVars[_GrtCardCount].IntValue));

        foreach (var card in selectedCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }

    }

    private static LocString ChoicePrompt { get; } = new("card_selection", "TO_BIE_XIANGT_TAO_CHOICE");


    protected override void OnUpgrade()
    {
        DynamicVars[_GrtCardCount].UpgradeValueBy(1);
    }

}
