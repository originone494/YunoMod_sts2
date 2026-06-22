using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Cards.Attack;

public class ChengShengZhuiJiCard : YunoBaseCard, IOnForesee
{
    private const string _growthKey = "Growth";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),
        new DynamicVar(_growthKey, 3m),
    };

    public ChengShengZhuiJiCard() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Foresee),
    ];

    

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);

        // 每使用一次永久成�?
        DynamicVars.Damage.BaseValue += DynamicVars[_growthKey].BaseValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[_growthKey].UpgradeValueBy(2m);
    }

    public Task OnForesee(PlayerChoiceContext ctx, Player player, int amount, int discardedAmount)
    {
        if (player == base.Owner && Pile!.Type == PileType.Discard)
        {
            CardPileCmd.Add(this, PileType.Hand);

        }
        return Task.CompletedTask;
    }

}
