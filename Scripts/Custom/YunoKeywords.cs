using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts;

[RegisterOwnedCardKeyword(nameof(Dagger), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Axe), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Gun), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Sword), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]


[RegisterOwnedCardKeyword(nameof(LingHuo), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

// 「应对」：不进卡牌的关键字列表（所以卡面不会被注入"应对。"那一行），只在悬停提示里展示。
// 卡面靠"应对：xxx"这段纯文本 + YunoTags.YingDui 标签承担。
[RegisterOwnedCardKeyword(nameof(YingDui), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Foresee), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

// 「登场」：因效果加入手牌时触发（机制见 DengChangHook）
[RegisterOwnedCardKeyword(nameof(DengChang), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Stance), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Retriever), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]


[RegisterOwnedCardKeyword(nameof(YaZhi), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(ZhuLei), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiMoFa), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiXianJing), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiXiaJiGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiRongHeGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiRongHe), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(JuShe), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Diary), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(HeLuSiGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(YiJie), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(YiJieGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(YiJieMoXian), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(TianBeiLong), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(CanHuanMoFa), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(CanHuanXianJing), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(CanHuanGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(LongZu), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(TongDiao), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(TiaoZheng), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(SanXing), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(SiXing), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(QiXing), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ShiXing), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(TongBu), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

public class YunoKeywords
{
    public static readonly CardKeyword Dagger = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Dagger)).GetModCardKeyword();

    public static readonly CardKeyword Axe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Axe)).GetModCardKeyword();
    public static readonly CardKeyword Gun = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Gun)).GetModCardKeyword();
    public static readonly CardKeyword Sword = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Sword)).GetModCardKeyword();
    public static readonly CardKeyword LingHuo = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(LingHuo)).GetModCardKeyword();

    // 「应对」：只用于悬停说明（小美 / 天下独步的大义贼），不挂进卡的 CanonicalKeywords
    public static readonly CardKeyword YingDui = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YingDui)).GetModCardKeyword();

    public static readonly CardKeyword Foresee = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Foresee)).GetModCardKeyword();

    // 「登场」：因效果加入手牌时，将其打出并返回手牌
    public static readonly CardKeyword DengChang = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(DengChang)).GetModCardKeyword();

    public static readonly CardKeyword Stance = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Stance)).GetModCardKeyword();

    public static readonly CardKeyword Retriever = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Retriever)).GetModCardKeyword();

    public static readonly CardKeyword YaZhi = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YaZhi)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLei = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLei)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLeiMoFa = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiMoFa)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLeiXianJing = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiXianJing)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLeiGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiGuaiShou)).GetModCardKeyword();

    // 「珠泪下级怪兽」：珠泪怪兽里非融合的那部分（只用于悬停说明，不挂进 CanonicalKeywords）
    public static readonly CardKeyword ZhuLeiXiaJiGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiXiaJiGuaiShou)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLeiRongHeGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiRongHeGuaiShou)).GetModCardKeyword();
    public static readonly CardKeyword ZhuLeiRongHe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiRongHe)).GetModCardKeyword();

    public static readonly CardKeyword JuShe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(JuShe)).GetModCardKeyword();

    public static readonly CardKeyword Diary = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Diary)).GetModCardKeyword();

    public static readonly CardKeyword HeLuSiGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(HeLuSiGuaiShou)).GetModCardKeyword();

    public static readonly CardKeyword YiJie = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YiJie)).GetModCardKeyword();

    public static readonly CardKeyword YiJieGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YiJieGuaiShou)).GetModCardKeyword();

    public static readonly CardKeyword YiJieMoXian = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YiJieMoXian)).GetModCardKeyword();

    // —— 天杯龙系列 ——
    // 「天杯龙」：系列总类，包含天杯龙怪兽、灿幻魔法、灿幻陷阱
    public static readonly CardKeyword TianBeiLong = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(TianBeiLong)).GetModCardKeyword();

    public static readonly CardKeyword CanHuanMoFa = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(CanHuanMoFa)).GetModCardKeyword();

    public static readonly CardKeyword CanHuanXianJing = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(CanHuanXianJing)).GetModCardKeyword();

    public static readonly CardKeyword CanHuanGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(CanHuanGuaiShou)).GetModCardKeyword();

    // 「龙族」：龙族的怪兽卡
    public static readonly CardKeyword LongZu = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(LongZu)).GetModCardKeyword();

    // 「同调」：同调召唤相关（效果文本待补，悬停占位）
    public static readonly CardKeyword TongDiao = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(TongDiao)).GetModCardKeyword();

    // 「调整」：调整怪兽（效果文本待补，悬停占位）
    public static readonly CardKeyword TiaoZheng = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(TiaoZheng)).GetModCardKeyword();

    // 「三星 / 四星 / 七星 / 十星」：星级标记（效果文本待补，悬停占位）
    public static readonly CardKeyword SanXing = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(SanXing)).GetModCardKeyword();

    public static readonly CardKeyword SiXing = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(SiXing)).GetModCardKeyword();

    public static readonly CardKeyword QiXing = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(QiXing)).GetModCardKeyword();

    public static readonly CardKeyword ShiXing = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ShiXing)).GetModCardKeyword();

    // 「同步」：效果文本待补（悬停占位）
    public static readonly CardKeyword TongBu = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(TongBu)).GetModCardKeyword();

}
