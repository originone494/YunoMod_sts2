using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;


public class JiaoHuanPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;

        int strengthAmount = 0;
        if (Owner.HasPower<StrengthPower>())
            strengthAmount = Owner.GetPowerAmount<StrengthPower>();

        int dexterityAmount = 0;
        if (Owner.HasPower<DexterityPower>())
            dexterityAmount = Owner.GetPowerAmount<DexterityPower>();


        await PowerCmd.Remove<StrengthPower>(Owner);
        await PowerCmd.Remove<DexterityPower>(Owner);

        if (dexterityAmount > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, dexterityAmount, Owner, null);
        }
        if (strengthAmount > 0)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, strengthAmount, Owner, null);
        }

        await PowerCmd.Remove(this);
    }

}