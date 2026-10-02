using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Power.PowerCard;

// 珠泪·劈弦：打出「珠泪」卡时，从抽牌堆顶将3张牌送入弃牌堆
[RegisterPower]
public class ZhuLeiPiXianPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!ZhuLeiFilter.IsCard(cardPlay.Card)) return Task.CompletedTask;
        // 只响应自己打出的珠泪卡（与珠泪·哀唱的守卫口径一致）
        if (cardPlay.Card.Owner != Owner.Player) return Task.CompletedTask;
        if (CombatState == null) return Task.CompletedTask;

        Flash();
        return ToolCmd.DuiMu(choiceContext, Owner.Player!, 3);
    }
}
