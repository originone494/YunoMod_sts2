using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;

namespace YunoMod.Scripts.Relics;

// 正位03-女皇：根据职业获得不同效果（原版遗物 ScrollBoxes 同款 is 分支写法）
// 逆位：正位的效果暂不生效（获得时效果早于逆位掷骰，无法追溯；战斗内表现为图标/文本切换）
public class Tarot03TheEmpressRelic : TarotRelicBase
{
    private const decimal _necrobinderMaxHp = 13m;
    private const decimal _otherStrength = 1m;
    private const decimal _otherDexterity = 1m;

    protected override bool SupportsReversed => true;

    public override RelicRarity Rarity => RelicRarity.Common;

    // 拾取时：按职业发放卡牌/生命上限
    public override async Task AfterObtained()
    {
        var addResults = new List<CardPileAddResult>();

        if (Owner.Character is Ironclad)
        {
            addResults.Add(await AddToDeck<Rampage>());
        }
        else if (Owner.Character is Silent)
        {
            addResults.Add(await AddToDeck<Snakebite>());
        }
        else if (Owner.Character is Regent)
        {
            var armaments = Owner.RunState.CreateCard<Armaments>(Owner);
            CardCmd.Upgrade(armaments);
            addResults.Add(await CardPileCmd.Add(armaments, PileType.Deck));
        }
        else if (Owner.Character is Necrobinder)
        {
            Flash();
            await CreatureCmd.GainMaxHp(Owner.Creature, _necrobinderMaxHp);
        }
        else if (Owner.Character is Defect)
        {
            addResults.Add(await AddToDeck<Claw>());
        }
        // 其他职业（含由乃）：没有拾取时效果，力量/敏捷在战斗开始时获得

        if (addResults.Count > 0)
        {
            CardCmd.PreviewCardPileAdd(addResults, 2f);
        }
    }

    // 每场战斗开始：非原版五职业（其他）获得1点力量和1点敏捷（逆位时关闭）
    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (IsReversed) return;
        if (side != base.Owner.Creature.Side || combatState.RoundNumber > 1) return;
        if (Owner.Character is Ironclad or Silent or Regent or Necrobinder or Defect) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, _otherStrength, Owner.Creature, null);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, _otherDexterity, Owner.Creature, null);
    }

    // 暴走免费打出：仅战士的暴走费用为0（逆位关闭）
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (IsReversed) return false;
        if (card is Rampage && card.Owner == Owner && Owner.Character is Ironclad)
        {
            modifiedCost = 0m;
            return true;
        }
        return false;
    }

    // 蛇咬对全体敌人生效：仅猎人打出蛇咬后，对原目标以外的敌人施加同等毒层数（逆位关闭）
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsReversed) return;
        if (cardPlay.Card is not Snakebite || cardPlay.Card.Owner != Owner || Owner.Character is not Silent) return;
        if (Owner.Creature.CombatState == null) return;

        var poisonVar = cardPlay.Card.DynamicVars["PoisonPower"];
        if (poisonVar == null || poisonVar.BaseValue <= 0) return;

        var otherEnemies = Owner.Creature.CombatState.HittableEnemies
            .Where(e => e != cardPlay.Target)
            .ToList();
        foreach (var enemy in otherEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, poisonVar.BaseValue, Owner.Creature, null);
        }
    }

    // 打出爪击后，获得等同该伤害的格挡：仅机器人（按实际结算伤害，与血溅卡同款算法；逆位关闭）
    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (IsReversed) return;
        if (command.Attacker != Owner.Creature) return;
        if (command.CardPlay?.Card is not Claw) return;
        if (Owner.Character is not Defect) return;

        decimal totalDamage = command.Results
            .SelectMany(r => r)
            .Sum(r => r.TotalDamage + r.OverkillDamage);
        if (totalDamage <= 0) return;

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, totalDamage, ValueProp.Unpowered, null);
    }

    private async Task<CardPileAddResult> AddToDeck<T>() where T : CardModel
    {
        return await CardPileCmd.Add(Owner.RunState.CreateCard<T>(Owner), PileType.Deck);
    }
}
