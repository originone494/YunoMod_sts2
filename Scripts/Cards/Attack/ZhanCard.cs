using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;
using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Cards.Attack;

public class ZhanCard : YunoBaseCard
{
    private const string _prevDamageKey = "PrevDamage";
    private const string _prevPrevDamageKey = "PrevPrevDamage";
    private const decimal _baseDamage = 12m;

    // 斐波那契: F(1)=base, F(2)=base, F(n)=F(n-1)+F(n-2)
    // PrevDamage  = 上一次造成的伤害 F(n-1)
    // PrevPrevDamage = 上上次造成的伤害 F(n-2)

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(_baseDamage, ValueProp.Move),
        new DynamicVar(_prevDamageKey, 0m),
        new DynamicVar(_prevPrevDamageKey, 0m),
    };

    public ZhanCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Sword];


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Sword),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");


        int currentDamage = DynamicVars.Damage.IntValue;
        int oldPrev = DynamicVars[_prevDamageKey].IntValue;

        await DamageCmd.Attack(currentDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await ToolCmd.SwordStance(choiceContext, Owner, this);

        // 斐波那契: 推移前两次伤害记录
        // newPrev = 本次伤害, newPrevPrev = 上次伤害(oldPrev)
        int newPrev = currentDamage;
        int newPrevPrev = oldPrev;

        // 计算下次伤害
        // 如果 newPrevPrev == 0，说明只打出过0-1次，下次仍为基础伤害
        int nextDamage = (newPrevPrev == 0)
            ? (int)_baseDamage
            : newPrev + newPrevPrev;

        DynamicVars[_prevDamageKey].BaseValue = newPrev;
        DynamicVars[_prevPrevDamageKey].BaseValue = newPrevPrev;
        DynamicVars.Damage.BaseValue = nextDamage;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
