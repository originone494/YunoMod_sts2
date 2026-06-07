using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class UselessClockRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    private bool _triggeredThisTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
            _triggeredThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_triggeredThisTurn) return;

        var playedCard = cardPlay.Card;
        if (playedCard == null) return;
        if (playedCard.Owner != Owner) return;

        // 判断打出的是打击还是防御
        CardTag? targetTag = null;
        if (playedCard.Tags.Contains(CardTag.Strike))
            targetTag = CardTag.Defend;
        else if (playedCard.Tags.Contains(CardTag.Defend))
            targetTag = CardTag.Strike;

        if (targetTag == null) return;

        // 从抽牌堆找对应卡牌
        var targetCard = PileType.Draw.GetPile(Owner).Cards
            .FirstOrDefault(c => c.Tags.Contains(targetTag.Value));
        if (targetCard == null) return;

        _triggeredThisTurn = true;
        Flash();
        await CardCmd.AutoPlay(choiceContext, targetCard, null);
    }
}
