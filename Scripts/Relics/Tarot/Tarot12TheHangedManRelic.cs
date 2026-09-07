using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位12-倒吊人：失去生命值时抽1张牌
// 逆位：因敌人的攻击失去生命值时，该敌人恢复对应数值的生命值
public class Tarot12TheHangedManRelic : TarotRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override bool SupportsReversed => true;

    // 正位（钩子口径与正位05-教皇一致，delta<0 即失去生命）
    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (IsReversed) return;
        if (creature != Owner.Creature || delta >= 0) return;
        if (!CombatManager.Instance.IsInProgress) return;

        Flash();
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, Owner);
    }

    // 逆位：被敌人的攻击打掉多少血，攻击者就恢复多少生命
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!IsReversed) return;
        if (target != Owner.Creature) return;
        if (result.UnblockedDamage <= 0) return;
        if (dealer == null || !dealer.IsMonster) return;

        Flash();
        await CreatureCmd.Heal(dealer, result.UnblockedDamage);
    }
}
