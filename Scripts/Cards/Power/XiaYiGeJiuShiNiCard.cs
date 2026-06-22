using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Power;

public class XiaYiGeJiuShiNiCard : YunoBaseCard
{

    private const string _PowerCount = "PowerCount";

    public XiaYiGeJiuShiNiCard() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_PowerCount,1),
        new EnergyVar(1)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ZhiCanPower>(),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<ZhiCanPower>(choiceContext, CombatState!.HittableEnemies, DynamicVars[_PowerCount].BaseValue, Owner.Creature, null);

        await PowerCmd.Apply<XiaYiGeJiuShiNiPower>(choiceContext, Owner.Creature, DynamicVars[_PowerCount].BaseValue, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[_PowerCount].UpgradeValueBy(1);
    }
}
