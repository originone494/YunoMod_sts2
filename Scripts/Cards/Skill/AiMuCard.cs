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
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class AiMuCard : YunoBaseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
    };

    public AiMuCard() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];

    

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        int num = ResolveEnergyXValue();


        //计算（手牌数量的两倍+弃牌堆数量）/消耗能量数量的余数，若结果为0，则获得X点能量，抽X张卡牌，获得X层爱意。

        if ((Owner.PlayerCombatState!.Hand.Cards.Count * 2 + Owner.PlayerCombatState.DiscardPile.Cards.Count) % num == 0)
        {
            if (IsUpgraded) num *= 2;

            await CardPileCmd.Draw(choiceContext, num, Owner);

            await PowerCmd.Apply<LovePower>(Owner.Creature, num, Owner.Creature, this);

            await PlayerCmd.GainEnergy(num, Owner);
        }

    }

    protected override void OnUpgrade()
    {
    }
}
