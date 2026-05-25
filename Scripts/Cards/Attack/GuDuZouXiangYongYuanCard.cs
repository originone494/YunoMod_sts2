using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Cards.Attack;

public class GuDuZouXiangYongYuanCard : YunoBaseCard, IOnForesee
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(1m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => card.CombatState?.Enemies.Where((Creature c) => c.IsAlive).Sum((Creature c) => c.GetPowerAmount<VulnerablePower>() +c.GetPowerAmount<WeakPower>() ) ?? 0)
    };

    public GuDuZouXiangYongYuanCard() : base(7, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Foresee),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 计算所有敌人的虚弱和易伤层数之和
        int totalDebuffCount = 0;

        foreach (var enemy in CombatState!.HittableEnemies)
        {
            if (enemy.HasPower<VulnerablePower>())
            {
                totalDebuffCount += enemy.GetPowerAmount<VulnerablePower>();
            }
            if (enemy.HasPower<WeakPower>())
            {
                totalDebuffCount += enemy.GetPowerAmount<WeakPower>();
            }
        }
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitCount(totalDebuffCount)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);
    }
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-2);
    }

    public async Task OnForesee(PlayerChoiceContext ctx, Player player, int amount, int discardedAmount)
    {
        EnergyCost.AddThisCombat(-2);
        await Task.CompletedTask;
    }
}
