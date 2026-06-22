using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Tool;
using STS2RitsuLib.Cards.DynamicVars;

namespace YunoMod.Scripts.Cards.Attack;

public class QingKongDanJiaCard : YunoBaseCard
{


    public QingKongDanJiaCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Gun];


    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1,ValueProp.Move),
        new RepeatVar(0),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Gun),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        if (DynamicVars.Repeat.IntValue > 0)
            await ToolCmd.GunAttackAllEnemy(choiceContext, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        await ToolCmd.GunStance(choiceContext, Owner, this);

    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == this.Owner && cardPlay.Card.Keywords.Contains(YunoKeywords.Gun))
        {
            DynamicVars.Repeat.BaseValue += 1;
        }
        return Task.CompletedTask;
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
