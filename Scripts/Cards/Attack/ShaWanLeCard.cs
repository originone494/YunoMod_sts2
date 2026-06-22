using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;

using STS2RitsuLib.Keywords;
namespace YunoMod.Scripts.Cards.Attack;

public class ShaWanLeCard : YunoBaseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    ];

    public ShaWanLeCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, YunoKeywords.Axe];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(YunoKeywords.Axe),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];




    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await ToolCmd.AxeAttackAllEnemy(choiceContext, this, DynamicVars.Damage.BaseValue);


        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);

            int currentVuln = enemy.GetPowerAmount<VulnerablePower>();
            if (currentVuln > 0)
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, currentVuln, Owner.Creature, this);
            }
        }

        await ToolCmd.AxeStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}
