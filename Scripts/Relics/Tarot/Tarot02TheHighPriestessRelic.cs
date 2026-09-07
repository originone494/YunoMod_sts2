using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class Tarot02TheHighPriestessRelic : TarotRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override bool SupportsReversed => true;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;

        if (IsReversed)
        {
            // 逆位：所有敌人获得1层人工制品
            var enemies = base.Owner.Creature.CombatState?.HittableEnemies;
            if (enemies == null || enemies.Count == 0) return;

            Flash();
            foreach (var enemy in enemies)
            {
                await PowerCmd.Apply<ArtifactPower>(choiceContext, enemy, 1, base.Owner.Creature, null);
            }
        }
        else
        {
            // 正位：自己获得1层人工制品
            Flash();
            await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }
}
