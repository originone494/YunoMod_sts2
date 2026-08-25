using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Cards.Attack;

public class QuSiCard : YunoBaseCard, IOnStanceChange
{
    private const string _damageKey = "Damage";
    private const string _growthKey = "Growth";

    // 本场战斗内累计的伤害成长
    private int _combatGrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),  // 基础伤害（{Damage:diff()}，必须是 DamageVar 类型）
        new DynamicVar(_growthKey, 3m),      // 每次进入姿态的伤害成长（{Growth:diff()}）
        // 当前伤害显示 = 基础伤害 + 本场战斗成长
        ModCardVars.Computed("CurrentDamage", 12m, (card, _) => card is QuSiCard q ? q.CurrentDamage : 0m),
    ];

    public QuSiCard() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    // 当前伤害 = 基础伤害 + 本场战斗成长
    public decimal CurrentDamage => DynamicVars.Damage.BaseValue + _combatGrowth;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(CurrentDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // 每次进入姿态，伤害在本场战斗+3
    public async Task OnStanceChange(PlayerChoiceContext ctx, Player player, Stance oldStance, Stance newStance)
    {
        if (player != Owner) return;
        _combatGrowth += DynamicVars[_growthKey].IntValue;
        await Task.CompletedTask;
    }

    // 战斗结束：重置本场战斗成长
    public override Task AfterCombatEnd(CombatRoom room)
    {
        _combatGrowth = 0;
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars[_growthKey].UpgradeValueBy(1);
    }
}
