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

// 异解·福音：每回合可以免费打出1张「异解怪兽」；回合开始时，向消耗堆加入5张随机卡，之后，可以选择消耗堆的1张「异解怪兽」打出
[RegisterPower]
public class YiJieFuYinPower : YunoBasePower
{
    private const string _playPromptKey = "YUNO_MOD_POWER_YI_JIE_FU_YIN_POWER.playPrompt";
    private const int _randomCards = 5;

    private bool _freePlayUsedThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 每回合第一张打出的「异解怪兽」免费（自动打出的不消耗配额）
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (_freePlayUsedThisTurn || originalCost <= 0m) return false;
        if (card.Owner != Owner.Player!) return false;
        if (!card.Tags.Contains(YunoTags.YiJieGuaiShou)) return false;

        modifiedCost = 0m;
        return true;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.IsAutoPlay) return Task.CompletedTask;   // 自动打出（如回合开始的打出）不占用免费配额
        if (cardPlay.Card?.Owner != Owner.Player!) return Task.CompletedTask;
        if (!cardPlay.Card.Tags.Contains(YunoTags.YiJieGuaiShou)) return Task.CompletedTask;

        _freePlayUsedThisTurn = true;
        return Task.CompletedTask;
    }

    // 回合开始：灌注消耗堆，之后可以选择1张「异解怪兽」打出
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var self = Owner.Player!;
        if (player != self) return;
        _freePlayUsedThisTurn = false;

        // 1. 向消耗堆加入5张随机卡（全卡池完全随机，各抽各的）
        var copies = new List<CardModel>();
        for (int i = 0; i < _randomCards; i++)
        {
            var randomCanonical = self.RunState.Rng.Niche.NextItem(ModelDb.AllCards)!;
            copies.Add(Owner.CombatState!.CreateCard(randomCanonical!, self));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(copies, PileType.Exhaust, self);

        // 2. 可以选择消耗堆的1张「异解怪兽」打出（0-1张，可取消）
        var exhaustOptions = PileType.Exhaust.GetPile(self).Cards
            .Where(c => c.Tags.Contains(YunoTags.YiJieGuaiShou)).ToList();
        if (exhaustOptions.Count == 0) return;

        var prefs = new CardSelectorPrefs(new LocString("powers", _playPromptKey), 0, 1);
        var picked = (await CardSelectCmd.FromCombatPile(choiceContext,
            PileType.Exhaust.GetPile(self), self, prefs,
            filter: c => c.Tags.Contains(YunoTags.YiJieGuaiShou))).FirstOrDefault();

        if (picked != null)
        {
            Flash();
            await CardCmd.AutoPlay(choiceContext, picked, null);
        }
    }
}
