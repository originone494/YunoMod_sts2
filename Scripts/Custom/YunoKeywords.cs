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

public class YunoKeywords
{
    public static readonly string Dagger = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Dagger));
    public static readonly string Axe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Axe));
    public static readonly string Gun = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Gun));
    public static readonly string Sword = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Sword));
    public static readonly string LingHuo = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(LingHuo));

    public static readonly string Foresee = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Foresee));

    public static readonly string Stance = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Stance));

    public static readonly string Retriever = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Retriever));

    

}