using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Custom;

namespace YunoMod.Scripts.Power;

// 异解·瓦尔波勒斯：回合开始时，向消耗堆加入5张随机卡，之后，可以从消耗堆将1张「异解」卡加入手牌
[RegisterPower]
public class YiJieWaErBoLeSiPower : YunoBasePower
{
    private const string _handPromptKey = "YUNO_MOD_POWER_YI_JIE_WA_ER_BO_LE_SI_POWER.handPrompt";
    private const int _randomCards = 5;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 回合开始：灌注消耗堆，之后可以将1张「异解」卡加入手牌
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var self = Owner.Player!;
        if (player != self) return;

        // 1. 向消耗堆加入5张随机卡（全卡池完全随机，各抽各的）
        var copies = new List<CardModel>();
        for (int i = 0; i < _randomCards; i++)
        {
            var randomCanonical = self.RunState.Rng.Niche.NextItem(ModelDb.AllCards)!;
            copies.Add(Owner.CombatState!.CreateCard(randomCanonical!, self));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Exhaust, self);

        // 2. 可以选择消耗堆的1张「异解」卡加入手牌（0-1张，可取消）
        var options = PileType.Exhaust.GetPile(self).Cards
            .Where(c => c.Tags.Contains(YunoTags.YiJie)).ToList();
        if (options.Count == 0) return;

        var prefs = new CardSelectorPrefs(new LocString("powers", _handPromptKey), 0, 1);
        var picked = (await CardSelectCmd.FromCombatPile(choiceContext,
            PileType.Exhaust.GetPile(self), self, prefs,
            filter: c => c.Tags.Contains(YunoTags.YiJie))).FirstOrDefault();

        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
    }
}
