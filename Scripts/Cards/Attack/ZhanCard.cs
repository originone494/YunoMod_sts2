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

// 斩：斐波那契伤害——F(1)=F(2)=基础伤害，F(n)=F(n-1)+F(n-2)；本场战斗内累计，战斗结束重置
public class ZhanCard : YunoBaseCard
{
    private const string _prevDamageKey = "PrevDamage";
    private const string _prevPrevDamageKey = "PrevPrevDamage";
    private const decimal _baseDamage = 12m;
    private const decimal _upgradeDamage = 3m;

    // PrevDamage  = 上一次打出的伤害 F(n-1)
    // PrevPrevDamage = 上上次打出的伤害 F(n-2)

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(_baseDamage, ValueProp.Move),
        new DynamicVar(_prevDamageKey, 0m),
        new DynamicVar(_prevPrevDamageKey, 0m),
    };

    public ZhanCard() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.Sword];


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Sword),
        HoverTipFactory.FromKeyword(YunoKeywords.Stance),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");


        int currentDamage = DynamicVars.Damage.IntValue;
        int oldPrev = DynamicVars[_prevDamageKey].IntValue;

        await DamageCmd.Attack(currentDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await ToolCmd.SwordStance(choiceContext, Owner, this);

        // 斐波那契推移：newPrev = 本次伤害，newPrevPrev = 上一次伤害
        int newPrev = currentDamage;
        int newPrevPrev = oldPrev;

        // 计算下次伤害：F(2) 与 F(1) 相同（用本次实际伤害，升级加成自然保留）；
        // 此后 F(n+1) = F(n) + F(n-1)
        int nextDamage = (newPrevPrev == 0)
            ? currentDamage
            : newPrev + newPrevPrev;

        DynamicVars[_prevDamageKey].BaseValue = newPrev;
        DynamicVars[_prevPrevDamageKey].BaseValue = newPrevPrev;
        DynamicVars.Damage.BaseValue = nextDamage;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(_upgradeDamage);
    }

    // 战斗结束重置斐波那契状态，基础伤害按升级状态恢复
    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        DynamicVars.Damage.BaseValue = _baseDamage + (IsUpgraded ? _upgradeDamage : 0m);
        DynamicVars[_prevDamageKey].BaseValue = 0m;
        DynamicVars[_prevPrevDamageKey].BaseValue = 0m;
        return Task.CompletedTask;
    }
}
