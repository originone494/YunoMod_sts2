using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;

namespace YunoMod.Scripts.Relics;

public class MurumuruCrownRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeCombatStart()
    {
        Flash();
        var card = Owner.Creature.CombatState!.CreateCard<QiuTiCard>(Owner);
        var addResult = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: true);
        CardCmd.PreviewCardPileAdd(addResult, 2f);

    }
}
