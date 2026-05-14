using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Skill;

public class JueJingCard : AbstractTemplateBaseCard
{

    private const string _drawKey = "DrawCount";

    private const int _cost = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_drawKey, 2m),
        new EnergyVar(_cost)
    ];


    public JueJingCard() : base(_cost, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.LingHuo];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int drawCount = (int)DynamicVars[_drawKey].BaseValue;
        await CardPileCmd.Draw(choiceContext, drawCount, Owner);

        if (Owner.Creature.CurrentHp <= Owner.Creature.MaxHp * 0.4m)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_drawKey].UpgradeValueBy(1m);
    }
}
