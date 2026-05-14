using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Skill;

public class XuZhaCard : AbstractTemplateBaseCard
{

    public XuZhaCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.ZhiCanPower];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int heavyInjuryAmount = Owner.Creature.GetPowerAmount<ZhiCanPower>();

        if (heavyInjuryAmount > 0)
        {
            await PowerCmd.Remove<ZhiCanPower>(Owner.Creature);
            await PowerCmd.Apply<ZhiCanPower>(cardPlay.Target, heavyInjuryAmount, Owner.Creature, this);
        }

        heavyInjuryAmount = cardPlay.Target.GetPowerAmount<ZhiCanPower>();

        if (heavyInjuryAmount > 0)
        {
            await PowerCmd.Apply<ZhiCanPower>(cardPlay.Target, heavyInjuryAmount, Owner.Creature, this);
        }




    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
