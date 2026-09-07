using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Power.PowerCard;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·劈弦：打出「珠泪」卡时，从抽牌堆顶将3张牌送入弃牌堆（由 ZhuLeiPiXianPower 承载）；
// 灵活：检索1张「珠泪魔陷」卡加入手牌。
public class ZhuLeiPiXianCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiPiXianCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.LingHuo,
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiMoXian,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ZhuLeiPiXianPower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiMoXian),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ZhuLeiPiXianPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    // 灵活：检索1张「珠泪魔陷」卡加入手牌
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.RetrieverCard(
            ctx,
            player,
            c => c.Tags.Contains(YunoTags.ZhuLeiMoXian),
            p => p is YunoSpecialCardPool,
            1);
    }
}
