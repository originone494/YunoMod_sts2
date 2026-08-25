using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Attack;

public class WoHuiHaoHaoZuoDeCard : YunoBaseCard
{
    private const string _baseChanceKey = "BaseChance";
    private const string _critChanceKey = "CritChance";

    // 连续未触发次数：每未触发一次，暴击概率翻倍；触发后归零
    private int _missStreak;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, (ValueProp)0),            // 基础伤害 12（升级后 16）
        new DynamicVar(_baseChanceKey, 2m),          // 基础暴击概率 2%（升级后 3%）
        // 当前暴击概率：RitsuLib Computed 动态变量，显示值 = 委托实时计算（教程 19 - 计算动态变量）
        ModCardVars.Computed(_critChanceKey, 2m, (card, _) =>
        {
            if (card is not WoHuiHaoHaoZuoDeCard yuno) return 0m;
            decimal baseChance = yuno.DynamicVars[_baseChanceKey].BaseValue;
            return Math.Min(baseChance * (decimal)Math.Pow(2, yuno._missStreak), 100m);
        }),
    ];

    public WoHuiHaoHaoZuoDeCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        decimal baseDamage = DynamicVars.Damage.BaseValue;

        // 与卡面显示同源的当前概率（%），读取计算变量
        decimal currentChance = DynamicVars.EvaluateValueOrDefault(_critChanceKey, target: cardPlay.Target) / 100m;

        bool crit = Random.Shared.NextDouble() < (double)currentChance;

        // 触发时改为造成基础伤害的 2.5 次方点伤害
        decimal damageToDeal = crit
            ? (decimal)Math.Pow((double)baseDamage, 2.5)
            : baseDamage;

        await DamageCmd.Attack(damageToDeal)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);

        _missStreak = crit ? 0 : _missStreak + 1;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);            // 12 → 16
        DynamicVars[_baseChanceKey].UpgradeValueBy(1m);   // 2% → 3%
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _missStreak = 0;
        return Task.CompletedTask;
    }
}
