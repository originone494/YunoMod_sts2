using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

public class ZhuLeiXiaoMeiCard : YunoSpecialBaseCard, ILingHuoCard
{
    public ZhuLeiXiaoMeiCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.Self)
    {
    }

    // 只用于声明「这张牌能给格挡」：原版菲涅尔透镜的灵活(Nimble)附魔靠 CardModel.GainsBlock
    // 判定资格，而基类按 CanonicalVars 里是否存在 BlockVar 自动推导该属性。
    // 实际格挡值仍在 OnPlay 里直接传入——BlockVar 的附魔加成只作用于预览，不会与实际值重复计算。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10m, ValueProp.Move),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.LingHuo,
        YunoTags.ZhuLeiRongHe,
        YunoTags.ZhuLeiXiaJiGuaiShou,
        YunoTags.YingDui

    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        YunoKeywords.ZhuLeiRongHe,
        CardKeyword.Retain,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiRongHe),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        // 「应对」只在这里出现：卡面靠"应对：…"纯文本，悬停时给出这个关键字的说明
        HoverTipFactory.FromKeyword(YunoKeywords.YingDui),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiXiaJiGuaiShou),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得10点格挡
        await CreatureCmd.GainBlock(Owner.Creature, 10m, ValueProp.Move, cardPlay);

        // 从抽牌堆顶将3张卡送入弃牌堆
        await ToolCmd.DuiMu(choiceContext, Owner, 3);
    }

    // 「受到敌人攻击前，将这张卡打出」——参考天下独步的大义贼的 BeforeDamageReceived 写法。
    // 差别在于大义贼是"从弃牌堆返回抽牌堆 + 获得格挡"，本卡是"从手牌打出"（走自己的 OnPlay）。
    // 打出后它离开手牌、结算完落进弃牌堆，所以同一份拷贝不会在一次攻击里重复触发；
    // 配合「保留」关键字，没被打出时会留在手上等下一次攻击。
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;
        if (cardSource != null) return;                 // 只要"受到敌人攻击"，排除卡牌/能力造成的伤害
        if (dealer == null || !dealer.IsMonster) return;

        // AutoPlay 只在 card.Pile == null 时才把牌搬进 Play（CardCmd.cs:114-117），
        // 手牌里的卡必须先自己搬——与官方 CardPileCmd.AutoPlayFromDrawPile 同款做法。
        await CardPileCmd.Add(this, PileType.Play);
        await CardCmd.AutoPlay(choiceContext, this, null);
    }

    // 灵活：触发珠泪融合（返回卡组融合）

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ZhuLeiFusion.TryFusion(ctx, player, this);
    }
}
