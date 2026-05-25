using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Skill;

public class ChaoFuHeCard : YunoBaseCard, IOnLingHuo
{

    private const string _discardCount = "DiscardCount";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(3),
        new DynamicVar(_discardCount,3)
    };

    public ChaoFuHeCard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.LingHuo];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        // 先抽牌
        int drawCount = (int)DynamicVars.Cards.BaseValue;
        await CardPileCmd.Draw(choiceContext, drawCount, Owner);


        List<CardModel> selectedCards = (await CardSelectCmd.FromHandForDiscard(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1, 1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this
        )).ToList();

        if (selectedCards.Count > 0)
        {
            await CardCmd.Discard(choiceContext, selectedCards);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars[_discardCount].UpgradeValueBy(1);
    }

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.DuiMu(ctx, player, (int)DynamicVars[_discardCount].BaseValue);

        await CardPileCmd.Draw(ctx, 1, Owner);
    }

}
