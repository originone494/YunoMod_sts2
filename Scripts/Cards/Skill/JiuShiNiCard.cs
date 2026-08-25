using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class JiuShiNiCard : YunoBaseCard
{
    private const string _downStrengthKey = "DownStrength";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_downStrengthKey, 4m),   // 目标失去的力量（{DownStrength:diff()}）
        new PowerVar<StrengthPower>(4m),        // 自己获得的力量（{StrengthPower:diff()}）
    ];

    public JiuShiNiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 本回合内：目标失去4点力量（临时，回合结束自动恢复）
        await PowerCmd.Apply<JiuShiNiTempDownPower>(choiceContext, cardPlay.Target, DynamicVars[_downStrengthKey].BaseValue, Owner.Creature, this);

        // 本回合内：自己获得4点力量（临时，回合结束自动消失）
        await PowerCmd.Apply<JiuShiNiTempPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
