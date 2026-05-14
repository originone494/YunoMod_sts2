using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using YunoMod.Scripts;

namespace YunoMod.Scripts;

[RegisterOwnedCardKeyword(nameof(Dagger), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Axe), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Gun), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(Sword), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(LiuXuePower), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(ZhiCanPower), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword(nameof(LingHuo), IconPath = "res://icon.svg", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class YunoKeywords
{
    public static readonly string Dagger = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Dagger));
    public static readonly string Axe = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Axe));
    public static readonly string Gun = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Gun));
    public static readonly string Sword = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Sword));

    public static readonly string LiuXuePower = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(LiuXuePower));
    public static readonly string ZhiCanPower = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhiCanPower));

    public static readonly string LingHuo = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(LingHuo));

}