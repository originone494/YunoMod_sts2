using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Cards.Special;

public class AoLiHaGangDeJieJieCard : YunoSpecialBaseCard
{
    public AoLiHaGangDeJieJieCard() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<BufferPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 失去所有增益
        foreach (var power in Owner.Creature.Powers.ToList())
        {
            if (power.Type == PowerType.Buff)
                await PowerCmd.Remove(power);
        }

        // 获得5点力量
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 5, Owner.Creature, this);

        // 施加结界（回合开始1层缓冲 + 敌人<=5伤害免疫）
        await PowerCmd.Apply<AoLiHaGangDeJieJiePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
}
