using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

// 威胁：能力牌。打出后每个回合开始时，将带「虚无」的「刺伤」加入手牌（升级后数量 +1）。
public class WeiXieCard : YunoBaseCard
{
    private const string _stabCountKey = "StabCount";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_stabCountKey, 1m),   // 每回合加入的刺伤数量（{StabCount:diff()}）
    ];

    public WeiXieCard() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Dagger];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<CiShangCard>(),
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<WeiXieThreatPower>(choiceContext, Owner.Creature, DynamicVars[_stabCountKey].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_stabCountKey].UpgradeValueBy(1);
    }
}
