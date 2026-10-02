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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·残响（陷阱）
//   应对：受到敌人攻击前（本卡在手上）→ 对攻击者造成6点伤害、获得7点格挡、对攻击者给予2层虚弱，
//         处理完把这张卡打出（带「消耗」→ 进消耗堆）
//   灵活：从消耗堆将1张「珠泪怪兽」卡加入手牌
//   打出没有效果（按新文案）：靠「应对」与「灵活」生效，手动打出只是把它消耗掉
public class ZhuLeiCanXiangCard : YunoSpecialBaseCard, ILingHuoCard
{
    public ZhuLeiCanXiangCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move),
        new BlockVar(7m, ValueProp.Move),
        new PowerVar<WeakPower>(2),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.LingHuo,
        YunoTags.ZhuLeiXianJing,
        YunoTags.YingDui,
    ];

    // 「应对」不进关键字列表（卡面靠"应对：…"纯文本），只挂在悬停提示里；
    // 「保留」是这一版新加的（用户要求：应对从手牌触发，所以让它留得住）
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.YingDui),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiXianJing),
    ];

    // 打出没有效果（按新文案）：这张卡靠「应对」与「灵活」生效，打出只是把它消耗掉
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

    // 应对：受到敌人攻击前（参考小美 / 天下独步的大义贼 / 露莎卡）
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;
        if (cardSource != null) return;                 // 只要"受到敌人攻击"
        if (dealer == null || !dealer.IsMonster) return;

        // ① 造成6点伤害（对攻击者）
        await CreatureCmd.Damage(choiceContext, dealer, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, null);

        // ② 获得7点格挡（在扣血之前，所以能挡下这次攻击）
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);

        // ③ 给予2层虚弱（文本没写目标 → 对攻击者）
        await PowerCmd.Apply<WeakPower>(choiceContext, dealer, DynamicVars.Weak.IntValue, Owner.Creature, this);

        // ④ 效果处理完，把这张卡打出——它带「消耗」，于是被消耗掉
        //    （AutoPlay 只在 card.Pile == null 时才搬进 Play，手牌里的卡要先自己搬）
        await CardPileCmd.Add(this, PileType.Play);
        await CardCmd.AutoPlay(choiceContext, this, null);
    }

    // 灵活：从消耗堆将1张「珠泪怪兽」卡加入手牌（按效果文本口径 = 含融合怪兽）
    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        if (!exhaustPile.Cards.Any(ZhuLeiFilter.IsMonster)) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            exhaustPile,
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            filter: ZhuLeiFilter.IsMonster)).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}
