using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class ClairvoyanceRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;



    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {


        if (side == base.Owner.Creature.Side)
        {
            if (combatState.RoundNumber <= 1)
            {
                Flash();
                await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);

                var cards = PileType.Draw.GetPile(Owner).Cards.ToList();

                // 扫牌必须用不可被打断的选择上下文：爪音等卡的「灵活」会弹检索选择界面，
                // 若沿用引擎传入的钩子上下文，选择一发起就会把本钩子「暂停」并提前返回，
                // 流程会继续抽起手牌；而快照里的卡与抽出的手牌是同一批对象，
                // 循环恢复后会把这些卡（此时已在手牌）逐张送入弃牌堆。
                var sweepContext = new BlockingPlayerChoiceContext();

                foreach (var cardModel in cards)
                {
                    await CardCmd.Discard(sweepContext, cardModel);

                    CardCmd.Preview(cardModel);
                }
            }
        }
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }
}
