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
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Cards.Special;

// 珠泪·残响：抽2张牌。攻击意图：获得10点格挡，给予所有敌人2层虚弱。
// 灵活：从消耗堆将1张「珠泪怪兽」卡加入手牌。
public class ZhuLeiCanXiangCard : YunoSpecialBaseCard, IOnLingHuo
{
    public ZhuLeiCanXiangCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(6m, ValueProp.Move),
        new PowerVar<WeakPower>(1),
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
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ① 抽2张牌
        await CardPileCmd.Draw(choiceContext, 2, Owner);

        // ② 攻击意图：获得10点格挡，给予所有敌人2层虚弱
        if (CombatState == null) return;
        if (!CombatState.HittableEnemies.Any(e => e.Monster != null && e.Monster.IntendsToAttack)) return;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            CombatState.HittableEnemies,
            DynamicVars.Weak.IntValue,
            Owner.Creature,
            this);
    }

    // 灵活：从消耗堆将1张「珠泪怪兽」卡加入手牌
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        if (!exhaustPile.Cards.Any(c => c.Tags.Contains(YunoTags.ZhuLeiGuaiShou))) return;

        var picked = (await CardSelectCmd.FromCombatPile(
            ctx,
            exhaustPile,
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, 1, 1),
            filter: c => c.Tags.Contains(YunoTags.ZhuLeiGuaiShou))).FirstOrDefault();

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
