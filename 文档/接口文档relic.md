# 杀戮尖塔 2 Mod —— Relic（遗物）接口文档

> 基于 `RelicModel` 基类整理 | 命名空间：`MegaCrit.Sts2.Core.Models.Relics`

---

## 1. RelicModel 基类

```csharp
public abstract class RelicModel : AbstractModel
{
    public Player Owner { get; set; }
    public abstract RelicRarity Rarity { get; }
    public DynamicVarSet DynamicVars { get; }
    public int StackCount { get; }               // IsStackable=true 时 >1
    public int FloorAddedToDeck { get; set; }
    public RelicStatus Status { get; set; }      // Normal / Active / Disabled

    public LocString Title { get; }
    public LocString Flavor { get; }
    public LocString DynamicDescription { get; } // 自动注入 DynamicVars
    protected LocString Description { get; }
    protected LocString SelectionScreenPrompt { get; }

    public Texture2D Icon { get; }
    public Texture2D IconOutline { get; }
    public Texture2D BigIcon { get; }
    protected virtual string IconBaseName => Id.Entry.ToLowerInvariant();
    protected virtual string PackedIconPath => ...;
    protected virtual string BigIconPath => ...;
    public virtual string FlashSfx => "event:/sfx/ui/relic_activate_general";

    public event Action<RelicModel, IEnumerable<Creature>>? Flashed;
    public event Action? DisplayAmountChanged;
    public event Action? StatusChanged;
}
```

---

## 2. 核心属性/覆写项

| 属性 | 类型 | 默认值 | 说明 | 备注 |
|------|------|--------|------|------|
| `Rarity` | `RelicRarity` | (abstract) | 遗物稀有度 | |
| `IsUsedUp` | `bool` | `false` | 是否已耗尽（变灰色） | 配合 `_wasUsed` 字段 |
| `HasUponPickupEffect` | `bool` | `false` | 获得时立即触发效果 | 如加最大 HP |
| `IsStackable` | `bool` | `false` | 可堆叠多个 | 例：`Circlet` |
| `IsWax` | `bool` | `false` | 蜡质遗物（ToyBox 相关） | |
| `IsMelted` | `bool` | `false` | 是否已熔化 | |
| `ShowCounter` | `bool` | `false` | 是否显示计数器 | 例：`Nunchaku` |
| `DisplayAmount` | `int` | `0` | 计数器显示值 | |
| `Status` | `RelicStatus` | `Normal` | 外观状态 | Normal / Active / Disabled |
| `ShouldFlashOnPlayer` | `bool` | `true` | Flash 动画目标是否为玩家 | |
| `MerchantCost` | `int` | 按稀有度自动 | 商店价格 | |
| `IsTradable` | `bool` | 自动计算 | 是否可交易 | |
| `IsAllowedInShops` | `bool` | `true` | 是否允许出现在商店 | 例：`OldCoin` 为 false |
| `SpawnsPets` | `bool` | `false` | 是否生成宠物 | |
| `StackCount` | `int` | `1` | 堆叠数量 | 需 `IsStackable=true` |

---

## 3. RelicRarity 与 RelicStatus

```csharp
public enum RelicRarity
{
    None,       // 特殊（Circlet）
    Starter,    // 初始遗物
    Common,
    Uncommon,
    Rare,
    Shop,
    Event,
    Ancient     // BOSS
}

public enum RelicStatus
{
    Normal,   // 正常
    Active,   // 脉冲发光
    Disabled  // 灰色划叉（已耗尽）
}
```

---

## 4. DynamicVar 遗物变量系统

### 常用变量类型

| 类型 | 说明 |
|------|------|
| `PowerVar<T>(decimal)` | 引用 Power 数值 |
| `PowerVar<T>(string, decimal)` | 命名 Power 变量 |
| `BlockVar(decimal, ValueProp)` | 格挡值 |
| `HealVar(decimal)` | 治疗值 |
| `DamageVar(decimal, ValueProp)` | 伤害值 |
| `EnergyVar(decimal)` | 能量值 |
| `CardsVar(int)` | 卡牌数量 |
| `MaxHpVar(decimal)` | 最大 HP |
| `GoldVar(int)` | 金币 |
| `DynamicVar(string, decimal)` | 自定义数值变量 |

