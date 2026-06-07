using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Attack;

public class TanSheCard : YunoBaseCard, IOnLingHuo
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
        new DynamicVar("LingHuoDamage", 3m),
        new RepeatVar(3),
    };

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.LingHuo, YunoKeywords.Gun];

    public TanSheCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Gun),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.LingHuo),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await ToolCmd.GunAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue);

        await ToolCmd.GunStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await DamageCmd.Attack(DynamicVars["LingHuoDamage"].BaseValue)
         .FromCard(this)
         .TargetingRandomOpponents(CombatState!)
         .WithHitCount(DynamicVars.Repeat.IntValue)
         .WithHitFx("vfx/vfx_attack_blunt")
         .Execute(ctx);

        await ToolCmd.GunStance(ctx, Owner, this);
    }
}
