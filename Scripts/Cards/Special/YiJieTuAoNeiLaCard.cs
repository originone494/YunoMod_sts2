using System;
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
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;

namespace YunoMod.Scripts.Cards.Special;

// 异解·图奥内拉：获得格挡，向消耗堆加入3张随机卡，之后可从消耗堆打出1张异解怪兽卡
public class YiJieTuAoNeiLaCard : YunoSpecialBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(20m, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.YiJie),
        HoverTipFactory.FromKeyword(YunoKeywords.YiJieGuaiShou),
    ];

    public YiJieTuAoNeiLaCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [YunoTags.YiJie, YunoTags.YiJieGuaiShou];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 1. 获得20点格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 2. 向消耗堆加入3张随机卡（全卡池完全随机，各抽各的）
        var copies = new List<CardModel>();
        for (int i = 0; i < 3; i++)
        {
            var randomCanonical = Owner.RunState.Rng.Niche.NextItem(ModelDb.AllCards)!;
            copies.Add(Owner.Creature.CombatState!.CreateCard(randomCanonical!, Owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Exhaust, Owner);

        // 3. 从消耗堆选择1张「异解怪兽」卡打出（可以不选）
        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, 1);
        var picked = (await CardSelectCmd.FromCombatPile(
            choiceContext,
            PileType.Exhaust.GetPile(Owner),
            Owner,
            prefs,
            filter: c => c.Tags.Contains(YunoTags.YiJieGuaiShou))).FirstOrDefault();

        if (picked != null)
        {
            await CardCmd.AutoPlay(choiceContext, picked, null);
        }
    }

    // 敌人数量大于1的情况下，这张卡可以免费打出
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || originalCost <= 0m) return false;
        if ((card.Owner.Creature.CombatState?.HittableEnemies.Count ?? 0) <= 1) return false;

        modifiedCost = 0m;
        return true;
    }
}
