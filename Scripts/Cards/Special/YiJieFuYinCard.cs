using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

// 异解·福音：获得被动（每回合免费打出1张异解怪兽；回合开始时灌注消耗堆并可打出1张异解怪兽）
public class YiJieFuYinCard : YunoSpecialBaseCard
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.YiJie),
        HoverTipFactory.FromKeyword(YunoKeywords.YiJieMoXian),
    ];
    public YiJieFuYinCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [YunoTags.YiJie, YunoTags.YiJieMoXian];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得被动能力
        await PowerCmd.Apply<YiJieFuYinPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
