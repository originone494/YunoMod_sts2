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
    public YinGuoLvDaDianTangCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {

    }
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
        new DynamicVar(_diaryCount, 1)
        };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromCard<QiuTiCard>(),
        HoverTipFactory.FromPower<DiaryPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<YinGuoLvDaDianTangPower>(choiceContext, Owner.Creature, DynamicVars[_diaryCount].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
