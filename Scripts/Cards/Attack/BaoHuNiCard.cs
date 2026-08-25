using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Attack;

public class BaoHuNiCard : YunoBaseCard
{
    private const string _stabCountKey = "StabCount";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),   // 造成 9 点伤害（{Damage:diff()}）
        new DynamicVar(_stabCountKey, 1m),   // 抽到时加入的刺伤数量（{StabCount:diff()}）
    ];

    public BaoHuNiCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Dagger];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
        HoverTipFactory.FromCard<CiShangCard>()

    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 造成9点伤害
        await ToolCmd.DaggerAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.IntValue, cardPlay);

        // 丢弃手牌所有灵活卡
        var hand = PileType.Hand.GetPile(Owner);
        foreach (var card in hand.Cards.Where(c => c.Keywords.Contains(YunoKeywords.LingHuo)).ToList())
        {
            await CardCmd.Discard(choiceContext, card);
        }
    }

    // 抽到时，将 N 张带有灵活的「刺伤」加入手牌
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;

        for (int i = 0; i < DynamicVars[_stabCountKey].IntValue; i++)
        {
            CardModel stab = Owner.Creature.CombatState!.CreateCard<CiShangCard>(Owner);
            stab.AddModKeyword(YunoKeywords.LingHuo);
            await CardPileCmd.Add(stab, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_stabCountKey].UpgradeValueBy(1);
    }
}
