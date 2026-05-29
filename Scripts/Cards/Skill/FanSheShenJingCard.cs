using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Cards.Attack;
using YunoMod.Scripts.Cards.Other;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Cards.Skill;

public class FanSheShenJingCard : YunoBaseCard, IOnLingHuo
{

    private const string _playCard = "PlayCard";

    private const string _LingHuoCard = "LingHuoCard";

    public FanSheShenJingCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.LingHuo];


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar(_playCard,1),
        new DynamicVar(_LingHuoCard,2)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.LingHuo),

    ];

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await CardPileCmd.Draw(ctx, DynamicVars[_LingHuoCard].IntValue, Owner);


    }

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars[_playCard].IntValue, Owner);

    }

    protected override void OnUpgrade()
    {
        DynamicVars[_playCard].UpgradeValueBy(1);
        DynamicVars[_LingHuoCard].UpgradeValueBy(1);
    }
}
