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
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·俱舍（下级怪兽）
//   打出（0费）：先让玩家选1张弃牌堆的卡消耗；不选 → 失去2费。
//                然后造成伤害、从抽牌堆顶将3张卡送入弃牌堆
//   灵活：从抽牌堆顶将2张卡送入弃牌堆
public class ZhuLeiJuSheCard : YunoSpecialBaseCard, ILingHuoCard
{
    public ZhuLeiJuSheCard() : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 伤害使用动态变量：造成 23 点伤害
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(20m, ValueProp.Move),
    };

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.LingHuo,
        YunoTags.JuShe,
        YunoTags.ZhuLeiXiaJiGuaiShou,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.JuShe),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiXiaJiGuaiShou),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ① 先让玩家选择弃牌堆的卡消耗；不选 → 失去2费
        var discardPile = PileType.Discard.GetPile(Owner);
        CardModel? chosen = discardPile.Cards.Count == 0
            ? null
            : (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                discardPile.Cards.ToList(),
                Owner,
                CardPrefs(this, SelectionScreenPrompt, 0, 1))).FirstOrDefault();

        if (chosen != null)
        {
            await CardCmd.Exhaust(choiceContext, chosen);
        }
        else
        {
            await PlayerCmd.LoseEnergy(2, Owner);
        }

        // ② 造成伤害
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, cardPlay);

        // ③ 从抽牌堆顶将3张卡送入弃牌堆
        await ToolCmd.DuiMu(choiceContext, Owner, 3);
    }

    // 灵活：从抽牌堆顶将2张卡送入弃牌堆（触发效果而非打出）

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.DuiMu(ctx, player, 2);
    }
}
