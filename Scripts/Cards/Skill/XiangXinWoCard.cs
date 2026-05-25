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

namespace YunoMod.Scripts.Cards.Skill;

public class XiangXinWoCard : YunoBaseCard, IOnLingHuo
{
    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.LingHuo];


    public XiangXinWoCard() : base(3, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ArtifactPower>(),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.HasPower<ArtifactPower>() && IsUpgraded)
        {
            await PowerCmd.Apply<BufferPower>(Owner.Creature, 1, Owner.Creature, this);

        }
        await PowerCmd.Apply<ArtifactPower>(Owner.Creature, 1, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    public Task OnLingHuo(PlayerChoiceContext ctx, Player player)
    {
        return Task.CompletedTask;
    }

    public async Task LingHuoSpecial(PlayerChoiceContext ctx, Player player)
    {
        await CardCmd.AutoPlay(ctx, this, player.Creature);

        await CardPileCmd.Draw(ctx, 1, Owner);
    }
}
