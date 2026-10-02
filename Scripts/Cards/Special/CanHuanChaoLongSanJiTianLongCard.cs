using MegaCrit.Sts2.Core.Combat;
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

// 游戏王「燦幻超龍トランセンド・ドラギオン」（Sangenpai Transcendent Dragion，站内 sc_name「灿幻超龙 三极天龙」）：
//   打出：造成9点伤害
//   登场：「检索」1张「灿幻」卡（灿幻魔法/灿幻陷阱/灿幻怪兽）
//   时机：回合结束时，打出此卡，可以进行一次「同调」
public class CanHuanChaoLongSanJiTianLongCard : YunoSpecialBaseCard, IDengChangCard
{
    public CanHuanChaoLongSanJiTianLongCard() : base(3, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.LongZu,
        YunoTags.ShiXing,
        YunoTags.CanHuanGuaiShou,
        YunoTags.DengChang,
        YunoTags.TongBu,

    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [

    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LongZu),
        HoverTipFactory.FromKeyword(YunoKeywords.ShiXing),
        HoverTipFactory.FromKeyword(YunoKeywords.TongBu),
        HoverTipFactory.FromKeyword(YunoKeywords.CanHuanGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.DengChang),
        HoverTipFactory.FromKeyword(YunoKeywords.TongDiao),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成9点伤害
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // 登场：「检索」1张「灿幻」卡
    public async Task DengChangSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.RetrieverCard(
            ctx,
            player,
            c => c.Tags.Contains(YunoTags.CanHuanMoFa)
                 || c.Tags.Contains(YunoTags.CanHuanXianJing)
                 || c.Tags.Contains(YunoTags.CanHuanGuaiShou),
            p => p is YunoSpecialCardPool,
            1);
    }

    // 时机：回合结束时，打出此卡，可以进行一次「同调」
    // 打出此卡后，返回手牌，之后可以进行一次「同调」（同名卡一回合一次）
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != this) return;
        if (Owner == null) return;

        var onceKey = PerTurnOnce.Key("Play", Id.Entry);
        if (PerTurnOnce.IsUsed(Owner, onceKey)) return;
        PerTurnOnce.Mark(Owner, onceKey);

        // 打出后返回手牌；回合结束的最后由游戏机制正常清空手牌（连同它一起进弃牌堆）
        await CardPileCmd.Add(this, PileType.Hand);

        // 可以进行一次「同调」
        await TongDiao.TryTongDiao(choiceContext, Owner, this);
    }
}
