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
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Special;

public class ZhuLeiSaiRenCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiSaiRenCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 伤害使用动态变量：造成 18 点伤害
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(18m, ValueProp.Move),
    };

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.LingHuo,

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
        // 造成18点伤害
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, cardPlay);

        // 丢弃1张手牌（玩家选择，手牌为空则跳过）
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (handCards.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this)).ToList();
        if (selected.Count == 0) return;

        await CardCmd.Discard(choiceContext, selected[0]);
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
