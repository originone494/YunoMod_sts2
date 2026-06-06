using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

public class ShouHuCard : YunoBaseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6,ValueProp.Unpowered)
    };

    public ShouHuCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromPower<LovePower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ShouHuPower>(Owner.Creature, DynamicVars.Damage.IntValue, Owner.Creature, this);
    }


    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
