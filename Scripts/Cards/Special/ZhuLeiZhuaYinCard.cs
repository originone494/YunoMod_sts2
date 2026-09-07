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
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·爪音：先古魔陷。让目标在这个回合失去99点力量（其回合结束自动恢复），
// 「检索」并丢弃1张「珠泪」卡（丢弃的珠泪怪兽会触发其灵活效果）；
// 灵活：检索1张「珠泪怪兽」卡加入手牌。
public class ZhuLeiZhuaYinCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiZhuaYinCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.LingHuo,
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiMoXian,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiMoXian),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 在这个回合失去99点力量（目标回合结束自动恢复）
        await PowerCmd.Apply<ZhuLeiZhuaYinTempDownPower>(choiceContext, cardPlay.Target, 99m, Owner.Creature, this);

        // 「检索」并丢弃1张「珠泪」卡（珠泪=带珠泪标签且非珠泪融合卡，与珍珠世界口径一致）
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.ZhuLei) && !c.Tags.Contains(YunoTags.ZhuLeiRongHe),
            p => p is YunoSpecialCardPool,
            1,
            isDiscard: true);
    }

    // 灵活：检索1张「珠泪怪兽」卡加入手牌
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.RetrieverCard(
            ctx,
            player,
            c => c.Tags.Contains(YunoTags.ZhuLeiGuaiShou),
            p => p is YunoSpecialCardPool,
            1);
    }
}
