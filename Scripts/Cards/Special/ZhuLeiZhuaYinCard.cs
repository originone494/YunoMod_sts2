using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·爪音（陷阱）
//   应对：受到敌人攻击前（本卡在手上）→ 击晕攻击者，然后「检索」并丢弃1张「珠泪下级怪兽」卡，
//         处理完把这张卡打出（带「消耗」→ 进消耗堆，所以同一份拷贝只触发一次）
//   灵活：从弃牌堆将1张「珠泪怪兽」卡加入手牌
//   打出没有效果（按新文案）：靠「应对」与「灵活」生效，手动打出只是把它消耗掉
public class ZhuLeiZhuaYinCard : YunoSpecialBaseCard, ILingHuoCard
{
    // 目标改成自身：新文案里打出没有效果、也没有需要指定的敌人，留"指定敌人"只会让玩家白点一下
    public ZhuLeiZhuaYinCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.LingHuo,
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiXianJing,
        YunoTags.YingDui,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.YingDui),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiXianJing),
        HoverTipFactory.FromKeyword(YunoKeywords.Retriever),
    ];

    // 打出没有效果（按新文案）：这张卡靠「应对」与「灵活」生效，打出只是把它消耗掉
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

    // 应对：受到敌人攻击前（参考小美 / 天下独步的大义贼 / 露莎卡 / 残响）
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;
        if (cardSource != null) return;                 // 只要"受到敌人攻击"
        if (dealer == null || !dealer.IsMonster) return;

        // ① 击晕敌人（攻击者）。用原版的 CreatureCmd.Stun —— 原版卡「口哨」就是这么做的
        await CreatureCmd.Stun(dealer);

        // ② 「检索」并丢弃1张「珠泪下级怪兽」卡
        await ToolCmd.RetrieverCard(
            choiceContext,
            Owner,
            ZhuLeiFilter.IsLowerMonster,
            p => p is YunoSpecialCardPool,
            1,
            isDiscard: true);

        // ③ 效果处理完，把这张卡打出——它带「消耗」，于是被消耗掉。
        //    这样同一份拷贝不会一直留在手上、每次被攻击都重复触发
        //    （AutoPlay 只在 card.Pile == null 时才搬进 Play，手牌里的卡要先自己搬）
        await CardPileCmd.Add(this, PileType.Play);
        await CardCmd.AutoPlay(choiceContext, this, null);
    }

    // 灵活：从弃牌堆将1张「珠泪怪兽」卡加入手牌
    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        var discardPile = PileType.Discard.GetPile(player);
        if (!discardPile.Cards.Any(ZhuLeiFilter.IsMonster)) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            discardPile,
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            filter: ZhuLeiFilter.IsMonster)).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }
}
