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

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class NiaoNiaoLianGeJiDeYiShiHuaiPower : YunoBasePower, IOnLingHuo
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
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
