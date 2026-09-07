using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Power.PowerCard;

// 王之馆：回合开始时「检索」1张「荷鲁斯怪兽」卡送入弃牌堆，之后将弃牌堆的荷鲁斯怪兽全部加入手牌；
// 「荷鲁斯怪兽」卡进入弃牌堆时（检索送入/主动弃牌/打出后落弃/回合末冲牌等所有路径），对随机敌人造成8点伤害。
[RegisterPower]
public class WangZhiGuanPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        if (CombatState == null) return;

        // ① 「检索」1张「荷鲁斯怪兽」卡送入弃牌堆（poolFilter=null 即不限卡池；玩家可放弃不选）
        Flash();
        await ToolCmd.RetrieverCard(
            choiceContext,
            player,
            c => c.Tags.Contains(YunoTags.HeLuSiGuaiShou),
            null,
            1,
            isDiscard: true);

        // ② 将弃牌堆的「荷鲁斯怪兽」卡全部加入手牌（含①送入的那张，经弃牌堆"过一手"后回手）
        foreach (CardModel card in PileType.Discard.GetPile(player).Cards
            .Where(c => c.Tags.Contains(YunoTags.HeLuSiGuaiShou))
            .ToList())
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        // 只处理"进入弃牌堆"且带荷鲁斯怪兽标签的卡（打出、丢弃等所有路径都会走到这里）
        if (card.Pile?.Type != PileType.Discard) return Task.CompletedTask;
        if (!card.Tags.Contains(YunoTags.HeLuSiGuaiShou)) return Task.CompletedTask;
        if (CombatState == null) return Task.CompletedTask;

        Flash();

        Creature? enemy = Owner.Player!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (enemy == null) return Task.CompletedTask;

        return CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), enemy, 8m, ValueProp.Move, Owner, null, null);
    }
}
