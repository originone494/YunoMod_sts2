using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using MegaCrit.Sts2.Core.HoverTips;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Keywords;

namespace YunoMod.Scripts.Cards.Skill;

public class XiangXinWoCard : YunoBaseCard, IOnLingHuo
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
   {
        new BlockVar(14m, ValueProp.Move),
        new DynamicVar("LingHuoBlock", 8m),
   };


    public override IEnumerable<CardKeyword> CanonicalKeywords => [YunoKeywords.LingHuo];

    public XiangXinWoCard() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LingHuoBlock"].UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(2);
    }

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["LingHuoBlock"].BaseValue, ValueProp.Move, null);
    }
}
