using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class YinGuoLvDaDianTangCard : YunoBaseCard
{
    private const string _diaryCount = "DiaryCount";
    public YinGuoLvDaDianTangCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {

    }
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
        new DynamicVar(_diaryCount, 1)
        };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromCard<QiuTiCard>(),
        HoverTipFactory.FromPower<DiaryPower>(),

    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        if (Owner.Creature.HasPower<DiaryPower>())
        {

            int amount = Owner.Creature.GetPowerAmount<DiaryPower>();
            var resultList = new List<CardPileAddResult>();
            for (int i = 0; i < amount; i++)
            {
                CardModel card = Owner.Creature.CombatState!.CreateCard<QiuTiCard>(Owner);
                var addResult = await CardPileCmd.Add(card, PileType.Draw);
                resultList.Add(addResult);
            }
            CardCmd.PreviewCardPileAdd(resultList, 2f);
        }

        await CardCmd.Exhaust(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
