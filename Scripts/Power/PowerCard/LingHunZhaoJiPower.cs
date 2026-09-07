using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Power;

// 灵魂召集：打出攻击牌时，从抽牌堆顶将3张卡送去弃牌堆
[RegisterPower]
public class LingHunZhaoJiPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只有打出的卡是攻击牌时触发
        if (cardPlay.Card.Type != CardType.Attack) return Task.CompletedTask;
        if (CombatState == null) return Task.CompletedTask;

        Flash();
        return ToolCmd.DuiMu(choiceContext, Owner.Player!, 3);
    }
}
