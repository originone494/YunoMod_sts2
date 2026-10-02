using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 游戏王「幻禄の天盃龍」（Tenpai Dragon Genroku，站内 cn_name/sc_name「幻禄(之)天杯龙」）：
//   打出：造成1点伤害，「检索」除自身以外的1张「天杯龙」
//   登场：这张卡可以免费打出，变为「调整」，等级上升1星（移除「三星」获得「四星」）
//
// 登场的星级上升同时更新关键字与标签（同调效果的星级判定读的是标签）；
// 标签底层的 HashSet 直接变更（游戏注释说明标签默认不可变，这里是刻意为之的实例级变化）。
public class TianBeiLongHuanLuCard : YunoSpecialBaseCard, IDengChangCard
{
    public TianBeiLongHuanLuCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 登场后可免费打出
    private bool _canFreePlay;

    // 登场后已完成星级上升
    private bool _leveledUp;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(1m, ValueProp.Move),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.TianBeiLong,
        YunoTags.LongZu,
        YunoTags.SanXing,
        YunoTags.DengChang,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];

    // 悬停提示随登场状态变化：三星 ↔（调整 +）四星
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => _leveledUp
        ? [
            HoverTipFactory.FromKeyword(YunoKeywords.TianBeiLong),
            HoverTipFactory.FromKeyword(YunoKeywords.LongZu),
            HoverTipFactory.FromKeyword(YunoKeywords.SiXing),
            HoverTipFactory.FromKeyword(YunoKeywords.TiaoZheng),
            HoverTipFactory.FromKeyword(YunoKeywords.DengChang),
        ]
        : [
            HoverTipFactory.FromKeyword(YunoKeywords.TianBeiLong),
            HoverTipFactory.FromKeyword(YunoKeywords.LongZu),
            HoverTipFactory.FromKeyword(YunoKeywords.SanXing),
            HoverTipFactory.FromKeyword(YunoKeywords.DengChang),
            HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
        ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成1点伤害
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 「检索」除自身以外的1张「天杯龙」
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            c => c.Tags.Contains(YunoTags.TianBeiLong) && c.Id != Id,
            p => p is YunoSpecialCardPool,
            1);
    }

    // 登场：这张卡可以免费打出，变为「调整」，等级上升1星
    public async Task DengChangSpecial(PlayerChoiceContext ctx, Player player)
    {
        _canFreePlay = true;

        AddKeyword(YunoKeywords.TiaoZheng);   // 变为「调整」
        RemoveKeyword(YunoKeywords.SanXing);  // 等级上升1星
        AddKeyword(YunoKeywords.SiXing);

        if (Tags is HashSet<CardTag> tags)
        {
            tags.Remove(YunoTags.SanXing);
            tags.Remove(YunoTags.SiXing);
            tags.Remove(YunoTags.TiaoZheng);
            tags.Add(YunoTags.SiXing);
            tags.Add(YunoTags.TiaoZheng);
        }

        _leveledUp = true;
    }

    // 登场后免费打出
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !_canFreePlay) return false;

        modifiedCost = 0m;
        return true;
    }
}
