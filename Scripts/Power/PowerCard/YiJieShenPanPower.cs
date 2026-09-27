using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Scripts;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Special;

namespace YunoMod.Scripts.Power;

// 异解·审判：施加在玩家身上生效的能力。
// 效果1：洗牌时，从消耗堆取回1张「异解」卡加入手牌。
// 效果2：打出「异解」卡时，若抽牌堆数量为0，所有敌人在本回合失去99点力量。
[RegisterPower]
public class YiJieShenPanPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 效果1：洗牌时，从消耗堆取回1张「异解」卡加入手牌
    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player player)
    {
        if (!Owner.IsPlayer) return;
        if (player.Creature != Owner) return;

        var yiJieCards = PileType.Exhaust.GetPile(player).Cards
            .Where(c => c.Tags.Contains(YunoTags.YiJie))
            .ToList();
        if (yiJieCards.Count == 0) return;

        var card = player.RunState.Rng.Niche.NextItem(yiJieCards);
        if (card == null) return;

        await CardPileCmd.Add(card, PileType.Hand);
    }

    // 效果2：打出「异解」卡时，若抽牌堆数量为0，所有敌人在本回合失去99点力量
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!Owner.IsPlayer) return;
        if (cardPlay.Card == null) return;
        if (!cardPlay.Card.Tags.Contains(YunoTags.YiJie)) return;

        if (PileType.Draw.GetPile(Owner.Player!).Cards.Count != 0) return;

        foreach (var enemy in Owner.CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<YiJieShenPanTempDownPower>(choiceContext, enemy, 99m, Owner, null);
        }
    }
}

// 异解·审判的临时力量（负面）：敌人本回合失去力量，其回合结束自动恢复
[RegisterPower]
public class YiJieShenPanTempDownPower : YunoTempStrengthPower<YiJieShenPanCard>
{
    protected override bool IsPositive => false;
}