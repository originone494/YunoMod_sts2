using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「愚蠢的埋葬」（Foolish Burial）：「检索」并丢弃 1 张攻击牌。
// 复用 RetrieverCard 的 isDiscard 形态（与珠泪·爪音同款），poolFilter 传 null 不限卡池。
public class YuChunDeMaiZangCard : YunoSpecialBaseCard
{
    private static LocString RetrievePrompt { get; } = new("cards", "YUNO_MOD_CARD_YU_CHUN_DE_MAI_ZANG_CARD.selectionScreenPrompt");

    public YuChunDeMaiZangCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 「检索」（全卡池）并丢弃 1 张攻击牌
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Type == CardType.Attack,
            null,
            1,
            isDiscard: true,
            prompt: RetrievePrompt);
    }
}
