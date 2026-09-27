using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace YunoMod.Scripts.Cards.Skill;

public class YiYaHuanYaCard : YunoBaseCard
{
    public YiYaHuanYaCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(YunoKeywords.LingHuo),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<YiYaHuanYaPower>(1)
    };



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        // 打出消耗堆中所有「灵活」卡
        // AutoPlay 会自行处理目标选取（攻击牌随机敌人）、"不可打出"与 hook 拦截，
        // 并把卡从消耗堆移入 Play 堆再结算到它自己的目标牌堆
        var lingHuoCards = PileType.Exhaust.GetPile(Owner).Cards
            .Where(c => c.Tags.Contains(YunoTags.LingHuo) || c.Keywords.Contains(YunoKeywords.LingHuo))
            .ToList();

        foreach (var card in lingHuoCards)
        {
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
