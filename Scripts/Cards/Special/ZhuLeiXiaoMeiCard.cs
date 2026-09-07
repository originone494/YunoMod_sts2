using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.LingHuo

    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
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
