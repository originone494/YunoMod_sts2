using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts;

[RegisterOwnedCardTag(nameof(Dagger))]
[RegisterOwnedCardTag(nameof(Axe))]
[RegisterOwnedCardTag(nameof(Gun))]
[RegisterOwnedCardTag(nameof(Sword))]
public class YunoTags
{
    public static readonly string Dagger = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Dagger));
    public static readonly string Axe = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Axe));
    public static readonly string Gun = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Gun));
    public static readonly string Sword = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Sword));
}