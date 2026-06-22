using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class WoChuiCard : YunoBaseCard
{

    private const string _strengthKey = "DownStrength";

    private const string _strengthAloneKey = "DownAloneStrength";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];



    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_strengthKey, 6m),
        new DynamicVar(_strengthAloneKey, 12m),

    ];

    public WoChuiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int strengthLoss = DynamicVars[_strengthKey].IntValue;

        // 如果只有一个敌人，失去更多力量
        if (CombatState!.HittableEnemies.Count == 1)
        {
            strengthLoss = DynamicVars[_strengthAloneKey].IntValue;
        }

        await PowerCmd.Apply<PiercingWailPower>(choiceContext, CombatState!.HittableEnemies, strengthLoss, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_strengthKey].UpgradeValueBy(2m);
        DynamicVars[_strengthAloneKey].UpgradeValueBy(4m);

    }
}
