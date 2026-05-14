using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Skill;

public class JiaoHuanCard : AbstractTemplateBaseCard
{
    

    public JiaoHuanCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>[CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取力量(StrengthPower)和敏捷(DexterityPower)的层数
        int strengthAmount = Owner.Creature.GetPowerAmount<StrengthPower>();
        int dexterityAmount = Owner.Creature.GetPowerAmount<DexterityPower>();

        // 若两者相同，则两者翻倍
        if (strengthAmount == dexterityAmount)
        {
            strengthAmount *= 2;
            dexterityAmount *= 2;
        }

        // 移除原有的力量和敏捷能力
        await PowerCmd.Remove<StrengthPower>(Owner.Creature);
        await PowerCmd.Remove<DexterityPower>(Owner.Creature);

        // 交换力量和敏捷（旧力量的位置放敏捷，旧敏捷的位置放力量）
        if (dexterityAmount > 0)
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, dexterityAmount, Owner.Creature, this);
        }
        if (strengthAmount > 0)
        {
            await PowerCmd.Apply<DexterityPower>(Owner.Creature, strengthAmount, Owner.Creature, this);
        }

    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
