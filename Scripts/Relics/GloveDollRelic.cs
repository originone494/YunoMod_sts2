using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class GloveDollRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner.Creature) return 1m;   // 只减免自己受到的伤害（多人下不影响队友）

        if (dealer == null || !dealer.IsMonster || !dealer.HasPower<PoisonPower>())
            return 1m;

        return 0.75m;
    }
}
