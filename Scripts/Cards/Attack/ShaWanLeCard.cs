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
        new DamageVar(10m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    ];

    public ShaWanLeCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Axe];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Axe),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await ToolCmd.AxeAttackAllEnemy(choiceContext, this, DynamicVars.Damage.BaseValue);


        // 对所有敌人造成伤害
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            // 施加基础易伤层数
            await PowerCmd.Apply<VulnerablePower>(enemy, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);

            // 获取当前易伤层数并翻倍（叠加现有层数）
            int currentVuln = enemy.GetPowerAmount<VulnerablePower>();
            if (currentVuln > 0)
            {
                await PowerCmd.Apply<VulnerablePower>(enemy, currentVuln, Owner.Creature, this);
            }
        }

        await ToolCmd.AxeStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
