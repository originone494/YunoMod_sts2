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

public class ZhuLeiXiaoMeiCard : YunoSpecialBaseCard, IOnLingHuo
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
        YunoTags.ZhuLeiRongHe

    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        YunoKeywords.ZhuLeiRongHe,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiRongHe),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得10点格挡
        await CreatureCmd.GainBlock(Owner.Creature, 10m, ValueProp.Move, cardPlay);

        // 从抽牌堆顶将3张卡送入弃牌堆
        await ToolCmd.DuiMu(choiceContext, Owner, 3);
    }

    // 灵活：触发珠泪融合（返回卡组融合）
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ZhuLeiFusion.TryFusion(ctx, player, this);
    }
}
