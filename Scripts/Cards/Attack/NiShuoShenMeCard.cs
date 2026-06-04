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
namespace YunoMod.Scripts.Cards.Skill;

public class NiShuoShenMeCard : YunoBaseCard
{

    public NiShuoShenMeCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5,ValueProp.Move),
        new RepeatVar(1)
    };

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Dagger,YunoKeywords.LingHuo];

        protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LiuXuePower>(),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Dagger),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];

    


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await ToolCmd.DaggerAttackAllEnemy(choiceContext, this, DynamicVars.Damage.BaseValue);

        foreach (var enemy in CombatState!.HittableEnemies)
        {
            for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                if (enemy.HasPower<LiuXuePower>())
                {

                    var LoseBloodPower = enemy.GetPower<LiuXuePower>();
                    int powerCount = LoseBloodPower!.Amount;
                    await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), enemy, powerCount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
                    if (enemy.IsAlive)
                    {
                        await PowerCmd.Decrement(LoseBloodPower);
                    }

                    await BleedHook.OnBleedDamage(choiceContext, Owner.Creature.CombatState!, enemy, powerCount);

                }
            }
        }

        await ToolCmd.DaggerStance(choiceContext, Owner, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
