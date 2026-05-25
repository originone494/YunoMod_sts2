using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Entities.Players;
namespace YunoMod.Scripts.Cards.Attack;

public class RuoDianJiPoCard : YunoBaseCard, IOnLingHuo
{

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new RepeatVar( 3),
    };

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.LingHuo, YunoKeywords.Dagger];


    public RuoDianJiPoCard() : base(2, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Dagger),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        Creature lowestHpEnemy = CombatState!.HittableEnemies
            .Where(e => e.IsAlive)
            .OrderBy(e => e.CurrentHp)
            .FirstOrDefault()!;

        if (lowestHpEnemy == null) return;

        await ToolCmd.DaggerAttack(choiceContext, lowestHpEnemy, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        await ToolCmd.DaggerStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await CardCmd.AutoPlay(ctx, this, player.Creature);

        await CardPileCmd.Draw(ctx, 1, Owner);
    }
}
