using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class TianXiaDuBuDeDaYiZeiCard : YunoSpecialBaseCard
{
    public TianXiaDuBuDeDaYiZeiCard() : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 明确声明「不可被格挡类附魔」：本卡的格挡在「受到攻击前」的反应里获得，
    // CreatureCmd.GainBlock 只能传 null cardPlay，而引擎取 cardSource = cardPlay?.Card，
    // 所以灵活(Nimble)的 +格挡 加不上。若沿用基类的自动判定（有 BlockVar → true），
    // 菲涅尔透镜会把它附上、卡面还会预览出附魔后的数值，但实际给的格挡不变——显示与结算不符。
    public override bool GainsBlock => false;

    // 伤害使用动态变量：造成 25 点伤害
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(20m, ValueProp.Move),
        // 卡面直接写了「获得20点格挡」，格挡数值是看得见的，因此声明 Block 动态变量。
        new BlockVar(16m, ValueProp.Move)
    };

    // 「应对」：仅作为代码标记（标签），没有卡面注入；说明走悬停提示
    protected override HashSet<CardTag> CanonicalTags => [YunoTags.YingDui];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(YunoKeywords.YingDui),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 顺序处理，处理到不能处理为止：
        // ① 失去2费（能量足够时）

        // ② 费用不足：杀死敌方所有爪牙（存在爪牙时）
        if (Owner.Creature.CombatState!.HittableEnemies.Any(e => e.HasPower<MinionPower>()))
        {
            foreach (Creature enemy in Owner.Creature.CombatState!.HittableEnemies.Where(e => e.HasPower<MinionPower>()).ToList())
            {
                await CreatureCmd.Kill(enemy);
            }
        }
        // ③ 没有爪牙可杀：玩家选择丢弃2张攻击牌（选不够2张则效果停止）
        else
        {
            var attackCards = PileType.Hand.GetPile(Owner).Cards.Where(c => c.Type == CardType.Attack).ToList();
            if (attackCards.Count < 2) return;

            var selected = (await CardSelectCmd.FromHand(
                prefs: CardPrefs(this, SelectionScreenPrompt, 0, 2),
                context: choiceContext,
                player: Owner,
                filter: c => c.Type == CardType.Attack,
                source: this)).ToList();
            if (selected.Count < 2)
            {
                if (Owner.PlayerCombatState!.Energy >= 2)
                {
                    await PlayerCmd.LoseEnergy(2, this.Owner);

                }
                else
                {
                    return;
                }

            }
            else
            {
                await CardCmd.Discard(choiceContext, selected);

            }

        }

        // 造成25点伤害
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, cardPlay);
    }

    // 在受到攻击前：将弃牌堆的这张卡返回抽牌堆，获得20点格挡
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (CombatState == null) return;
        if (!PileType.Discard.GetPile(Owner).Cards.Contains(this)) return;
        if (cardSource != null) return;
        if (dealer != null && dealer.IsMonster)
        {
            await CardPileCmd.Add(this, PileType.Draw);
            await CreatureCmd.GainBlock(Owner.Creature, 20m, ValueProp.Move, null);
        }


    }
}
