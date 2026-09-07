using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Skill;

// 威胁：打出消耗堆中所有匕首牌
public class WeiXieCard : YunoBaseCard
{
    public WeiXieCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {

    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Dagger];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 消耗堆中所有匕首牌逐张自动打出（无需目标选择，由引擎为需要目标的牌自动选取）
        var daggers = PileType.Exhaust.GetPile(Owner).Cards
            .Where(c => c.Keywords.Contains(YunoKeywords.Dagger))
            .ToList();

        foreach (var dagger in daggers)
        {
            await CardCmd.AutoPlay(choiceContext, dagger, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
