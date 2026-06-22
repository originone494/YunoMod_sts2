using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

public class ZhanHouBaoZaPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return;
        if (result.UnblockedDamage <= 0) return;
        if (dealer == null) return;

        var ratio = 0m;

        if (dealer == Owner && cardSource != null)
        {
            ratio = 1m;
            var HealAmount = result.UnblockedDamage * ratio * Amount;
            await PowerCmd.Apply<BaoZaPower>(choiceContext, Owner, HealAmount, Owner, null);
        }
        else if (cardSource == null && !dealer!.IsPlayer && dealer!.IsMonster)
        {
            ratio = Amount * 0.25m;
            var HealAmount = result.UnblockedDamage * ratio * Amount;
            await PowerCmd.Apply<BaoZaPower>(choiceContext, Owner, HealAmount, Owner, null);
        }


    }
}