using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

public class JinJiDeShengGuanCard : YunoSpecialBaseCard
{
    public JinJiDeShengGuanCard() : base(0, CardType.Skill, CardRarity.Ancient, CustomTargetType.Anyone)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 目标可以是任意存活的友方或敌方（CustomTargetType.Anyone）
        foreach (var target in this.GetTargets(cardPlay.Target))
        {
            await PowerCmd.Apply<JinJiDeShengGuanPower>(choiceContext, target, 1, Owner.Creature, this);
        }
    }
}
