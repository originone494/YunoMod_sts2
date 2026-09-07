using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位21-世界：每打出12张牌，获得一次额外回合（一场战斗一次）
// 逆位：打出第12张牌时，结束你的回合（一场战斗只能触发一次）
public class Tarot21TheWorldRelic : TarotRelicBase
{
    private const int CardsNeeded = 12;

    private int _cardsPlayed;
    private bool _usedThisCombat;
    private bool _endTurnUsed;   // 逆位：结束回合仅触发一次

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override bool SupportsReversed => true;

    // 计数器：显示已打出的牌数
    public override bool ShowCounter => true;
    public override int DisplayAmount => Math.Max(0, _cardsPlayed);

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsAutoPlay) return;   // 自动打出的卡不计（与佩尔之眼的口径一致）
        if (cardPlay.Card?.Owner != Owner) return;
        if (Owner.Creature.CombatState == null) return;

        if (_cardsPlayed < CardsNeeded)
        {
            _cardsPlayed++;
            InvokeDisplayAmountChanged();
            if (_cardsPlayed >= CardsNeeded) Flash();
        }

        if (!IsReversed) return;

        // 逆位：打出第12张牌时，结束你的回合（一场战斗一次）
        if (_endTurnUsed || _cardsPlayed < CardsNeeded) return;
        _endTurnUsed = true;
        PlayerCmd.EndTurn(Owner, canBackOut: false);
    }

    // 正位：结束回合时游戏会询问每个模型是否需要额外回合（参考佩尔之眼）；逆位关闭
    public override bool ShouldTakeExtraTurn(Player player)
        => !IsReversed && player == Owner && !_usedThisCombat && _cardsPlayed >= CardsNeeded;

    public override Task AfterTakingExtraTurn(Player player)
    {
        if (player != Owner) return Task.CompletedTask;
        Flash();
        _usedThisCombat = true;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _cardsPlayed = 0;
        _usedThisCombat = false;
        _endTurnUsed = false;
        return base.AfterCombatEnd(room);
    }
}
