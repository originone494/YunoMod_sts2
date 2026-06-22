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
using STS2RitsuLib.Cards.DynamicVars;

namespace YunoMod.Scripts.Cards.Skill;

public class GuWangCard : YunoBaseCard
{
    private const string _VaryCount = "VaryCount";

    public GuWangCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];


    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
        new ComputedDynamicVar(_VaryCount, 0,_=>Owner.Creature.HasPower<DiaryPower>() ? Owner.Creature.GetPowerAmount<DiaryPower>() : 1)
        };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
        HoverTipFactory.FromCard<QiuTiCard>(),
        HoverTipFactory.FromPower<DiaryPower>(),
    ];



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        List<CardModel> list = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(SelectionScreenPrompt, 0, Owner.Creature.HasPower<DiaryPower>() ? Owner.Creature.GetPowerAmount<DiaryPower>() : 1), context: choiceContext, player: base.Owner, filter: null, source: this)).ToList();

        if (list.Count() == 0) return;
        foreach (CardModel item in list)
        {
            CardModel cardModel = base.CombatState!.CreateCard<QiuTiCard>(base.Owner);

            await CardCmd.Transform(item, cardModel);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