### 定义

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
{
    new CardsVar(3),
    new DamageVar(5m, ValueProp.Unpowered)
};
```

### 访问

- 通过类型名：`DynamicVars.Strength.BaseValue`、`DynamicVars.Energy.BaseValue`
- 通过自定义键：`DynamicVars["Discount"].BaseValue`
- 本地化文本中用 `{变量名}` 自动注入，例如 `"获得 {Block} 点格挡"`。

---

## 5. HoverTip 系统

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips => ...;
```

### 工厂方法

| 工厂方法 | 说明 |
|----------|------|
| `HoverTipFactory.FromPower<T>()` | 引用 Power 提示 |
| `HoverTipFactory.ForEnergy(this)` | 能量提示 |
| `HoverTipFactory.Static(StaticHoverTip.Block)` | 静态格挡提示 |
| `HoverTipFactory.Static(StaticHoverTip.Channeling)` | 充能提示 |
| `HoverTipFactory.FromCard<T>()` | 卡牌提示 |
| `HoverTipFactory.FromCardWithCardHoverTips<T>()` | 卡牌及子提示 |
| `HoverTipFactory.FromOrb<T>()` | Orb 提示 |

---

## 6. 生命周期钩子

### 遗物自身
| 钩子 | 签名 |
|------|------|
| `AfterObtained` | `Task AfterObtained()` |
| `AfterRemoved` | `Task AfterRemoved()` |
| `IsAllowed` | `bool IsAllowed(IRunState runState)` |

### 战斗
| 钩子 | 签名 |
|------|------|
| `BeforeCombatStart` | `Task BeforeCombatStart()` |
| `BeforeCombatStartLate` | `Task BeforeCombatStartLate()` |
| `AfterCombatEnd` | `Task AfterCombatEnd(CombatRoom room)` |
| `AfterCombatVictoryEarly` | `Task AfterCombatVictoryEarly(CombatRoom room)` |
| `AfterCombatVictory` | `Task AfterCombatVictory(CombatRoom room)` |

### 回合
| 钩子 | 签名 |
|------|------|
| `BeforeSideTurnStart` | `Task BeforeSideTurnStart(PlayerChoiceContext, CombatSide, CombatState)` |
| `AfterSideTurnStart` | `Task AfterSideTurnStart(CombatSide, CombatState)` |
| `AfterSideTurnStartLate` | `Task AfterSideTurnStartLate(CombatSide, CombatState)` |
| `BeforeTurnEndVeryEarly` | `Task BeforeTurnEndVeryEarly(PlayerChoiceContext, CombatSide)` |
| `BeforeTurnEnd` | `Task BeforeTurnEnd(PlayerChoiceContext, CombatSide)` |
| `AfterTurnEnd` | `Task AfterTurnEnd(PlayerChoiceContext, CombatSide)` |
| `AfterTurnEndLate` | `Task AfterTurnEndLate(PlayerChoiceContext, CombatSide)` |
| `AfterPlayerTurnStart` | `Task AfterPlayerTurnStart(PlayerChoiceContext, Player)` |
| `AfterPlayerTurnStartEarly` | `Task AfterPlayerTurnStartEarly(PlayerChoiceContext, Player)` |
| `AfterPlayerTurnStartLate` | `Task AfterPlayerTurnStartLate(PlayerChoiceContext, Player)` |

