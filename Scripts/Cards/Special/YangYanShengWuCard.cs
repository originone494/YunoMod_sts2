using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Cards.Other;

// 「羊衍生物」：无法打出，抽到时消耗，按费用次数获得8点格挡，然后抽1张牌
public class YangYanShengWuCard : YunoBaseCard
{
    private const string _repeatKey = "Repeat";

    public YangYanShengWuCard() : base(0, CardType.Attack, CardRarity.Token, TargetType.Self)
    {
    }

    // 无法打出
    protected override bool IsPlayable => false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(8m, ValueProp.Move),      // 每次获得的格挡
        new DynamicVar(_repeatKey, 0m),        // 重复次数，由替罪羊按费用设置
    ];

    // 抽到时触发（该钩子对每张被抽的卡都会调用，必须判断是本卡）
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this) return;

        await CardCmd.Exhaust(choiceContext, this);
        for (int i = 0; i < DynamicVars[_repeatKey].IntValue; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
        }
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
}