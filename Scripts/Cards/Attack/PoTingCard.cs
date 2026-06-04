using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
namespace YunoMod.Scripts.Cards.Attack;

public class PoTingCard : YunoBaseCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(2m, ValueProp.Move),
        new RepeatVar(4),
    };

    public PoTingCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }
    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Gun];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Gun),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.GunAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        // 从抽牌堆检索1张攻击牌加入手牌
        CardModel attackCard = PileType.Draw.GetPile(Owner).Cards
            .FirstOrDefault(card => card.Type == CardType.Attack);
        if (attackCard != null)
        {
            await CardPileCmd.Add(attackCard, PileType.Hand);
        }

        await ToolCmd.GunStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
