using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;

namespace YunoMod.Scripts.Cards.Special;

// 异解·冥神府：向消耗堆加入7张随机卡，造成伤害，抽牌数随消耗堆中的异解卡数量增长
public class YiJieMingShenFuCard : YunoSpecialBaseCard
{
    private const int _randomCards = 7;
    private const int _baseDraw = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(21m, ValueProp.Move),
    ];

    public YiJieMingShenFuCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    protected override HashSet<CardTag> CanonicalTags => [YunoTags.YiJie, YunoTags.YiJieGuaiShou];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 1. 向消耗堆加入7张随机卡（全卡池完全随机，各抽各的）
        var copies = new List<CardModel>();
        for (int i = 0; i < _randomCards; i++)
        {
            var randomCanonical = Owner.RunState.Rng.Niche.NextItem(ModelDb.AllCards)!;
            copies.Add(Owner.Creature.CombatState!.CreateCard(randomCanonical!, Owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Exhaust, Owner);

        // 2. 造成21点伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 3. 抽2张牌；消耗堆中每有1张「异解」卡，额外多抽1张（在7张随机卡入堆之后统计）
        int yiJieCount = PileType.Exhaust.GetPile(Owner).Cards.Count(c => c.Tags.Contains(YunoTags.YiJie));
        await CardPileCmd.Draw(choiceContext, _baseDraw + yiJieCount, Owner);
    }
}
