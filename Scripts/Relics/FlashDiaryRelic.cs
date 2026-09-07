using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

public class FlashDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task BeforeCombatStart()
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile.Cards.Count == 0) return Task.CompletedTask;

        var card = Owner.RunState.Rng.Niche.NextItem(drawPile.Cards)!;
        card.BaseReplayCount += 1;
        Flash();

        return Task.CompletedTask;
    }
}
