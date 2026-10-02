using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using YunoMod.Scripts.Hook;
using YunoMod.Scripts.Pool;

namespace YunoMod.Scripts.Base;

[RegisterCard(typeof(YunoCardPool), Inherit = true)]
public abstract class YunoBaseCard(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary = true) : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{

    // 原版靠 CardModel.GainsBlock 判断一张牌能否被「格挡类附魔」影响：
    // 灵活(Nimble) 的 CanEnchant 直接读这个属性，而它默认 false、需要逐卡声明
    // （原版 80 张格挡牌都手写了 public override bool GainsBlock => true;）。
    // 菲涅尔透镜正是靠给卡牌奖励附上灵活生效，因此漏声明会导致它对整套 mod 卡完全失效。
    // 这里改为按 CanonicalVars 是否声明了 BlockVar 自动判定，避免每新增一张格挡牌就漏一次。
    public override bool GainsBlock => base.GainsBlock || CanonicalVars.Any(v => v is BlockVar);

    // 灵活：本卡因效果被送入弃牌堆时触发。
    // 用原版唯一的弃牌事件（CardCmd.DiscardAndDraw 内部、入堆之后），
    // 因此回合结束清空手牌、正常/自动打出后的落堆都不会触发。
    public override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        await base.AfterCardDiscarded(choiceContext, card);
        if (card != this) return;
        await LingHuoHook.OnCardDiscarded(choiceContext, card);
    }

    // 登场：本卡因效果加入手牌时触发（抽牌不算，靠 DrawWindowPatch 的抽牌窗口区分）。
    // 这个钩子没有 PlayerChoiceContext，需要上下文的地方由 DengChangHook 自己造。
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        await base.AfterCardChangedPiles(card, oldPileType, clonedBy);
        if (card != this) return;
        await DengChangHook.OnCardAddedToHand(card);
    }

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://YunoMod/images/cards/{GetType().Name}.png"
    // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。

    // FramePath: type switch
    //{
    //  CardType.Attack => "res://RitsuTest/images/card_frame_attack.png",
    //CardType.Skill => "res://RitsuTest/images/card_frame_skill.png",
    //CardType.Power => "res://RitsuTest/images/card_frame_power.png",
    //_ => ""
    //}

    // 如果使用自定义卡池，需要改下material（TODO）
    // FramePath: "", // 卡牌背景
    // PortraitBorderPath: "", // 边框（状态牌感染使用的）
    // BannerTexturePath: "" // 横幅（不同类型）
    );
}