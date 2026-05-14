using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class BreedDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 检查打出的卡牌是否是能力牌
        if (cardPlay.Card.Type != CardType.Power)
        {
            return;
        }

        Flash();

        // 创建相同能力牌的副本并添加到手牌
        CardModel clone = cardPlay.Card.CreateClone();
        await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, addedByPlayer: true);
    }
}
