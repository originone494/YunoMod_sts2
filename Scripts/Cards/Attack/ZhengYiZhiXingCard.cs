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
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Attack;

public class ZhengYiZhiXingCard : YunoBaseCard
{


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(21m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unblockable|ValueProp.Unpowered).WithMultiplier((CardModel card, Creature? _) => ( card.Owner != null) ? (int)(card.Owner.Creature.MaxHp * 0.1m) : 0),
    };

    public ZhengYiZhiXingCard() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<BaoZaPower>(),
    ];

    

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(Owner.Creature.CombatState!)
                .WithHitFx("vfx/vfx_dramatic_stab")
                .Execute(choiceContext);

        int damage = (int)(Owner.Creature.MaxHp * 0.1m);

        await CreatureCmd.Damage(choiceContext, Owner.Creature, damage, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this);

        await PowerCmd.Apply<BaoZaPower>(Owner.Creature, damage, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }
}
