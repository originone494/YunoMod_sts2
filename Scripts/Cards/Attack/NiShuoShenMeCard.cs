using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Power;
using YunoMod.Scripts.Tool;

using STS2RitsuLib.Keywords;
using MegaCrit.Sts2.Core.Entities.Players;
namespace YunoMod.Scripts.Cards.Skill;

public class NiShuoShenMeCard : YunoBaseCard, IOnLingHuo
{

    public NiShuoShenMeCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {

    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8,ValueProp.Move),
        new PowerVar<WeakPower>(1)
    };


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Dagger, YunoKeywords.LingHuo];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    HoverTipFactory.FromPower<LiuXuePower>(),
        HoverTipFactory.FromKeyword(YunoKeywords.Dagger),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await CardCmd.AutoPlay(ctx, this, player.Creature);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await ToolCmd.DaggerAttackAllEnemy(choiceContext, this, DynamicVars.Damage.BaseValue);

        await PowerCmd.Apply<WeakPower>(choiceContext, CombatState!.HittableEnemies, DynamicVars.Weak.IntValue, Owner.Creature, this);


        await ToolCmd.DaggerStance(choiceContext, Owner, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1);

    }
}
