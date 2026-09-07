using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Cards.Special;

public class ZhuLeiLuShaKaCard : YunoSpecialBaseCard, IOnLingHuo
{
    private bool _lingHuoUsedThisTurn;

    public ZhuLeiLuShaKaCard() : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    // 伤害使用动态变量：造成 30 点伤害（打出与驻场共用）
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(30m, ValueProp.Move),
    };

    protected override HashSet<CardTag> CanonicalTags => [
        YunoTags.ZhuLei,
        YunoTags.ZhuLeiGuaiShou,
        YunoTags.LingHuo,
        YunoTags.ZhuLeiRongHe,
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain,
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLei),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiGuaiShou),
        HoverTipFactory.FromKeyword(YunoKeywords.ZhuLeiRongHe),
    ];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 造成30点伤害，给予1层虚弱，获得1层人工制品
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.Damage(choiceContext, cardPlay.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, cardPlay);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    // 驻场：回合结束时，对随机敌人造成30点伤害
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (CombatState == null) return;
        if (!PileType.Hand.GetPile(Owner).Cards.Contains(this)) return;

        Creature? creature = Owner!.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (creature == null) return;

        await CreatureCmd.Damage(choiceContext, creature, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this, null);
        await PowerCmd.Apply<WeakPower>(choiceContext, creature, 1, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);

    }

    // 灵活：一回合一次，将这张卡打出，然后加入手牌
    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        if (_lingHuoUsedThisTurn) return;
        _lingHuoUsedThisTurn = true;

        // 打出（随机敌人）
        Creature? creature = Owner!.RunState.Rng.CombatTargets.NextItem(Owner.Creature.CombatState!.HittableEnemies);
        if (creature != null)
        {
            await CardCmd.AutoPlay(ctx, this, creature);
        }

        // 加入手牌
        await CardPileCmd.Add(this, PileType.Hand);

        LingHuoHook.HandledByLingHuo.Add(this);

    }

    // 回合开始时重置"一回合一次"
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        _lingHuoUsedThisTurn = false;
        await Task.CompletedTask;
    }
}