### 卡牌
| 钩子 | 签名 |
|------|------|
| `BeforeCardPlayed` | `Task BeforeCardPlayed(CardPlay)` |
| `AfterCardPlayed` | `Task AfterCardPlayed(PlayerChoiceContext, CardPlay)` |
| `AfterCardPlayedLate` | `Task AfterCardPlayedLate(PlayerChoiceContext, CardPlay)` |
| `BeforeHandDraw` | `Task BeforeHandDraw(Player, PlayerChoiceContext, CombatState)` |
| `BeforeHandDrawLate` | `Task BeforeHandDrawLate(Player, PlayerChoiceContext, CombatState)` |
| `AfterHandEmptied` | `Task AfterHandEmptied(PlayerChoiceContext, Player)` |
| `AfterCardDrawn` | `Task AfterCardDrawn(PlayerChoiceContext, CardModel, bool fromHandDraw)` |
| `AfterCardDrawnEarly` | `Task AfterCardDrawnEarly(PlayerChoiceContext, CardModel, bool fromHandDraw)` |
| `AfterCardExhausted` | `Task AfterCardExhausted(PlayerChoiceContext, CardModel, bool causedByEthereal)` |
| `AfterCardDiscarded` | `Task AfterCardDiscarded(PlayerChoiceContext, CardModel)` |
| `BeforeFlush` | `Task BeforeFlush(PlayerChoiceContext, Player)` |
| `BeforeFlushLate` | `Task BeforeFlushLate(PlayerChoiceContext, Player)` |
| `AfterCardChangedPiles` | `Task AfterCardChangedPiles(CardModel, PileType, AbstractModel?)` |
| `AfterCardChangedPilesLate` | `Task AfterCardChangedPilesLate(CardModel, PileType, AbstractModel?)` |

### 伤害/格挡
| 钩子 | 签名 |
|------|------|
| `BeforeDamageReceived` | `Task BeforeDamageReceived(PlayerChoiceContext, Creature, decimal, ValueProp, Creature?, CardModel?)` |
| `AfterDamageReceived` | `Task AfterDamageReceived(PlayerChoiceContext, Creature, DamageResult, ValueProp, Creature?, CardModel?)` |
| `AfterDamageReceivedLate` | `Task AfterDamageReceivedLate(PlayerChoiceContext, Creature, DamageResult, ValueProp, Creature?, CardModel?)` |
| `AfterDamageGiven` | `Task AfterDamageGiven(PlayerChoiceContext, Creature?, DamageResult, ValueProp, Creature, CardModel?)` |
| `AfterBlockGained` | `Task AfterBlockGained(Creature, decimal, ValueProp, CardModel?)` |
| `AfterBlockBroken` | `Task AfterBlockBroken(Creature)` |
| `AfterCurrentHpChanged` | `Task AfterCurrentHpChanged(Creature, decimal)` |

### 死亡
| 钩子 | 签名 |
|------|------|
| `BeforeDeath` | `Task BeforeDeath(Creature)` |
| `AfterDeath` | `Task AfterDeath(PlayerChoiceContext, Creature, bool, float)` |
| `AfterPreventingDeath` | `Task AfterPreventingDeath(Creature)` |
| `ShouldDieLate` | `bool ShouldDieLate(Creature)` |

### 房间/地图
| 钩子 | 签名 |
|------|------|
| `BeforeRoomEntered` | `Task BeforeRoomEntered(AbstractRoom)` |
| `AfterRoomEntered` | `Task AfterRoomEntered(AbstractRoom)` |
| `AfterActEntered` | `Task AfterActEntered()` |
| `AfterCreatureAddedToCombat` | `Task AfterCreatureAddedToCombat(Creature)` |

### 其他
| 钩子 | 签名 |
|------|------|
| `AfterGoldGained` | `Task AfterGoldGained(Player)` |
| `AfterItemPurchased` | `Task AfterItemPurchased(Player, MerchantEntry, int)` |
| `AfterRewardTaken` | `Task AfterRewardTaken(Player, Reward)` |
| `AfterPotionUsed` | `Task AfterPotionUsed(PotionModel, Creature?)` |
| `AfterShuffle` | `Task AfterShuffle(PlayerChoiceContext, Player)` |
| `AfterStarsGained` | `Task AfterStarsGained(int, Player)` |
| `AfterStarsSpent` | `Task AfterStarsSpent(int, Player)` |
| `AfterOrbEvoked` | `Task AfterOrbEvoked(PlayerChoiceContext, OrbModel, IEnumerable<Creature>)` |

---

## 7. 数值修正/修饰方法

