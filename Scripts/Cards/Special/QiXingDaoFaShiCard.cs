using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「七星道魔术师」：先古攻击卡，将抽牌堆顶1张送入弃牌堆，
// 伤害 = 21 + 3 × 弃牌堆中不同卡名的数量（用 CalculatedDamageVar 动态显示变化伤害）
public class QiXingDaoFaShiCard : YunoSpecialBaseCard
{
    public QiXingDaoFaShiCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8,ValueProp.Move),
        // 计算式：基础21 + 额外3 × 弃牌堆不同卡名数，卡面上用 {CalculatedDamage:diff()} 实时显示总伤害
        new CalculationBaseVar(4m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Unpowered).WithMultiplier(static (card, _) =>
        {
            if (card?.Owner == null) return 0m;
            if(PileType.Hand.GetPile(card.Owner).Cards.Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.None) >= 2).ToList().Count()<=0) return 0m;
            return PileType.Discard.GetPile(card.Owner).Cards.Select(c => c.Title).Distinct().Count();
        }),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await ToolCmd.DuiMu(choiceContext, Owner, 1);

        var handCards = PileType.Hand.GetPile(Owner).Cards.Where(c => c.EnergyCost.GetWithModifiers(CostModifiers.None) >= 2).ToList();

        int down = DynamicVars.CalculationBase.IntValue;

        if (handCards.Count() > 0)
            down = down + PileType.Discard.GetPile(Owner).Cards.Select(c => c.Title).Distinct().Count();


        await PowerCmd.Apply<QiXingDaoFaShiTempDownPower>(choiceContext, cardPlay.Target!, down, Owner.Creature, this);

    }
}
