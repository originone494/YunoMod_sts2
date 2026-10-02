using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using YunoMod.Scripts;

[RegisterOwnedCardTag(nameof(YaZhi))]
[RegisterOwnedCardTag(nameof(ZhuLei))]
[RegisterOwnedCardTag(nameof(ZhuLeiMoFa))]
[RegisterOwnedCardTag(nameof(ZhuLeiXianJing))]
[RegisterOwnedCardTag(nameof(ZhuLeiGuaiShou))]
[RegisterOwnedCardTag(nameof(ZhuLeiXiaJiGuaiShou))]
[RegisterOwnedCardTag(nameof(ZhuLeiRongHeGuaiShou))]
[RegisterOwnedCardTag(nameof(ZhuLeiRongHe))]
[RegisterOwnedCardTag(nameof(LingHuo))]
[RegisterOwnedCardTag(nameof(YingDui))]
[RegisterOwnedCardTag(nameof(DengChang))]
[RegisterOwnedCardTag(nameof(JuShe))]
[RegisterOwnedCardTag(nameof(Diary))]
[RegisterOwnedCardTag(nameof(HeLuSiGuaiShou))]
[RegisterOwnedCardTag(nameof(YiJie))]
[RegisterOwnedCardTag(nameof(YiJieGuaiShou))]
[RegisterOwnedCardTag(nameof(YiJieMoXian))]
[RegisterOwnedCardTag(nameof(TianBeiLong))]
[RegisterOwnedCardTag(nameof(CanHuanMoFa))]
[RegisterOwnedCardTag(nameof(CanHuanXianJing))]
[RegisterOwnedCardTag(nameof(CanHuanGuaiShou))]
[RegisterOwnedCardTag(nameof(LongZu))]
[RegisterOwnedCardTag(nameof(TongDiao))]
[RegisterOwnedCardTag(nameof(TiaoZheng))]
[RegisterOwnedCardTag(nameof(SanXing))]
[RegisterOwnedCardTag(nameof(SiXing))]
[RegisterOwnedCardTag(nameof(QiXing))]
[RegisterOwnedCardTag(nameof(ShiXing))]
[RegisterOwnedCardTag(nameof(TongBu))]
public class YunoTags
{
    public static readonly CardTag YaZhi = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YaZhi)).GetModCardTag();
    public static readonly CardTag ZhuLei = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLei)).GetModCardTag();
    public static readonly CardTag ZhuLeiMoFa = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiMoFa)).GetModCardTag();
    public static readonly CardTag ZhuLeiXianJing = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiXianJing)).GetModCardTag();
    public static readonly CardTag ZhuLeiGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiGuaiShou)).GetModCardTag();
    // 「珠泪下级怪兽」= 珠泪怪兽里非融合的那部分（融合素材/检索对象用的就是它）。
    // 层级：珠泪 ⊃ 珠泪怪兽（含融合） ⊃ 珠泪融合怪兽 ／ 珠泪下级怪兽
    public static readonly CardTag ZhuLeiXiaJiGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiXiaJiGuaiShou)).GetModCardTag();
    public static readonly CardTag ZhuLeiRongHeGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiRongHeGuaiShou)).GetModCardTag();
    public static readonly CardTag ZhuLeiRongHe = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhuLeiRongHe)).GetModCardTag();
    public static readonly CardTag LingHuo = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(LingHuo)).GetModCardTag();
    // 「应对」：受到敌人攻击前触发效果的卡（小美 / 天下独步的大义贼）。仅作代码标记，没有卡面文本。
    public static readonly CardTag YingDui = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YingDui)).GetModCardTag();
    // 「登场」：因效果加入手牌时触发效果的卡。和「应对」一样只作代码标记（卡面用纯文本写"登场：…"），
    // 关键字本体只放进悬停提示，不给卡面加多余的"登场。"行。
    public static readonly CardTag DengChang = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(DengChang)).GetModCardTag();
    public static readonly CardTag JuShe = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(JuShe)).GetModCardTag();
    public static readonly CardTag Diary = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Diary)).GetModCardTag();
    public static readonly CardTag HeLuSiGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(HeLuSiGuaiShou)).GetModCardTag();
    public static readonly CardTag YiJie = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YiJie)).GetModCardTag();
    public static readonly CardTag YiJieGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YiJieGuaiShou)).GetModCardTag();
    public static readonly CardTag YiJieMoXian = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YiJieMoXian)).GetModCardTag();

    // —— 天杯龙系列 ——
    // 层级：天杯龙 ⊃ 天杯龙怪兽 ／ 灿幻魔法 ／ 灿幻陷阱；龙族为种族标记
    public static readonly CardTag TianBeiLong = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(TianBeiLong)).GetModCardTag();
    public static readonly CardTag CanHuanMoFa = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(CanHuanMoFa)).GetModCardTag();
    public static readonly CardTag CanHuanXianJing = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(CanHuanXianJing)).GetModCardTag();
    public static readonly CardTag CanHuanGuaiShou = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(CanHuanGuaiShou)).GetModCardTag();
    public static readonly CardTag LongZu = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(LongZu)).GetModCardTag();
    // 「同调」「调整」「星级」：代码标记（悬停文本待补）
    public static readonly CardTag TongDiao = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(TongDiao)).GetModCardTag();
    public static readonly CardTag TiaoZheng = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(TiaoZheng)).GetModCardTag();
    public static readonly CardTag SanXing = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(SanXing)).GetModCardTag();
    public static readonly CardTag SiXing = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(SiXing)).GetModCardTag();
    public static readonly CardTag QiXing = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(QiXing)).GetModCardTag();
    public static readonly CardTag ShiXing = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ShiXing)).GetModCardTag();

    public static readonly CardTag TongBu = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(TongBu)).GetModCardTag();

}
