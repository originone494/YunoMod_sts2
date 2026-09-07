using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

public class ZhongYanDeDaoJiShiCard : YunoSpecialBaseCard
{
    public ZhongYanDeDaoJiShiCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<ZhongYanDeDaoJiShiPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 失去2点生命（不可格挡、不受力量加成）
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 2, ValueProp.Unpowered | ValueProp.Unblockable, Owner.Creature, this, cardPlay);

        // 施加倒计时计数器
        await PowerCmd.Apply<ZhongYanDeDaoJiShiPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
