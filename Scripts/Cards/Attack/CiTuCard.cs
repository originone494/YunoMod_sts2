using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Tool;

using MegaCrit.Sts2.Core.HoverTips;
using STS2RitsuLib.Keywords;
namespace YunoMod.Scripts.Cards.Attack;

public class CiTuCard : YunoBaseCard
{

    private const string _DuiMuCount = "DuiMuCount";
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new RepeatVar(1),
        new DynamicVar(_DuiMuCount,5)
    ];

    public CiTuCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {

    }

    protected override IEnumerable<string> RegisteredKeywordIds => [YunoKeywords.Dagger];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Dagger),
        ModKeywordRegistry.CreateHoverTip(YunoKeywords.Stance),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");


        IEnumerable<CardModel> list = await ToolCmd.DuiMu(choiceContext, Owner, DynamicVars[_DuiMuCount].IntValue);

        int amount = 0;

        foreach (CardModel card in list)
        {
            if (card.Type == CardType.Attack)
            {
                amount++;
                break;
            }
        }

        if (amount > 0)
            await ToolCmd.DaggerAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue, DynamicVars.Repeat.IntValue + 1);
        else

            await ToolCmd.DaggerAttack(choiceContext, cardPlay.Target, this, DynamicVars.Damage.BaseValue);

        await ToolCmd.DaggerStance(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
