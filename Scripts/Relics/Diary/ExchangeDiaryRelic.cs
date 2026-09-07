using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace YunoMod.Scripts.Relics;

public class ExchangeDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;

        Flash();
        int strengthAmount = 0;
        if (Owner.Creature.HasPower<StrengthPower>())
            strengthAmount = Owner.Creature.GetPowerAmount<StrengthPower>();

        int dexterityAmount = 0;
        if (Owner.Creature.HasPower<DexterityPower>())
            dexterityAmount = Owner.Creature.GetPowerAmount<DexterityPower>();


        if (strengthAmount == dexterityAmount)
        {
            strengthAmount *= 2;
            dexterityAmount *= 2;
        }


        await PowerCmd.Remove<StrengthPower>(Owner.Creature);
        await PowerCmd.Remove<DexterityPower>(Owner.Creature);

        if (dexterityAmount > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, dexterityAmount, Owner.Creature, null);
        }
        if (strengthAmount > 0)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, strengthAmount, Owner.Creature, null);
        }

        await PowerCmd.Apply<JiaoHuanPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
