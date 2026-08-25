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
using MegaCrit.Sts2.Core.ValueProps;

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

    public WoChuiCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");


        if (cardPlay.Target!.Block > 0)
        {
            await CreatureCmd.LoseBlock(choiceContext, cardPlay.Target!, cardPlay.Target!.Block, this.Owner.Creature);
        }
        foreach (var power in cardPlay.Target!.Powers)
        {
            if (power.Type == MegaCrit.Sts2.Core.Entities.Powers.PowerType.Buff)
                await PowerCmd.Remove(power);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
