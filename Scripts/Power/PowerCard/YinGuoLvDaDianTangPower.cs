using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Power;

public class YinGuoLvDaDianTangPower : YunoBasePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side) return;
        Flash();
        var resultList = new List<CardPileAddResult>();
        for (int i = 0; i < Amount; i++)
        {
            CardModel card = Owner.CombatState!.CreateCard<QiuTiCard>(Owner.Player!);
            var addResult = await CardPileCmd.Add(card, PileType.Draw);
            resultList.Add(addResult);
        }
        CardCmd.PreviewCardPileAdd(resultList, 2f);

    }

}
