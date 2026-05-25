using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class YuZhiMengPower : YunoBasePower, IOnForesee
{
    private int _foreseeDrawsPending;
    private readonly HashSet<CardModel> _freeCardsThisTurn = [];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Task OnForesee(PlayerChoiceContext ctx, Player player, int amount, int discardedAmount)
    {
        if (player.Creature != Owner) return Task.CompletedTask;
        _foreseeDrawsPending++;
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (_foreseeDrawsPending <= 0) return Task.CompletedTask;
        _freeCardsThisTurn.Add(card);
        _foreseeDrawsPending--;
        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (_freeCardsThisTurn.Contains(card))
        {
            modifiedCost = 0;
            return true;
        }
        modifiedCost = originalCost;
        return false;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        _freeCardsThisTurn.Clear();
        _foreseeDrawsPending = 0;
        return Task.CompletedTask;
    }
}
