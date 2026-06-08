using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Power;

public class WuYiZhongTingCard : YunoBaseCard
{

    private const string _drawKey = "Draw";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_drawKey, 1m),
    ];

    public WuYiZhongTingCard() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<WuYiZhongTingPower>(Owner.Creature, DynamicVars[_drawKey].IntValue, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[_drawKey].UpgradeValueBy(1m);
    }
}
