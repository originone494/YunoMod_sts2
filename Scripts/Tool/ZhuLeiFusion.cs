using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using YunoMod.Scripts.Cards.Special;
using YunoMod.Scripts.Hook;

namespace YunoMod.Scripts.Custom;

// 珠泪融合：由触发灵活的珠泪卡调用
// 流程：根据弃牌堆与场上堆内容展示候选融合怪兽（水仙/卡雷多哈特/露莎卡人鱼）→ 玩家选1 → 按选择选素材 → 触发卡+素材返回抽牌堆 → 融合怪兽入手牌
// 说明：触发灵活的卡在弃牌堆中（被弃后触发灵活），作为素材之一被筛出返回抽牌堆
public static class ZhuLeiFusion
{
    public static async Task TryFusion(PlayerChoiceContext choiceContext, Player player, CardModel triggerCard)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        // 素材池 = 弃牌堆 + 场上（Play）堆。
        // 雷诺哈特等卡在效果处理期间位于场上堆（Play，游戏在效果结束后才将其放入弃牌堆），
        // 若不纳入会误判“素材不够”，例如打出雷诺哈特检索并丢弃梅洛时，梅洛触发融合却找不到雷诺哈特。
        var availableCards = PileType.Discard.GetPile(player).Cards
            .Concat(PileType.Play.GetPile(player).Cards)
            .Distinct()
            .ToList();
        if (availableCards.Count == 0) return;

        // 素材池中的「珠泪怪兽卡」（触发卡在弃牌堆中，自然包含在内）
        var monsters = availableCards.Where(c => c.Tags.Contains(YunoTags.ZhuLeiGuaiShou)).ToList();
        bool hasLeiNuo = availableCards.Any(c => c is ZhuLeiLeiNuoHaTeCard);
        bool hasShuiXian = availableCards.Any(c => c is ZhuLeiShuiXianCard);

        // 构建候选融合怪兽（Splash 式展示，最多3张）
        var candidates = new List<CardModel>();
        if (monsters.Count > 0)
            candidates.Add(combatState.CreateCard<ZhuLeiShuiXianCard>(player));
        if (hasLeiNuo && monsters.Count > 1)
            candidates.Add(combatState.CreateCard<ZhuLeiKaLeiDuoHaTeCard>(player));
        if (hasShuiXian)
            candidates.Add(combatState.CreateCard<ZhuLeiLuShaKaCard>(player));

        if (candidates.Count == 0) return;

        // 玩家从候选融合怪兽中选1张（canSkip: true 允许放弃融合）
        CardModel? picked = await CardSelectCmd.FromChooseACardScreen(choiceContext, candidates, player, canSkip: true);
        if (picked == null) return;

        // 素材 = 触发灵活的卡（必含） + 按选择从弃牌堆选的卡
        var materials = new List<CardModel> { triggerCard };

        if (picked is ZhuLeiShuiXianCard)
        {
            // 选水仙：再从弃牌堆选1张「珠泪怪兽卡」
            var m = await SelectOne(choiceContext, player, monsters.Where(c => c != triggerCard).ToList(), ShuiXianMaterialPrompt);
            if (m == null) return;
            materials.Add(m);
        }
        else if (picked is ZhuLeiKaLeiDuoHaTeCard)
        {
            // 选卡雷多哈特：先选1张「珠泪·雷诺哈特」，再选1张「珠泪怪兽卡」
            var leiNuoOptions = availableCards.OfType<ZhuLeiLeiNuoHaTeCard>().Cast<CardModel>().ToList();
            var m1 = await SelectOne(choiceContext, player, leiNuoOptions, KaLeiDuoHaTeRenoPrompt);
            if (m1 == null) return;
            materials.Add(m1);

            var monsterOptions = monsters.Where(c => c != triggerCard && c != m1).ToList();
            var m2 = await SelectOne(choiceContext, player, monsterOptions, KaLeiDuoHaTeMonsterPrompt);
            if (m2 == null) return;
            materials.Add(m2);
        }
        else if (picked is ZhuLeiLuShaKaCard)
        {
            // 选露莎卡人鱼：再从弃牌堆选1张「珠泪·水仙」
            var shuiXianOptions = availableCards.OfType<ZhuLeiShuiXianCard>().Cast<CardModel>().ToList();
            var m = await SelectOne(choiceContext, player, shuiXianOptions, LuShaKaMaterialPrompt);
            if (m == null) return;
            materials.Add(m);
        }

        // 触发卡 + 素材卡返回抽牌堆
        foreach (var mat in materials.Distinct())
        {
            await CardPileCmd.Add(mat, PileType.Draw);
        }

        // 标记触发卡已被灵活效果处理（弃牌流程跳过它，避免返回抽牌堆后被再次弃掉）
        LingHuoHook.HandledByLingHuo.Add(triggerCard);

        picked.EnergyCost.AddThisTurn(-picked.EnergyCost.GetWithModifiers(CostModifiers.None));


        // 融合怪兽加入手牌
        await CardPileCmd.AddGeneratedCardToCombat(picked, PileType.Hand, player);
    }

    private static async Task<CardModel?> SelectOne(PlayerChoiceContext choiceContext, Player player, List<CardModel> options, LocString prompt)
    {
        if (options.Count == 0) return null;
        // 统一用简单网格选素材，并显示对应的融合提示文字
        return (await CardSelectCmd.FromSimpleGrid(
            choiceContext, options, player, new CardSelectorPrefs(prompt, 1, 1))).FirstOrDefault();
    }

    private static LocString ShuiXianMaterialPrompt { get; } = new("card_selection", "TO_FUSION_SHUI_XIAN");
    private static LocString KaLeiDuoHaTeRenoPrompt { get; } = new("card_selection", "TO_FUSION_KA_LEI_DUO_HA_TE_RENO");
    private static LocString KaLeiDuoHaTeMonsterPrompt { get; } = new("card_selection", "TO_FUSION_KA_LEI_DUO_HA_TE_MONSTER");
    private static LocString LuShaKaMaterialPrompt { get; } = new("card_selection", "TO_FUSION_LU_SHA_KA");
}