| 方法 | 说明 |
|------|------|
| `ModifyDamageAdditive(Creature?, decimal, ValueProp, Creature?, CardModel?)` | 伤害加法修正 |
| `ModifyDamageMultiplicative(Creature?, decimal, ValueProp, Creature?, CardModel?)` | 伤害乘法修正 |
| `ModifyHpLostAfterOsty(Creature, decimal, ValueProp, Creature?, CardModel?)` | HP 损失修正（Osty 后） |
| `ModifyHpLostBeforeOsty(Creature, decimal, ValueProp, Creature?, CardModel?)` | HP 损失修正（Osty 前） |
| `AfterModifyingHpLostAfterOsty()` | 修正后回调 |
| `AfterModifyingHpLostBeforeOsty()` | 修正前回调 |
| `ModifyMaxEnergy(Player, decimal)` | 修改最大能量 |
| `ModifyHandDraw(Player, decimal)` | 修改抽牌数 |
| `AfterModifyingHandDraw()` | 抽牌修正后回调 |
| `ModifyMerchantPrice(Player, MerchantEntry, decimal)` | 修改商店价格 |
| `ModifyXValue(CardModel, int)` | 修改 X 费用数值 |
| `ModifyPowerAmountGiven(PowerModel, Creature, decimal, Creature?, CardModel?)` | 修改给予的 Power 数值 |
| `ModifyVulnerableMultiplier(Creature, decimal, ValueProp, Creature?, CardModel?)` | 修改易伤倍率 |
| `ModifyWeakMultiplier(Creature, decimal, ValueProp, Creature?, CardModel?)` | 修改虚弱倍率 |

---

## 8. 条件/判断方法

| 方法 | 说明 |
|------|------|
| `ShouldFlush(Player)` | 回合结束时是否清空手牌 |
| `ShouldPlayerResetEnergy(Player)` | 是否重置能量 |
| `ShouldPlay(CardModel, AutoPlayType)` | 是否允许打出某卡牌 |
| `ShouldProcurePotion(PotionModel, Player)` | 是否允许获得药水 |
| `ShouldGainGold(decimal, Player)` | 是否允许获得金币 |
| `ShouldDieLate(Creature)` | 是否应该死亡 |
| `ShouldClearBlock(Creature)` | 是否清空格挡（通常 Power 实现） |

---

## 9. 奖励与商店方法

| 方法 | 签名 |
|------|------|
| `TryModifyRewards` | `bool(Player, List<Reward>, AbstractRoom?)` |
| `TryModifyRewardsLate` | `bool(Player, List<Reward>, AbstractRoom?)` |

---

## 10. SavedProperty 持久化模式

使用 `[SavedProperty]` 标记需保存的字段。

```csharp
[SavedProperty]
public int AttacksPlayed
{
    get => _attacksPlayed;
    set
    {
        AssertMutable();
        _attacksPlayed = value;
        UpdateDisplay(); // 或 InvokeDisplayAmountChanged()
    }
}
```

**setter 三原则：** 调用 `AssertMutable()` → 更新后备字段 → 更新 UI（`UpdateDisplay()` / `InvokeDisplayAmountChanged()`）。

---

## 11. 显示与计数器系统

- `ShowCounter`：覆写返回 `true` 或根据 `CombatManager.Instance.IsInProgress`。
- `DisplayAmount`：覆写返回计数值。
- `Status` 联动：到达阈值时设为 `Active` 脉冲，用完后设为 `Disabled`。
- 常用重置点：`BeforeCombatStart`、`BeforeSideTurnStart`、`AfterCombatEnd`。

---

## 12. Relic 指令

| 方法 | 说明 |
|------|------|
| `Flash()` / `Flash(IEnumerable<Creature>)` | 播放闪烁动画 |
| `InvokeDisplayAmountChanged()` | 触发计数器 UI 更新 |
| `IncrementStackCount()` | 堆叠计数 +1（需 `IsStackable`） |

**常用非 Relic 指令：** `PowerCmd.Apply<T>()`、`CreatureCmd.GainBlock/Heal/Damage`、`PlayerCmd.GainEnergy/GainGold`、`CardPileCmd.Draw/AddCurseToDeck` 等（需引入相应命名空间）。
