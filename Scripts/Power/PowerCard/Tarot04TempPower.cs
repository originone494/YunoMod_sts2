using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Relics;

namespace YunoMod.Scripts.Power;

// 正位04-皇帝的临时力量（原版 TemporaryStrengthPower 机制，回合结束自动消失）
[RegisterPower]
public class Tarot04TempStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<Tarot04TheEmperorRelic>();
}

// 皇帝逆位的临时力量（负面）：目标失去力量，其回合结束自动恢复
[RegisterPower]
public class Tarot04TempStrengthDownPower : TemporaryStrengthPower
{
    protected override bool IsPositive => false;

    public override AbstractModel OriginModel => ModelDb.Relic<Tarot04TheEmperorRelic>();
}

// 正位04-皇帝的临时敏捷（原版 TemporaryDexterityPower 机制，回合结束自动消失）
[RegisterPower]
public class Tarot04TempDexterityPower : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<Tarot04TheEmperorRelic>();
}
