using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts;

namespace YunoMod.Scripts;

[RegisterOwnedCardKeyword(nameof(Dagger), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Axe), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Gun), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Sword), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]


[RegisterOwnedCardKeyword(nameof(LingHuo), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Foresee), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Stance), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

[RegisterOwnedCardKeyword(nameof(Retriever), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]


[RegisterOwnedCardKeyword(nameof(YaZhi), IconPath = "res://yuno.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]

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


}