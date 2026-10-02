using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

using YunoMod.Scripts.Tool;
namespace YunoMod.Scripts.Power;

[RegisterPower]
public class NiaoNiaoLianGeJiDeYiShiHuaiPower : YunoBasePower, ILingHuoObserver
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 观察者：任意一张「珠泪怪兽」卡触发灵活时，从抽牌堆选 1 张费用不超过 1 的卡送入弃牌堆
    public async Task OnLingHuo(PlayerChoiceContext ctx, Player player, CardModel trigger)
    {
        if (player != Owner.Player) return;
        // 「珠泪怪兽」按新口径 = 含珠泪融合怪兽
        if (!ZhuLeiFilter.IsMonster(trigger)) return;

        var drawPile = PileType.Draw.GetPile(player);
        var candidates = drawPile.Cards
            .Where(card => card.EnergyCost.GetWithModifiers(CostModifiers.None) <= 1)
            .ToList();

        if (candidates.Count == 0) return;

        var selected = (await CardSelectCmd.FromSimpleGrid(
            ctx,
            candidates,
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1))).FirstOrDefault();

        if (selected != null)
        {
            await CardCmd.Discard(ctx, selected);
        }
    }
}
