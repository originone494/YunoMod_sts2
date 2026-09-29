using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;

namespace YunoMod.Scripts.Power;

// 「威胁」打出后挂载的能力：自己每个回合开始时，按层数将带「虚无」的「刺伤」加入手牌。
// 层数即每次加入的张数，因此重复打出会叠加。
// 参考原版 InfiniteBladesPower（无限刀刃）／CallOfTheVoidPower：用 BeforeHandDraw 摸牌前入手。
public class WeiXieThreatPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<CiShangCard>(),
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
    ];

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        // 只在自己的回合开始时结算（多人下队友回合不触发）
        if (player != Owner.Player) return;
        if (Amount <= 0) return;
        if (CombatManager.Instance.IsOverOrEnding) return;

        var stabs = new List<CardModel>(Amount);
        for (int i = 0; i < Amount; i++)
        {
            // CreateCard 会把它登记进 CombatState（不能用 RunState.CreateCard，
            // 否则卡牌不在 _allCards 中，之后洗牌时会抛 "must be added to a CombatState"）
            CardModel stab = combatState.CreateCard<CiShangCard>(player);
            // 额外的「虚无」：回合结束时若仍在手牌则被消耗
            CardCmd.ApplyKeyword(stab, CardKeyword.Ethereal);
            stabs.Add(stab);
        }

        Flash();
        await CardPileCmd.AddGeneratedCardsToCombat(stabs, PileType.Hand, Owner.Player);
    }
}
