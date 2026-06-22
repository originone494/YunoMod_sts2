using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class ZiShangCard : YunoBaseCard
{

    private const string _zhiCanPower = "ZhiCanPower";
    public ZiShangCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {

    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(3),
        new DynamicVar(_zhiCanPower, 2)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ZhiCanPower>(),
    ];

    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await PowerCmd.Apply<ZhiCanPower>(choiceContext, Owner.Creature, DynamicVars[_zhiCanPower].BaseValue, Owner.Creature, this);
        // 获得能量
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);


    }

    protected override void OnUpgrade()
    {
        DynamicVars[_zhiCanPower].UpgradeValueBy(-1);
    }
}
