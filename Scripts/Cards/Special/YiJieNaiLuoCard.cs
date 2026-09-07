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

// 异解·奈落：造成伤害，向消耗堆加入4张随机卡，之后可从消耗堆打出1张异解怪兽卡
public class YiJieNaiLuoCard : YunoSpecialBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(17m, ValueProp.Move),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.YiJie),
        HoverTipFactory.FromKeyword(YunoKeywords.YiJieGuaiShou),
    ];

    public YiJieNaiLuoCard() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [YunoTags.YiJie, YunoTags.YiJieGuaiShou];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 1. 造成17点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 2. 向消耗堆加入4张随机卡（全卡池完全随机，各抽各的）
        var copies = new List<CardModel>();
        for (int i = 0; i < 4; i++)
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

    // 场上存在带有攻击意图的敌人的情况下，这张卡可以免费打出
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || originalCost <= 0m) return false;
        if (card.Owner.Creature.CombatState?.HittableEnemies.Any(e => e.Monster != null && e.Monster.IntendsToAttack) != true) return false;

        modifiedCost = 0m;
        return true;
    }
}
