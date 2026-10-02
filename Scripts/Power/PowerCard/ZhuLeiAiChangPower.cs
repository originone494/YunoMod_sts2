using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

using YunoMod.Scripts.Tool;
namespace YunoMod.Scripts.Power;

// 珠泪·哀唱：每回合第一次打出「珠泪」卡时，给予所有敌人2层易伤
[RegisterPower]
public class ZhuLeiAiChangPower : YunoBasePower
{
    private const int _vulnerableAmount = 2;

    private bool _triggeredThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_triggeredThisTurn) return;
        if (cardPlay.Card == null) return;
        if (cardPlay.Card.Owner != Owner.Player) return;
        if (!ZhuLeiFilter.IsCard(cardPlay.Card)) return;
        if (CombatState == null) return;

        _triggeredThisTurn = true;

        Flash();
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            CombatState.HittableEnemies,
            _vulnerableAmount,
            Owner,
            null);
    }

    // 每回合开始时重置「第一次」标记
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return Task.CompletedTask;

        _triggeredThisTurn = false;
        return Task.CompletedTask;
    }
}
