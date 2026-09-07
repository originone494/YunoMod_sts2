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

[RegisterOwnedCardKeyword(nameof(Foresee), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Stance), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Retriever), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]


[RegisterOwnedCardKeyword(nameof(YaZhi), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuChang), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLei), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiMoXian), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(ZhuLeiRongHe), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(JuShe), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Diary), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(HeLuSiGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(YiJie), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(YiJieGuaiShou), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(YiJieMoXian), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

public class YunoKeywords
{
    public static readonly CardKeyword Dagger = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Dagger)).GetModCardKeyword();

    public static readonly CardKeyword Axe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Axe)).GetModCardKeyword();
    public static readonly CardKeyword Gun = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Gun)).GetModCardKeyword();
    public static readonly CardKeyword Sword = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Sword)).GetModCardKeyword();
    public static readonly CardKeyword LingHuo = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(LingHuo)).GetModCardKeyword();

    public static readonly CardKeyword Foresee = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Foresee)).GetModCardKeyword();

    public static readonly CardKeyword Stance = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Stance)).GetModCardKeyword();

    public static readonly CardKeyword Retriever = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Retriever)).GetModCardKeyword();

    public static readonly CardKeyword YaZhi = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YaZhi)).GetModCardKeyword();

    public static readonly CardKeyword ZhuChang = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuChang)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLei = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLei)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLeiMoXian = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiMoXian)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLeiGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiGuaiShou)).GetModCardKeyword();

    public static readonly CardKeyword ZhuLeiRongHe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuLeiRongHe)).GetModCardKeyword();

    public static readonly CardKeyword JuShe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(JuShe)).GetModCardKeyword();

    public static readonly CardKeyword Diary = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Diary)).GetModCardKeyword();

    public static readonly CardKeyword HeLuSiGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(HeLuSiGuaiShou)).GetModCardKeyword();

    public static readonly CardKeyword YiJie = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YiJie)).GetModCardKeyword();

    public static readonly CardKeyword YiJieGuaiShou = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YiJieGuaiShou)).GetModCardKeyword();

    public static readonly CardKeyword YiJieMoXian = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(YiJieMoXian)).GetModCardKeyword();

}
