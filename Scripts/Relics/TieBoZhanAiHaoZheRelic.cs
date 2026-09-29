using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 迁移自 OriginRelicBox：铁波斩爱好者。
// 获得时：将一张铁波斩加入牌组，移除牌组中所有打击与防御，每移除一张，再将一张铁波斩加入牌组。
public class TieBoZhanAiHaoZheRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromCard<IronWave>()
    ];

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        await CardPileCmd.Add(Owner.RunState.CreateCard<IronWave>(Owner), PileType.Deck);

        List<CardModel> strikes = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(CardTag.Strike)).ToList();
        List<CardModel> defends = PileType.Deck.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(CardTag.Defend)).ToList();

        int count = strikes.Count + defends.Count;

        foreach (CardModel item in strikes.Concat(defends))
        {
            await CardPileCmd.RemoveFromDeck(item);
        }

        await Cmd.Wait(0.75f);

        for (int i = 0; i < count; i++)
        {
            await CardPileCmd.Add(Owner.RunState.CreateCard<IronWave>(Owner), PileType.Deck);
        }
    }
}
