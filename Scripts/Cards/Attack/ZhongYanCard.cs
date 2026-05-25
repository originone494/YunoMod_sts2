using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Attack;

public class ZhongYanCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(60m, ValueProp.Move),
    };

    public ZhongYanCard() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        
    }

    protected override bool IsPlayable => Owner.Creature.GetPowerAmount<LovePower>() >= 8;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                        .FromCard(this)
                        .TargetingAllOpponents(Owner.Creature.CombatState!)
                        .WithHitFx("vfx/vfx_dramatic_stab")
                        .Execute(choiceContext);
                        
        await ToolCmd.GainLovePower(choiceContext,Owner,this,2);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(15m);
    }
}
