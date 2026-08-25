using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using YunoMod.Scripts.Base;
using YunoMod.Scripts.Power;

namespace YunoMod.Scripts.Relics;

public class AncientSearchDiaryRelic : YunoBaseRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    private const string _DiaryCount = "DiaryCount";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_DiaryCount,2)
    ];

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<DiaryPower>(choiceContext, Owner.Creature, 1, base.Owner.Creature, null);
        }
    }

    // 使用卡牌造成单体伤害时：目标获得「注视」，其他目标失去「注视」
    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner.Creature) return;   // 自己的攻击
        if (command.CardPlay?.Card == null) return;       // 必须是卡牌造成的（群体/能力伤害不触发）
        if (!command.IsSingleTargeted) return;            // 只对单体伤害生效（群体无法区分目标）

        var target = command.Results.SelectMany(r => r).FirstOrDefault()?.Receiver;
        if (target == null || !target.IsAlive) return;

        // 移除其他目标身上的注视
        foreach (var enemy in Owner.Creature.CombatState!.HittableEnemies)
        {
            if (enemy != target && enemy.HasPower<ZhuShiPower>())
            {
                await PowerCmd.Remove(enemy.GetPower<ZhuShiPower>()!);
            }
        }

        // 目标获得注视
        if (!target.HasPower<ZhuShiPower>())
            await PowerCmd.Apply<ZhuShiPower>(choiceContext, target, 1, Owner.Creature, null);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();

        int amount = Owner.Creature.GetPowerAmount<DiaryPower>();

        if (amount > 0)
            await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, amount, Owner.Creature, null);
    }

    public override async Task AfterRemoved()
    {
        if (Owner.Creature.HasPower<DiaryPower>())
        {
            await PowerCmd.Decrement(Owner.Creature.GetPower<DiaryPower>()!);
        }
    }

    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        int amount = DynamicVars[_DiaryCount].IntValue;

        for (int i = 0; i < amount; i++)
            GetDiary();
    }

    private async void GetDiary()
    {
        List<int> available = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        if (Owner.GetRelic<BreedDiaryRelic>() == null)
            available[0] = 1;
        if (Owner.GetRelic<ClairvoyanceRelic>() == null)
            available[1] = 1;
        if (Owner.GetRelic<EscapeDiaryRelic>() == null)
            available[2] = 1;
        if (Owner.GetRelic<ExchangeDiaryRelic>() == null)
            available[3] = 1;
        if (Owner.GetRelic<FeedDiaryRelic>() == null)
            available[4] = 1;
        if (Owner.GetRelic<GraffitiDiaryRelic>() == null)
            available[5] = 1;
        if (Owner.GetRelic<JusticeDiaryRelic>() == null)
            available[6] = 1;
        if (Owner.GetRelic<KillingDiaryRelic>() == null)
            available[7] = 1;
        if (Owner.GetRelic<RadarDiaryRelic>() == null)
            available[8] = 1;
        if (Owner.GetRelic<SearchDiaryRelic>() == null)
            available[9] = 1;
        if (Owner.GetRelic<SeekDiaryRelic>() == null)
            available[10] = 1;
        if (Owner.GetRelic<TheWatcherRelic>() == null)
            available[11] = 1;

        List<int> candidates = [];
        for (int i = 0; i < available.Count; i++)
        {
            if (available[i] == 1)
                candidates.Add(i);
        }

        if (candidates.Count == 0)
            return;

        int pick = candidates[Random.Shared.Next(candidates.Count)];
        switch (pick)
        {
            case 0: await RelicCmd.Obtain<BreedDiaryRelic>(Owner); break;
            case 1: await RelicCmd.Obtain<ClairvoyanceRelic>(Owner); break;
            case 2: await RelicCmd.Obtain<EscapeDiaryRelic>(Owner); break;
            case 3: await RelicCmd.Obtain<ExchangeDiaryRelic>(Owner); break;
            case 4: await RelicCmd.Obtain<FeedDiaryRelic>(Owner); break;
            case 5: await RelicCmd.Obtain<GraffitiDiaryRelic>(Owner); break;
            case 6: await RelicCmd.Obtain<JusticeDiaryRelic>(Owner); break;
            case 7: await RelicCmd.Obtain<KillingDiaryRelic>(Owner); break;
            case 8: await RelicCmd.Obtain<RadarDiaryRelic>(Owner); break;
            case 9: await RelicCmd.Obtain<SearchDiaryRelic>(Owner); break;
            case 10: await RelicCmd.Obtain<SeekDiaryRelic>(Owner); break;
            case 11: await RelicCmd.Obtain<TheWatcherRelic>(Owner); break;
        }
    }

}
