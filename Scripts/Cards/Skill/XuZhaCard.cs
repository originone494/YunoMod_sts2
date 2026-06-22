using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Skill;

public class XuZhaCard : YunoBaseCard
{

    public XuZhaCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ZhiCanPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int heavyInjuryAmount = Owner.Creature.GetPowerAmount<ZhiCanPower>();

        if (heavyInjuryAmount > 0)
        {
            await PowerCmd.Remove<ZhiCanPower>(Owner.Creature);
            await PowerCmd.Apply<ZhiCanPower>(choiceContext, cardPlay.Target, heavyInjuryAmount, Owner.Creature, this);
        }

        if (cardPlay.Target.HasPower<ZhiCanPower>())
        {
            heavyInjuryAmount = cardPlay.Target.GetPowerAmount<ZhiCanPower>();

            if (heavyInjuryAmount > 0)
            {
                await PowerCmd.Apply<ZhiCanPower>(choiceContext, cardPlay.Target, heavyInjuryAmount, Owner.Creature, this);
            }
        }

    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
