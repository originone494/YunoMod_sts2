using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

public class TunShiBaiWanDeBaoShiShouCard : YunoSpecialBaseCard
{
    private const int _randomCards = 5;

    public TunShiBaiWanDeBaoShiShouCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;

        await ToolCmd.AddRandomCardsToExhaust(Owner, _randomCards);

        int exhaustCount = PileType.Exhaust.GetPile(Owner).Cards.Count;
        await DamageCmd.Attack(exhaustCount)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}
