using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;

namespace YunoMod.Scripts.Power;

[RegisterPower]
public class DiaryPower : YunoBasePower
{

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;

        int round = CombatState.RoundNumber;
        int amount = Amount;

        if (round == 1)
        {
            if (amount >= 1)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner, amount, ValueProp.Unpowered, null);
            }
            if (amount >= 7)
            {
                Flash();
                await CardPileCmd.Draw(choiceContext, 2, Owner.Player!);
            }
        }

        if (amount >= 5)
        {
            Flash();
            await PowerCmd.Apply<PoisonPower>(choiceContext,Owner.CombatState!.HittableEnemies, 1, Owner, null);
        }

        if (round == 2 && amount >= 9)
        {
            Flash();
            var resultList = new List<CardPileAddResult>();

            var card = Owner.CombatState!.CreateCard<ZhiShiYuanBoCard>(Owner.Player!);
            var addResult = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
            resultList.Add(addResult);
            CardCmd.PreviewCardPileAdd(resultList, 2f);

        }

        if (round == 3 && amount >= 10)
        {
            Flash();
            var resultList = new List<CardPileAddResult>();
            var card = Owner.CombatState!.CreateCard<WanQianLunHuiCard>(Owner.Player!);
            var addResult = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
            resultList.Add(addResult);
            CardCmd.PreviewCardPileAdd(resultList, 2f);

        }
    }

    // 持有 12 个日记：每次战斗胜利后，获得 1 个随机遗物（放入战斗结束的奖励界面，可拿取/跳过）。
    // 该钩子在所有玩家的 AfterCombatEnd 清空能力之前触发，DiaryPower 与层数仍有效；
    // 奖励只会在胜利后被引擎发放（失败局不会进入奖励界面）。
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Amount >= 12 && Owner.Player != null)
        {
            room.AddExtraReward(Owner.Player, new RelicReward(Owner.Player));
        }
        await Task.CompletedTask;
    }

    public async override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && amount > 0 && applier == Owner)
        {
            if (Amount == 2)
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(choiceContext,Owner, 1, Owner, null);
            }
            if (Amount == 3)
            {
                Flash();
                await PowerCmd.Apply<DexterityPower>(choiceContext,Owner, 1, Owner, null);
            }
        }

    }

}
