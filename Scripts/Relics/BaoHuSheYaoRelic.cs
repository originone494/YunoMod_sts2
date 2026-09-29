using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 迁移自 OriginRelicBox：保护我方蛇咬。
// 战斗开始时，将一张蛇咬加入手牌；回合开始时，手牌中每有一张蛇咬，给予全体敌人中毒。
public class BaoHuSheYaoRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("BaoHuSheYao_PoisonCount", 2m)];

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner || combatState.RoundNumber != 1) return;

        Flash();
        CardModel cardModel = Owner.Creature.CombatState!.CreateCard<Snakebite>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, Owner);
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner.Creature.CombatState!.RoundNumber == 1) return;

        List<CardModel> snakebites = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Id.Entry == "SNAKEBITE").ToList();

        foreach (var _ in snakebites)
        {
            Flash();
            await PowerCmd.Apply<PoisonPower>(
                choiceContext,
                Owner.Creature.CombatState.HittableEnemies,
                DynamicVars["BaoHuSheYao_PoisonCount"].BaseValue,
                Owner.Creature,
                null);
        }
    }
}
