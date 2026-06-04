using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class YiYaHuanYaPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {

        if (target != Owner) return;

        if (dealer == Owner && cardSource != null)
        {
            await PowerCmd.Apply<FuChouPower>(Owner.CombatState!.HittableEnemies, Amount, Owner, null);
        }
        else if (dealer != null && dealer.IsMonster && result.UnblockedDamage > 0)
        {
            await PowerCmd.Apply<FuChouPower>(dealer, Amount, Owner, null);
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target == null) return 1m;
        if (dealer != Owner) return 1m;
        if (!target.HasPower<FuChouPower>()) return 1m;
        return 1.5m;
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult results,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner) return;
        var fuChou = target.GetPower<FuChouPower>();
        if (fuChou == null) return;
        await PowerCmd.Decrement(fuChou);
    }
}