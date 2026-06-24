using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts;

[RegisterOwnedCardTag(nameof(YaZhi))]
public class YunoTags
{
    public static readonly CardTag YaZhi = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YaZhi)).GetModCardTag();
}