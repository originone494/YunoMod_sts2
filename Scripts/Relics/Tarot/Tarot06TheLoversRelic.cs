using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class Tarot06TheLoversRelic : TarotRelicBase
{
    // 本场战斗中上一张打出的牌的类型（自动打出的牌不参与，不更新）
    private CardType? _lastCardType;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override bool SupportsReversed => true;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsReversed) return;                             // 逆位：正位的效果暂不生效
        if (cardPlay.IsAutoPlay) return;                    // 自动打出的牌不算"打出"
        if (cardPlay.Card?.Owner != Owner) return;
        if (Owner.Creature.CombatState == null) return;

        CardType current = cardPlay.Card.Type;
        bool isAlternation = (_lastCardType == CardType.Attack && current == CardType.Skill)
                          || (_lastCardType == CardType.Skill && current == CardType.Attack);

        _lastCardType = current;
        if (!isAlternation) return;

        Flash();
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _lastCardType = null;
        return base.AfterCombatEnd(room);
    }
}
