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
using YunoMod.Scripts.Tool;

namespace YunoMod.Scripts.Cards.Attack;

public class LianSheCard : YunoBaseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(2m, ValueProp.Move),
        new RepeatVar(4),
    };


    public override IEnumerable<CardKeyword> CanonicalKeywords => [ YunoKeywords.Gun];
    public LianSheCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.Gun),
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {


        await ToolCmd.GunAttackRandomEnemy(choiceContext, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue);

        await ToolCmd.GunStance(choiceContext, Owner, this);

		await Cmd.Wait(0.25f);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType resultPileTypeForCardPlay = base.GetResultPileTypeForCardPlay();
        if (resultPileTypeForCardPlay != PileType.Discard)
        {
            return resultPileTypeForCardPlay;
        }
        return PileType.Hand;
    }
}
