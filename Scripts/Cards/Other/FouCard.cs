using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Other;

// 辅助选择卡「否」：珠泪·水仙 检索时选择送入弃牌堆
public class FouCard : YunoBaseCard
{
    public FouCard() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Task.CompletedTask;
    }
}
