using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·劈弦（魔法）
//   不可打出 + 保留
//   驻场：一回合一次（按这张卡自己，不是按卡名：手上有多张就各触发一次）
//         → 打出「珠泪怪兽」卡时（本卡在手上）：从抽牌堆顶将3张卡送入弃牌堆，所有敌人在本回合失去5点力量
//   灵活：「检索」1张「珠泪陷阱」卡
public class ZhuLeiPiXianCard : YunoSpecialBaseCard, ILingHuoCard
{
    // 「一回合一次」按实例记：记"哪一回合用过"，换回合/换战斗自动失效
    private (CombatId? Combat, int Turn)? _zhuChangUsedKey;

    public ZhuLeiPiXianCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.LingHuo,
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiMoFa,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Unplayable, CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiMoFa),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    // 不可打出：这只是一个兜底实现（引擎不会让这张卡被打出）
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

    // 驻场：一回合一次。打出「珠泪怪兽」卡时，从抽牌堆顶将3张卡送入弃牌堆，所有敌人在本回合失去5点力量
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;   // 驻场：本卡在手上才算
        if (cardPlay.Card.Owner != Owner) return;
        if (!ZhuLeiFilter.IsMonster(cardPlay.Card)) return;               // 只认「珠泪怪兽」（含融合怪兽）

        // 「一回合一次」按这张卡自己算（手上有几张同名卡就能各触发一次）
        var now = PerTurnOnce.CurrentKey(Owner);
        if (_zhuChangUsedKey == now) return;
        _zhuChangUsedKey = now;

        await ToolCmd.DuiMu(choiceContext, Owner, 3);

        // 所有敌人在本回合失去5点力量（它们的回合结束时自动恢复）
        await PowerCmd.Apply<ZhuLeiPiXianTempDownPower>(
            choiceContext,
            CombatState.HittableEnemies,
            5m,
            Owner.Creature,
            this);
    }

    // 灵活：「检索」1张「珠泪陷阱」卡
    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.RetrieverCard(
            ctx,
            player,
            c => ZhuLeiFilter.IsTrap(c),
            p => p is YunoSpecialCardPool,
            1);
    }
}
