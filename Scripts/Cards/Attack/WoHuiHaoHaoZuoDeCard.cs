using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
using MegaCrit.Sts2.Core.Commands.Builders;
namespace YunoMod.Scripts.Cards.Attack;

public class WoHuiHaoHaoZuoDeCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BasePow", 4m),
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
        {
            if (card.Owner == null) return 0;
            decimal baseVal = card.DynamicVars["BasePow"].BaseValue;
            bool inSword = card.Owner.Creature.HasPower<SwordPower>();
            return !inSword ? baseVal : (decimal)Math.Pow((double)baseVal, 2.5);
        }),
    ];

    public WoHuiHaoHaoZuoDeCard() : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Sword];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Sword),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        decimal baseVal = DynamicVars["BasePow"].BaseValue;
        bool inSword = Owner.Creature.HasPower<SwordPower>();
        decimal damage = !inSword ? baseVal : (decimal)Math.Pow((double)baseVal, 2.5);

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);

        await ToolCmd.SwordStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BasePow"].UpgradeValueBy(1m);
    }

}
