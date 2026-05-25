using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class BadEndCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3,ValueProp.Unpowered |ValueProp.Unblockable),
        new BlockVar(1m, ValueProp.Move),
        new EnergyVar(1),
        new PowerVar<StrengthPower>(1)
    ];

    public BadEndCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LiuXuePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 3, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this);

        var drawList = await CardPileCmd.Draw(choiceContext, 10 - Owner.PlayerCombatState!.Hand.Cards.Count, Owner);

        foreach (var card in drawList)
        {
            if (card.Type == CardType.Attack)
            {
                // 攻击牌: 所有敌人获得1层流血(BleedingPower)
                foreach (Creature enemy in CombatState!.HittableEnemies)
                {
                    await PowerCmd.Apply<LiuXuePower>(enemy, 1, base.Owner.Creature, this);
                }
            }
            else if (card.Type == CardType.Skill)
            {
                // 技能牌: 获得1点格挡
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            }
            else if (card.Type == CardType.Power)
            {
                // 能力牌: 获得1费
                await PlayerCmd.GainEnergy((int)DynamicVars.Energy.BaseValue, Owner);

            }

        }

    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
