using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;

namespace YunoMod.Scripts.Cards.Other;

public class QiDaoCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {

    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public QiDaoCard() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [

    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        int amount = PileType.Discard.GetPile(Owner).Cards.ToList().Count();

        for (int i = 0; i < amount; i++)
        {
            CardModel cardModel = PileType.Discard.GetPile(Owner).Cards.ToList().FirstOrDefault()!;
            if (cardModel != null)
            {
                await CardPileCmd.Add(cardModel, PileType.Draw);

                CardCmd.Preview(cardModel);
            }
        }

        await ToolCmd.DuiMu(choiceContext, Owner, PileType.Draw.GetPile(Owner).Cards.ToList().Count());
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
