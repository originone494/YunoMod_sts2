using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class Tarot05TheHierophantRelic : TarotRelicBase
{
    private const decimal _maxHpGain = 20m;
    private const decimal _maxHpPerHeal = 1m;
    private const decimal _maxHpPerLoss = 1m;

    // 防重入：GainMaxHp 内部会 Heal 等量生命、LoseMaxHp 也可能引发 HP 变动，不加标记会连锁触发
    private bool _changingMaxHp;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override bool SupportsReversed => true;

    public override async Task AfterObtained()
    {
        Flash();
        await CreatureCmd.GainMaxHp(Owner.Creature, _maxHpGain);
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (_changingMaxHp) return;
        if (creature != Owner.Creature) return;
        if (!CombatManager.Instance.IsInProgress) return;

        if (IsReversed)
        {
            // 逆位：失去生命值时，失去1点最大生命值
            if (delta >= 0) return;

            _changingMaxHp = true;
            try
            {
                Flash();
                await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature, _maxHpPerLoss, isFromCard: false);
            }
            finally
            {
                _changingMaxHp = false;
            }
            return;
        }

        // 正位：获得生命值时，获得1点最大生命值
        if (delta <= 0) return;

        _changingMaxHp = true;
        try
        {
            Flash();
            await CreatureCmd.GainMaxHp(Owner.Creature, _maxHpPerHeal);
        }
        finally
        {
            _changingMaxHp = false;
        }
    }
}
