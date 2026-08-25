using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Ui.Toast;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Relics;

public class RadarDiaryRelic : YunoBaseRelic, IModRightClickableRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    private bool _isRighted = false;


    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await ToolCmd.ForeseeAndDraw(choiceContext, Owner);
    }

    // 右键遗物：使玩家回到1层
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (!_isRighted)
        {
            _isRighted = true;
            await RunManager.Instance.EnterAct(0);

        }

    }
}
