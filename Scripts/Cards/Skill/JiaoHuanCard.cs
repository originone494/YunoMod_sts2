using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Skill;

public class JiaoHuanCard : YunoBaseCard
{


    public JiaoHuanCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {


        int strengthAmount = 0;
        if (Owner.Creature.HasPower<StrengthPower>())
            strengthAmount = Owner.Creature.GetPowerAmount<StrengthPower>();

        int dexterityAmount = 0;
        if (Owner.Creature.HasPower<DexterityPower>())
            dexterityAmount = Owner.Creature.GetPowerAmount<DexterityPower>();


        if (strengthAmount == dexterityAmount)
        {
            strengthAmount *= 2;
            dexterityAmount *= 2;
        }


        await PowerCmd.Remove<StrengthPower>(Owner.Creature);
        await PowerCmd.Remove<DexterityPower>(Owner.Creature);

        if (dexterityAmount > 0)
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, dexterityAmount, Owner.Creature, this);
        }
        if (strengthAmount > 0)
        {
            await PowerCmd.Apply<DexterityPower>(Owner.Creature, strengthAmount, Owner.Creature, this);
        }

        await PowerCmd.Apply<JiaoHuanPower>(Owner.Creature, 1, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
