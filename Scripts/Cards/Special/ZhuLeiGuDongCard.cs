using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·鼓动：给予所有敌人2层虚弱，丢弃1张手牌。
// 灵活：从消耗堆将1张「珠泪魔法」卡加入手牌。
public class ZhuLeiGuDongCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiGuDongCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2),
    ];

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.LingHuo,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiMoFa),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ① 给予所有敌人2层虚弱
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            CombatState!.HittableEnemies,
            DynamicVars.Weak.IntValue,
            Owner.Creature,
            this);

        // ② 丢弃1张手牌（玩家选择，手牌为空则跳过）
        if (PileType.Hand.GetPile(Owner).Cards.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this)).ToList();

        if (selected.Count == 0) return;

        await CardCmd.Discard(choiceContext, selected[0]);
    }

    // 灵活：从消耗堆将1张「珠泪魔法」卡加入手牌
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        if (!exhaustPile.Cards.Any(c => c.Tags.Contains(YunoTags.ZhuLeiMoFa))) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            exhaustPile,
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            filter: c => c.Tags.Contains(YunoTags.ZhuLeiMoFa))).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}
