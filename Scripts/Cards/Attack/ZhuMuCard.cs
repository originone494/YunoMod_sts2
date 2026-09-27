using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Tool;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Entities.Players;

namespace YunoMod.Scripts.Cards.Attack;

public class ZhuMuCard : YunoBaseCard, IOnGetLove
{


    public ZhuMuCard() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {

    }


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(23,ValueProp.Move)

    ];

    public async Task OnGetLove(PlayerChoiceContext ctx, Player player, int amount)
    {
        // 只响应自己获得爱意（多人下队友获得爱意不触发）
        if (player != Owner) return;

        if (Pile?.Type == PileType.Hand)
            await CardCmd.AutoPlay(ctx, this, player.Creature);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this, cardPlay)
        .TargetingAllOpponents(CombatState!)
        .WithHitFx("vfx/vfx_dramatic_stab")
        .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}
