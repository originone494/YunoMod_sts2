using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class Tarot18TheMoonRelic : TarotRelicBase
{
    private const decimal _goldOnCombatStart = 20m;
    private const decimal _goldLossPerHit = 5m;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    // 战斗开始获得20金币（逆位关闭）
    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (IsReversed) return;
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;

        Flash();
        await PlayerCmd.GainGold(_goldOnCombatStart, Owner);
    }

    // 因敌人攻击而失去生命值时，失去5金币（按实际掉血结算，格挡挡住的不算）
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (result.UnblockedDamage <= 0) return;              // 没掉血（被格挡挡住）不算
        if (dealer == null || !dealer.IsMonster) return;      // 只响应敌人攻击（自伤/tick的dealer为null或自己）

        Flash();
        await PlayerCmd.LoseGold(_goldLossPerHit, Owner, GoldLossType.Stolen);
    }
}
