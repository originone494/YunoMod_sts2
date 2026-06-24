using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using YunoMod.Scripts.Tool;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Entities.Players;

namespace YunoMod.Scripts.Cards.Skill;

public class LianXiCard : YunoBaseCard, IOnLingHuo
{

    private const string _playLoveCount = "PlayLoveCount";


    public LianXiCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {

    }


    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<LovePower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<LovePower>(7),
        new DynamicVar(_playLoveCount,4)

    ];


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.LingHuo];

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await ToolCmd.GainLovePower(ctx, Owner, this, DynamicVars["LovePower"].IntValue);
    }

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await ToolCmd.GainLovePower(choiceContext, Owner, this, DynamicVars[_playLoveCount].IntValue);

    }

    protected override void OnUpgrade()
    {
        DynamicVars["LovePower"].UpgradeValueBy(2);
    }
}
