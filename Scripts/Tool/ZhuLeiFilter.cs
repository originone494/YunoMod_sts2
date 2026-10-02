using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Tool;

// 「珠泪」家族的口径集中在这里，避免每张卡各写一遍 Tags.Contains(...) 导致同一个词有多种含义。
//
// 层级（子集向上兼容：挂了子标签的卡一定也挂父标签）：
//   珠泪（所有珠泪卡）
//     ├─ 珠泪怪兽        ← 所有怪兽，「含」融合怪兽
//     │    ├─ 珠泪融合怪兽   （子集）
//     │    └─ 珠泪下级怪兽   （= 珠泪怪兽 − 珠泪融合怪兽：融合素材/检索对象用这个）
//     ├─ 珠泪魔法
//     └─ 珠泪陷阱
//
// 另有跨类机制关键字「珠泪融合」（塞壬/小美/梅洛身上的发动式关键字），它不是卡种类，用 HasFusionKeyword 判断。
//
// 改口径只改这个文件，所有卡与效果自动跟上。
public static class ZhuLeiFilter
{
    /// <summary>珠泪卡（一切，含魔法/陷阱/融合怪兽）。</summary>
    public static bool IsCard(CardModel c) => c.Tags.Contains(YunoTags.ZhuLei);

    /// <summary>珠泪怪兽（含珠泪融合怪兽）。</summary>
    public static bool IsMonster(CardModel c) => c.Tags.Contains(YunoTags.ZhuLeiGuaiShou);

    /// <summary>珠泪融合怪兽。</summary>
    public static bool IsFusionMonster(CardModel c) => c.Tags.Contains(YunoTags.ZhuLeiRongHeGuaiShou);

    /// <summary>珠泪下级怪兽 = 珠泪怪兽里非融合的那些（融合素材、检索对象）。</summary>
    public static bool IsLowerMonster(CardModel c) => c.Tags.Contains(YunoTags.ZhuLeiXiaJiGuaiShou);

    /// <summary>珠泪魔法。</summary>
    public static bool IsSpell(CardModel c) => c.Tags.Contains(YunoTags.ZhuLeiMoFa);

    /// <summary>珠泪陷阱。</summary>
    public static bool IsTrap(CardModel c) => c.Tags.Contains(YunoTags.ZhuLeiXianJing);

    /// <summary>除珠泪融合怪兽以外的珠泪卡（"检索1张珠泪卡，融合怪兽除外"这种口径）。</summary>
    public static bool IsCardExceptFusionMonster(CardModel c) => IsCard(c) && !IsFusionMonster(c);

    /// <summary>是否带「珠泪融合」这个发动式关键字（不是卡种类，是机制）。</summary>
    public static bool HasFusionKeyword(CardModel c) => c.Keywords.Contains(YunoKeywords.ZhuLeiRongHe);
}
