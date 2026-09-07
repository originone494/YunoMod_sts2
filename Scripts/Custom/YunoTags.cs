using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts;

[RegisterOwnedCardTag(nameof(YaZhi))]
[RegisterOwnedCardTag(nameof(ZhuLei))]
[RegisterOwnedCardTag(nameof(ZhuLeiMoXian))]
[RegisterOwnedCardTag(nameof(ZhuLeiGuaiShou))]
[RegisterOwnedCardTag(nameof(ZhuLeiRongHe))]
[RegisterOwnedCardTag(nameof(ZhuChang))]
[RegisterOwnedCardTag(nameof(LingHuo))]
[RegisterOwnedCardTag(nameof(JuShe))]
[RegisterOwnedCardTag(nameof(Diary))]
[RegisterOwnedCardTag(nameof(HeLuSiGuaiShou))]
[RegisterOwnedCardTag(nameof(YiJie))]
[RegisterOwnedCardTag(nameof(YiJieGuaiShou))]
[RegisterOwnedCardTag(nameof(YiJieMoXian))]
public class YunoTags
{
    public static readonly CardTag YaZhi = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YaZhi)).GetModCardTag();
    public static readonly CardTag ZhuLei = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLei)).GetModCardTag();
    public static readonly CardTag ZhuLeiMoXian = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiMoXian)).GetModCardTag();
    public static readonly CardTag ZhuLeiGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiGuaiShou)).GetModCardTag();
    public static readonly CardTag ZhuLeiRongHe = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiRongHe)).GetModCardTag();
    public static readonly CardTag ZhuChang = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuChang)).GetModCardTag();
    public static readonly CardTag LingHuo = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(LingHuo)).GetModCardTag();
    public static readonly CardTag JuShe = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(JuShe)).GetModCardTag();
    public static readonly CardTag Diary = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Diary)).GetModCardTag();
    public static readonly CardTag HeLuSiGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(HeLuSiGuaiShou)).GetModCardTag();
    public static readonly CardTag YiJie = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YiJie)).GetModCardTag();
    public static readonly CardTag YiJieGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YiJieGuaiShou)).GetModCardTag();
    public static readonly CardTag YiJieMoXian = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YiJieMoXian)).GetModCardTag();

}
