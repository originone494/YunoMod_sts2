using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Special;

public class MingYunHuaiBaoDeBaoDanCard : YunoSpecialBaseCard
{
    public MingYunHuaiBaoDeBaoDanCard() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    // 这张卡无法打出
    protected override bool IsPlayable => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
    ];

    // 敌人攻击前：消耗手牌的这张卡，获得10点格挡，对进行攻击的敌人造成等额伤害
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (CombatState == null) return;
        if (cardSource != null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;
        if (dealer == null) return;
        if (!dealer.IsMonster) return;

        // 消耗手牌的这张卡
        await CardCmd.Exhaust(choiceContext, this);

        // 获得10点格挡
        await CreatureCmd.GainBlock(Owner.Creature, 10m, ValueProp.Move, null);

        // 对进行攻击的敌人造成等额伤害
        await CreatureCmd.Damage(choiceContext, dealer, amount, ValueProp.Move, Owner.Creature, this, null);

        // 卡组存在「现世与冥界的逆转」时，自身获得与伤害等额的格挡
        if (Owner.Deck.Cards.Any(c => c is XianShiYuMingJieDeNiZhuanCard))
        {
            await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Move, null);
        }
    }
}
