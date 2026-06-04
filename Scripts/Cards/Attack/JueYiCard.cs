using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Power;
using STS2RitsuLib.Cards.DynamicVars;

namespace YunoMod.Scripts.Cards.Attack;

public class JueYiCard : YunoBaseCard
{


    private const string _LoveCount = "LoveCount";

    public JueYiCard() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {

    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new DamageVar(1,ValueProp.Move),
        new ComputedDynamicVar(_LoveCount, 0m, card =>
        {
            if (card == null || !card.IsMutable || card.Owner == null) return 0m;
            var creature = card.Owner.Creature;
            return (int)(creature.HasPower<LovePower>() ?creature.GetPowerAmount<LovePower>() : 0);
        }),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState!)
            .WithHitCount(Owner.Creature.HasPower<LovePower>() ? Owner.Creature.GetPowerAmount<LovePower>() : 0)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
