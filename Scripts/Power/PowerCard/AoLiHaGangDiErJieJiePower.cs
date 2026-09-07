using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;

namespace YunoMod.Scripts.Power;

// 奥利哈刚第二结界：
// - 回合开始时恢复5点生命
// - 回合结束时，选择1张手牌获得永久「保留」
// - 若拥有「奥利哈刚的结界」，敌人攻击时随机丢弃1张手牌，使那次攻击伤害为0
[RegisterPower]
public class AoLiHaGangDiErJieJiePower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 回合开始时，恢复5点生命
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;
        Flash();
        await CreatureCmd.Heal(Owner, 5m);
    }

    // 回合结束时，选择1张手牌获得永久「保留」
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        Flash();

        var handCards = PileType.Hand.GetPile(Owner.Player!).Cards.ToList();
        if (handCards.Count == 0) return;

        var selected = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(YunoSelectorPrefs.RetainSelectionPrompt, 1, 1),
            context: choiceContext,
            player: Owner.Player!,
            filter: null,
            source: this)).ToList();
        if (selected.Count == 0) return;

        // 永久获得「保留」关键字（不是临时保留一回合）
        selected[0].AddKeyword(CardKeyword.Retain);
    }

    // 若拥有「奥利哈刚的结界」，敌人攻击时使那次攻击伤害为0
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target == Owner
            && dealer != null && dealer.IsMonster
            && Owner.HasPower<AoLiHaGangDeJieJiePower>()
            && Owner.Player!.PlayerCombatState!.Hand.Cards.Count > 0)
        {
            return 0m;
        }
        return 1m;
    }

    // 敌人攻击时，随机丢弃1张手牌（配合上面的伤害归零）
    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return;
        if (dealer == null || !dealer.IsMonster) return;
        if (!Owner.HasPower<AoLiHaGangDeJieJiePower>()) return;

        var handCards = Owner.Player!.PlayerCombatState!.Hand.Cards.ToList();
        if (handCards.Count == 0) return;

        var toDiscard = Owner.Player.RunState.Rng.Niche.NextItem(handCards)!;
        await CardCmd.Discard(choiceContext, toDiscard);
    }
}
