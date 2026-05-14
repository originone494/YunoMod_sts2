using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Attack;

public class CongZuoBianErLaiCard : AbstractTemplateBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
    ];

    public CongZuoBianErLaiCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);

        await ToolCmd.ForeseeAndDraw(choiceContext, Owner);

        // 将自身的易伤和虚弱转移给目标
        decimal selfVulnerable = 0;
        decimal selfWeak = 0;

        VulnerablePower selfVulnPower = Owner.Creature.GetPower<VulnerablePower>()!;
        WeakPower selfWeakPower = Owner.Creature.GetPower<WeakPower>()!;

        if (selfVulnPower != null)
        {
            selfVulnerable = selfVulnPower.Amount;
            await PowerCmd.Remove(selfVulnPower);
        }

        if (selfWeakPower != null)
        {
            selfWeak = selfWeakPower.Amount;
            await PowerCmd.Remove(selfWeakPower);
        }

        if (selfVulnerable > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, selfVulnerable, Owner.Creature, this);
        }

        if (selfWeak > 0)
        {
            await PowerCmd.Apply<WeakPower>(cardPlay.Target, selfWeak, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
