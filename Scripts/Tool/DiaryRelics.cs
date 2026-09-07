using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace YunoMod.Scripts.Relics;

/// <summary>
/// 「日记」遗物家族清单（已固定，不再新增）。
/// 集中登记后即可像卡牌标签一样判断/筛选遗物，例如：
///   relic 是否日记：DiaryRelics.IsDiary(relic)
///   玩家持有的日记：DiaryRelics.OwnedBy(player)
/// </summary>
public static class DiaryRelics
{
    /// <summary>全部「日记」遗物（含先古「跟踪日记（新）」）。</summary>
    public static readonly IReadOnlyList<Type> All =
    [
        typeof(BreedDiaryRelic),
        typeof(ClairvoyanceRelic),
        typeof(EscapeDiaryRelic),
        typeof(ExchangeDiaryRelic),
        typeof(FeedDiaryRelic),
        typeof(GraffitiDiaryRelic),
        typeof(JusticeDiaryRelic),
        typeof(KillingDiaryRelic),
        typeof(RadarDiaryRelic),
        typeof(SearchDiaryRelic),
        typeof(SeekDiaryRelic),
        typeof(TheWatcherRelic),
        typeof(AncientSearchDiaryRelic),
    ];

    /// <summary>先古「跟踪日记（新）」可以补给的普通日记（不含它自身）。</summary>
    public static readonly IReadOnlyList<Type> ObtainableByAncientSearch =
        All.Where(t => t != typeof(AncientSearchDiaryRelic)).ToArray();

    /// <summary>该遗物是否属于「日记」家族。</summary>
    public static bool IsDiary(RelicModel relic) =>
        relic != null && All.Any(t => t.IsInstanceOfType(relic));

    /// <summary>玩家当前持有的全部日记遗物（按持有顺序）。</summary>
    public static IEnumerable<RelicModel> OwnedBy(Player player) =>
        player.Relics.Where(IsDiary);

    /// <summary>玩家是否已持有指定日记（类型匹配，包含派生类）。</summary>
    public static bool Owns(Player player, Type diaryType) =>
        player.Relics.Any(r => diaryType.IsInstanceOfType(r));

    /// <summary>随机挑1本尚未持有的普通日记（可变实例）；若已全部持有则返回 null。</summary>
    public static RelicModel? PickRandomUnobtained(Player player)
    {
        var missing = ObtainableByAncientSearch
            .Where(t => !Owns(player, t))
            .ToList();
        if (missing.Count == 0) return null;

        Type pick = player.RunState.Rng.Niche.NextItem(missing)!;
        return ModelDb.GetById<RelicModel>(ModelDb.GetId(pick)).ToMutable();
    }

    /// <summary>随机获得1本尚未持有的普通日记；若已全部持有则返回 false。</summary>
    public static async Task<bool> TryGrantRandomUnobtained(Player player)
    {
        RelicModel? relic = PickRandomUnobtained(player);
        if (relic == null) return false;
        await RelicCmd.Obtain(relic, player);
        return true;
    }
}
