using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Power;

public class LiangRenCard : YunoBaseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(2),
    ];

    public LiangRenCard() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Sword];


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.Sword),HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, (int)DynamicVars.Strength.BaseValue, Owner.Creature, this);


        var bladeCards = PileType.Draw.GetPile(Owner).Cards
            .Concat(PileType.Discard.GetPile(Owner).Cards)
            .Concat(PileType.Exhaust.GetPile(Owner).Cards)
            .Where(card =>
                card.Keywords.Contains(YunoKeywords.Sword)
            ).ToList();

        foreach (var card in bladeCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }

        await ToolCmd.SwordStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1);
    }
}
