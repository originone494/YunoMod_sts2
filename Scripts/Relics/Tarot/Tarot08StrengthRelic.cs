using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class Tarot08StrengthRelic : TarotRelicBase
{
    private const decimal _hpThreshold = 10m;

    private int _counter;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    // 遗物计数器（跨战斗累计，随存档保存；原版香炉 JossPaper 同款模式）
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override bool ShowCounter => true;
    public override int DisplayAmount => Counter;

    // 战斗开始：获得等同于计数的力量加护（逆位关闭）
    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (IsReversed) return;
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;
        if (Counter <= 0) return;

        Flash();
        await PowerCmd.Apply<LiLiangJiaHuPower>(choiceContext, Owner.Creature, Counter, Owner.Creature, null);
    }

    // 回合结束时：血量低于10的敌人获得宽恕（逆位关闭）
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (IsReversed) return;
        if (side != Owner.Creature.Side) return;

        var targets = Owner.Creature.CombatState?.HittableEnemies
            .Where(e => e.IsAlive && e.CurrentHp <= _hpThreshold && !e.HasPower<KuanShuPower>())
            .ToList();
        if (targets == null || targets.Count == 0) return;

        Flash();
        foreach (var enemy in targets)
        {
            await PowerCmd.Apply<KuanShuPower>(choiceContext, enemy, 1, Owner.Creature, null);
        }
    }

    // 带着宽恕死亡的敌人使计数+1（死亡钩子在能力清除前触发，可可靠判定；逆位关闭）
    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (IsReversed) return Task.CompletedTask;
        if (creature.Monster != null && creature.HasPower<KuanShuPower>())
        {
            Counter++;
        }
        return Task.CompletedTask;
    }
}
