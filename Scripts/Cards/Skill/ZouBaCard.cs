using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class ZouBaCard : YunoBaseCard
{

    private const string _LoveCount = "LoveCount";
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_LoveCount,3)
    };

    public ZouBaCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
    ];

    

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await ToolCmd.GainLovePower(choiceContext, Owner, this, DynamicVars[_LoveCount].IntValue);

        int amount = Owner.Creature.GetPowerAmount<LovePower>();

        await CreatureCmd.GainBlock(Owner.Creature, new BlockVar(amount / 2 + 1, ValueProp.Unpowered), cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_LoveCount].UpgradeValueBy(1m);
    }
}
