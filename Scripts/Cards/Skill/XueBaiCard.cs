using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Skill;

public class XueBaiCard : YunoBaseCard
{
    private const string _healPercentKey = "HealPercent";

    // ShouldDie 是同步方法，先标记防止死亡；AfterDeath 里再执行消耗与回血
    private bool _revivePending;

    public XueBaiCard() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_healPercentKey, 40m),   // 复活后恢复的最大生命值百分比（升级后 50%）
    ];

    // 这张卡不能打出
    protected override bool IsPlayable => false;

    // 死亡时：若这张卡还在抽牌堆/手牌/弃牌堆中，防止死亡，改为触发复活
    public override bool ShouldDie(Creature creature)
    {
        if (creature == Owner.Creature && !_revivePending && IsInUsablePile)
        {
            _revivePending = true;
            return false;
        }
        return base.ShouldDie(creature);
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature == Owner.Creature && wasRemovalPrevented && _revivePending)
        {
            _revivePending = false;

            // 先回血：IsAlive 由 HP 计算（IsDead = CurrentHp <= 0），
            // 必须先复活再消耗，否则 CardPileCmd.Add 会因为生物死亡而静默跳过消耗
            decimal heal = Owner.Creature.MaxHp * DynamicVars[_healPercentKey].BaseValue / 100m;
            await CreatureCmd.Heal(Owner.Creature, heal);

            // 再消耗抽牌堆、手牌、弃牌堆中的这张卡
            if (IsInUsablePile)
            {
                await CardCmd.Exhaust(choiceContext, this);
            }
        }
    }

    // 这张卡是否还在可被消耗的牌堆中
    private bool IsInUsablePile => Pile?.Type is PileType.Draw or PileType.Hand or PileType.Discard;

    protected override void OnUpgrade()
    {
        DynamicVars[_healPercentKey].UpgradeValueBy(10m);   // 40% → 50%
    }

    // 无法打出，OnPlay 不会被调用（模板要求实现）
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}
