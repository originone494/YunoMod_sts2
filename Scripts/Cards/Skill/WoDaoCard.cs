using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Hook;
using MegaCrit.Sts2.Core.HoverTips;

namespace YunoMod.Scripts.Cards.Skill;

public class WoDaoCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(3m, ValueProp.Move),
    };


    public WoDaoCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int discardCount = PileType.Hand.GetPile(base.Owner).Cards.Count;
        await CardCmd.Discard(choiceContext, PileType.Hand.GetPile(base.Owner).Cards);
        if (discardCount > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue * discardCount, ValueProp.Move, cardPlay);
        }

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }


}
