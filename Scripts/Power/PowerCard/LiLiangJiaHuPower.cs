using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Power;

// 力量加护：打出牌时消耗1层，获得1点能量并抽1张牌
[RegisterPower]
public class LiLiangJiaHuPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsAutoPlay) return;                  // 自动打出的牌不消耗层数
        if (cardPlay.Card?.Owner != Owner.Player) return;
        if (Amount <= 0) return;

        Flash();
        await PowerCmd.Decrement(this);
        await PlayerCmd.GainEnergy(1, Owner.Player!);
        await CardPileCmd.Draw(choiceContext, 1, Owner.Player!);
    }
}
