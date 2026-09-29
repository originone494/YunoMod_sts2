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
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class FeedDiaryRelic : YunoBaseRelic
{

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    private const string _increaseKey = "IncreaseCount";

    private bool _isFirstPlay = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_increaseKey,2)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_isFirstPlay) return;

        // 只强化自己打出的牌（多人下队友的牌不触发、不消耗次数）
        if (cardPlay.Card.Owner != Owner) return;

        if (cardPlay.Card.Type == CardType.Attack && cardPlay.Card.DynamicVars.ContainsKey("Damage"))
        {
            cardPlay.Card.DynamicVars.Damage.BaseValue += DynamicVars[_increaseKey].BaseValue;
            Flash();
            _isFirstPlay = false;
        }
        else if (cardPlay.Card.Type == CardType.Skill
                 && cardPlay.Card.DynamicVars.ContainsKey("Block")
                 && cardPlay.Card.DynamicVars["Block"] is BlockVar skillBlockVar)
        {
            // 必须用 is BlockVar 判定：ContainsKey("Block") 只说明"有这个名字的变量"，
            // 而 DynamicVarSet.Block 是硬转换 (BlockVar)_vars["Block"]，
            // 对「羊衍生物」这类用普通 DynamicVar 占了 "Block" 名字的卡会直接抛 InvalidCastException。
            skillBlockVar.BaseValue += DynamicVars[_increaseKey].BaseValue;
            Flash();
            _isFirstPlay = false;
        }
        await Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side)
        {
            _isFirstPlay = true;
        }
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
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
