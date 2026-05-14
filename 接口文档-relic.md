# 杀戮尖塔 2 Mod —— Relic（遗物）接口文档

> 基于 `code\Relics\` 与 `RelicModel` 基类整理\
> 命名空间：`MegaCrit.Sts2.Core.Models.Relics`\
> 基类：`RelicModel : AbstractModel` (位于 `MegaCrit.Sts2.Core.Models`)

***

## 目录

1. [RelicModel 基类](#1-relicmodel-基类)
2. [核心属性/覆写项](#2-核心属性覆写项)
3. [RelicRarity 与 RelicStatus](#3-relicrarity-与-relicstatus)
4. [DynamicVar 遗物变量系统](#4-dynamicvar-遗物变量系统)
5. [HoverTip 系统](#5-hovertip-系统)
6. [生命周期钩子（按执行顺序）](#6-生命周期钩子按执行顺序)
7. [数值修正/修饰方法](#7-数值修正修饰方法)
8. [条件/判断方法](#8-条件判断方法)
9. [奖励与商店方法](#9-奖励与商店方法)
10. [SavedProperty 持久化模式](#10-savedproperty-持久化模式)
11. [显示与计数器系统](#11-显示与计数器系统)
12. [Relic 指令](#12-relic-指令)
13. [完整 Relic 示例](#13-完整-relic-示例)
14. [附录：常用命名空间](#14-附录常用命名空间)

***

## 1. RelicModel 基类

```csharp
namespace MegaCrit.Sts2.Core.Models;

public abstract class RelicModel : AbstractModel
{
    // ===== 核心属性 =====
    public Player Owner { get; set; }
    public abstract RelicRarity Rarity { get; }
    public DynamicVarSet DynamicVars { get; }
    public int StackCount { get; }           // 仅 IsStackable=true 时 >1
    public int FloorAddedToDeck { get; set; }
    public RelicStatus Status { get; set; }  // Normal / Active / Disabled

    // ===== 文字描述 =====
    public LocString Title { get; }
    public LocString Flavor { get; }
    public LocString DynamicDescription { get; }  // 自动注入 DynamicVars
    protected LocString Description { get; }
    protected LocString SelectionScreenPrompt { get; }

    // ===== 图标 =====
    public Texture2D Icon { get; }
    public Texture2D IconOutline { get; }
    public Texture2D BigIcon { get; }
    protected virtual string IconBaseName => Id.Entry.ToLowerInvariant();
    protected virtual string PackedIconPath => ...;
    protected virtual string BigIconPath => ...;
    public virtual string FlashSfx => "event:/sfx/ui/relic_activate_general";

    // ===== 事件 =====
    public event Action<RelicModel, IEnumerable<Creature>>? Flashed;
    public event Action? DisplayAmountChanged;
    public event Action? StatusChanged;
}
```

***

## 2. 核心属性/覆写项

| 属性                    | 类型            | 默认值        | 说明                 | 使用示例                                                    |
| --------------------- | ------------- | ---------- | ------------------ | ------------------------------------------------------- |
| `Rarity`              | `RelicRarity` | (abstract) | 遗物稀有度              | `RelicRarity.Common`                                    |
| `IsUsedUp`            | `bool`        | `false`    | 是否已耗尽（变为灰色）        | `LizardTail` 用 `_wasUsed` 控制                            |
| `HasUponPickupEffect` | `bool`        | `false`    | 获取时立即触发的效果（如加最大HP） | `Mango`, `Pear`, `Strawberry`, `OldCoin`, `CallingBell` |
| `IsStackable`         | `bool`        | `false`    | 是否可以堆叠多个           | `Circlet : IsStackable => true`                         |
| `IsWax`               | `bool`        | `false`    | 是否为蜡质遗物（ToyBox 相关） | `_isWax` 属性                                             |
| `IsMelted`            | `bool`        | `false`    | 是否已熔化              | `_isMelted` 属性                                          |
| `ShowCounter`         | `bool`        | `false`    | 是否显示计数器            | `Nunchaku`, `PenNib`, `HappyFlower`, `StoneCalendar`    |
| `DisplayAmount`       | `int`         | `0`        | 计数器显示值             | 见第 11 章                                                 |
| `Status`              | `RelicStatus` | `Normal`   | 遗物外观状态             | `Normal` / `Active`(脉冲) / `Disabled`(灰色)                |
| `ShouldFlashOnPlayer` | `bool`        | `true`     | Flash 动画目标是否为玩家    | 默认是                                                     |
| `MerchantCost`        | `int`         | 按稀有度自动     | 商店价格               | 自动根据 Rarity 计算                                          |
| `IsTradable`          | `bool`        | 自动计算       | 是否可交易              | 自动根据多种条件                                                |
| `IsAllowedInShops`    | `bool`        | `true`     | 是否允许出现在商店          | `OldCoin : IsAllowedInShops => false`                   |
| `SpawnsPets`          | `bool`        | `false`    | 是否生成宠物             | 特殊遗物使用                                                  |
| `StackCount`          | `int`         | `1`        | 堆叠数量               | `IsStackable=true` 时有效                                  |

### 使用模式

```csharp
// 基本稀有度声明
public override RelicRarity Rarity => RelicRarity.Common;
public override RelicRarity Rarity => RelicRarity.Uncommon;
public override RelicRarity Rarity => RelicRarity.Rare;
public override RelicRarity Rarity => RelicRarity.Shop;
public override RelicRarity Rarity => RelicRarity.Event;
public override RelicRarity Rarity => RelicRarity.Ancient;  // BOSS遗物
public override RelicRarity Rarity => RelicRarity.Starter;   // 初始遗物
public override RelicRarity Rarity => RelicRarity.None;      // 特殊（Circlet）

// 一次性效果
public override bool HasUponPickupEffect => true;   // AfterObtained 中实现

// 是否已耗尽
public override bool IsUsedUp => _wasUsed;          // 配合 SavedProperty

// 堆叠
public override bool IsStackable => true;           // Circlet 模式
```

***

## 3. RelicRarity 与 RelicStatus

### RelicRarity

```csharp
namespace MegaCrit.Sts2.Core.Entities.Relics;

public enum RelicRarity
{
    None,       // 特殊（Circlet）
    Starter,    // 初始遗物（BurningBlood）
    Common,     // 普通
    Uncommon,   // 罕见
    Rare,       // 稀有
    Shop,       // 商店
    Event,      // 事件
    Ancient     // BOSS
}
```

### RelicStatus

```csharp
namespace MegaCrit.Sts2.Core.Entities.Relics;

public enum RelicStatus
{
    Normal,   // 正常显示
    Active,   // 脉冲发光动画
    Disabled  // 灰色划叉（已用完）
}
```

Status 控制遗物图标的外观表现，在 `UpdateTexture()` 中通过 Shader 参数控制：

```csharp
// RelicModel.UpdateTexture 内部逻辑
switch (Status)
{
    case RelicStatus.Normal:   // 不发光，不划叉
    case RelicStatus.Active:   // 脉冲发光
    case RelicStatus.Disabled: // 灰色划叉（LizardTail 用完时）
}
```

***

## 4. DynamicVar 遗物变量系统

遗物通过 `CanonicalVars` 定义可在本地化文本中动态显示的变量。

### 常用 DynamicVar 类型

| 类型                              | 构造参数  | 说明            | 使用示例                                              |
| ------------------------------- | ----- | ------------- | ------------------------------------------------- |
| `PowerVar<T>(decimal)`          | 数值    | 引用某 Power 的数值 | `new PowerVar<StrengthPower>(1m)` → 显示"1点力量"      |
| `PowerVar<T>(string, decimal)`  | 键名+数值 | 命名 Power 变量   | `new PowerVar<StrengthPower>("SelfStrength", 2m)` |
| `BlockVar(decimal, ValueProp)`  | 数值+属性 | 格挡值           | `new BlockVar(10m, ValueProp.Unpowered)`          |
| `HealVar(decimal)`              | 数值    | 治疗值           | `new HealVar(6m)`                                 |
| `DamageVar(decimal, ValueProp)` | 数值+属性 | 伤害值           | `new DamageVar(5m, ValueProp.Unpowered)`          |
| `EnergyVar(decimal)`            | 数值    | 能量值           | `new EnergyVar(1)`                                |
| `CardsVar(int)`                 | 整数    | 卡牌数量          | `new CardsVar(2)`                                 |
| `MaxHpVar(decimal)`             | 数值    | 最大HP          | `new MaxHpVar(10m)`                               |
| `GoldVar(int)`                  | 整数    | 金币            | `new GoldVar(300)`                                |
| `DynamicVar(string, decimal)`   | 键名+数值 | 自定义数值变量       | `new DynamicVar("Discount", 50m)`                 |

### 定义语法

```csharp
// 单个变量
protected override IEnumerable<DynamicVar> CanonicalVars =>
    new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
        new PowerVar<StrengthPower>(1m)
    );

// 多个变量
protected override IEnumerable<DynamicVar> CanonicalVars =>
    new _003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
    {
        new CardsVar(3),
        new DamageVar(5m, ValueProp.Unpowered)
    });

// 自定义键名变量
protected override IEnumerable<DynamicVar> CanonicalVars =>
    new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
        new DynamicVar("Discount", 50m)
    );
```

### 访问方式

```csharp
// 通过类型名（PowerVar<T> 自动注册为 T 的类型名）
base.DynamicVars.Strength.BaseValue      // PowerVar<StrengthPower> → "Strength"
base.DynamicVars.Strength.IntValue
base.DynamicVars.Vulnerable.BaseValue    // PowerVar<VulnerablePower>
base.DynamicVars.ThornsPower.BaseValue

// DamageVar/BlockVar/HealVar 等有命名的变量
base.DynamicVars.Damage                  // DamageVar 自动注册为 "Damage"
base.DynamicVars.Damage.BaseValue
base.DynamicVars.Damage.IntValue
base.DynamicVars.Damage.Props           // ValueProp

base.DynamicVars.Block                   // BlockVar
base.DynamicVars.Heal                    // HealVar
base.DynamicVars.Energy                  // EnergyVar
base.DynamicVars.Cards                   // CardsVar
base.DynamicVars.MaxHp                   // MaxHpVar
base.DynamicVars.Gold                    // GoldVar

// 通过字符串键名（DynamicVar 自定义键名）
base.DynamicVars["Discount"].BaseValue
base.DynamicVars["Discount"].IntValue
base.DynamicVars["DamageTurn"].IntValue
base.DynamicVars["SelfStrength"].BaseValue
base.DynamicVars["EnemyStrength"].BaseValue
base.DynamicVars["BlockNextTurn"].BaseValue
base.DynamicVars["HpLossReduction"].BaseValue
base.DynamicVars["CardThreshold"].BaseValue
base.DynamicVars["ExtraDamage"].BaseValue
base.DynamicVars["Turns"].IntValue
base.DynamicVars["Relics"].BaseValue
base.DynamicVars["Dark"].BaseValue
```

### 本地化自动注入

DynamicVars 会自动注入到 `DynamicDescription` 中，因此遗物描述文本（JSON）中可直接使用变量名：

```
// relics/{id}.description 中可直接使用
"description": "每场战斗获得 {Block} 点格挡。"
// 其中 {Block} 会被 DynamicVars.Block 的值替换
```

***

## 5. HoverTip 系统

遗物通过 `ExtraHoverTips` 提供额外的悬浮提示。

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips => ...;
```

### 模式一览

| 模式       | 工厂方法                                                 | 说明             | 使用示例                                                |
| -------- | ---------------------------------------------------- | -------------- | --------------------------------------------------- |
| 引用 Power | `HoverTipFactory.FromPower<TPower>()`                | 显示某个 Power 的提示 | Vajra → StrengthPower, BronzeScales → ThornsPower   |
| 引用能量     | `HoverTipFactory.ForEnergy(this)`                    | 显示能量相关的提示      | HappyFlower, Nunchaku, Sozu, Ectoplasm, GremlinHorn |
| 静态格挡     | `HoverTipFactory.Static(StaticHoverTip.Block)`       | 格挡提示           | Anchor, Orichalcum, SelfFormingClay                 |
| 静态充能     | `HoverTipFactory.Static(StaticHoverTip.Channeling)`  | 充能提示           | SymbioticVirus                                      |
| 引用卡牌     | `HoverTipFactory.FromCard<TCard>()`                  | 卡牌提示           | 部分遗物                                                |
| 引用卡牌+子提示 | `HoverTipFactory.FromCardWithCardHoverTips<TCard>()` | 卡牌及其子提示        | CallingBell → CurseOfTheBell                        |
| 引用 Orb   | `HoverTipFactory.FromOrb<TOrb>()`                    | Orb 提示         | SymbioticVirus → DarkOrb                            |

### 定义语法

```csharp
// 单个 ExtraHoverTip
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(
        HoverTipFactory.FromPower<StrengthPower>()
    );

// 多个 ExtraHoverTip
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new _003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.ForEnergy(this)
    });

// 混合类型
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    new _003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
    {
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<DarkOrb>()
    });
```

***

## 6. 生命周期钩子（按执行顺序）

### 6.1 遗物生命周期

| 钩子              | 签名                                   | 说明                   |
| --------------- | ------------------------------------ | -------------------- |
| `AfterObtained` | `Task AfterObtained()`               | 获得遗物时触发（立即生效遗物在此实现）  |
| `AfterRemoved`  | `Task AfterRemoved()`                | 遗物被移除时触发             |
| `IsAllowed`     | `bool IsAllowed(IRunState runState)` | 判断该遗物在当前 run 中是否允许出现 |

### 6.2 战斗生命周期

| 钩子                        | 签名                                              | 说明                |
| ------------------------- | ----------------------------------------------- | ----------------- |
| `BeforeCombatStart`       | `Task BeforeCombatStart()`                      | 战斗开始前（Flash 在此调用） |
| `BeforeCombatStartLate`   | `Task BeforeCombatStartLate()`                  | 战斗开始前（延迟阶段）       |
| `AfterCombatEnd`          | `Task AfterCombatEnd(CombatRoom room)`          | 战斗结束（无论胜负）        |
| `AfterCombatVictoryEarly` | `Task AfterCombatVictoryEarly(CombatRoom room)` | 战斗胜利（早期）          |
| `AfterCombatVictory`      | `Task AfterCombatVictory(CombatRoom room)`      | 战斗胜利              |

### 6.3 回合生命周期

| 钩子                          | 签名                                                                       | 说明              |
| --------------------------- | ------------------------------------------------------------------------ | --------------- |
| `BeforeSideTurnStart`       | `Task BeforeSideTurnStart(PlayerChoiceContext, CombatSide, CombatState)` | 侧回合开始前（用于重置计数器） |
| `AfterSideTurnStart`        | `Task AfterSideTurnStart(CombatSide, CombatState)`                       | 侧回合开始后          |
| `AfterSideTurnStartLate`    | `Task AfterSideTurnStartLate(CombatSide, CombatState)`                   | 侧回合开始后（延迟）      |
| `BeforeTurnEndVeryEarly`    | `Task BeforeTurnEndVeryEarly(PlayerChoiceContext, CombatSide)`           | 回合结束前（极早期）      |
| `BeforeTurnEnd`             | `Task BeforeTurnEnd(PlayerChoiceContext, CombatSide)`                    | 回合结束前           |
| `AfterTurnEnd`              | `Task AfterTurnEnd(PlayerChoiceContext, CombatSide)`                     | 回合结束后           |
| `AfterTurnEndLate`          | `Task AfterTurnEndLate(PlayerChoiceContext, CombatSide)`                 | 回合结束后（延迟）       |
| `AfterPlayerTurnStart`      | `Task AfterPlayerTurnStart(PlayerChoiceContext, Player)`                 | 玩家回合开始          |
| `AfterPlayerTurnStartEarly` | `Task AfterPlayerTurnStartEarly(PlayerChoiceContext, Player)`            | 玩家回合开始（早期）      |
| `AfterPlayerTurnStartLate`  | `Task AfterPlayerTurnStartLate(PlayerChoiceContext, Player)`             | 玩家回合开始（延迟）      |

### 6.4 卡牌相关钩子

| 钩子                          | 签名                                                                               | 说明        |
| --------------------------- | -------------------------------------------------------------------------------- | --------- |
| `BeforeCardPlayed`          | `Task BeforeCardPlayed(CardPlay)`                                                | 卡牌打出前     |
| `AfterCardPlayed`           | `Task AfterCardPlayed(PlayerChoiceContext, CardPlay)`                            | 卡牌打出后     |
| `AfterCardPlayedLate`       | `Task AfterCardPlayedLate(PlayerChoiceContext, CardPlay)`                        | 卡牌打出后（延迟） |
| `BeforeHandDraw`            | `Task BeforeHandDraw(Player, PlayerChoiceContext, CombatState)`                  | 抽牌前       |
| `BeforeHandDrawLate`        | `Task BeforeHandDrawLate(Player, PlayerChoiceContext, CombatState)`              | 抽牌前（延迟）   |
| `AfterHandEmptied`          | `Task AfterHandEmptied(PlayerChoiceContext, Player)`                             | 手牌清空时     |
| `AfterCardDrawn`            | `Task AfterCardDrawn(PlayerChoiceContext, CardModel, bool fromHandDraw)`         | 抽牌后       |
| `AfterCardDrawnEarly`       | `Task AfterCardDrawnEarly(PlayerChoiceContext, CardModel, bool fromHandDraw)`    | 抽牌后（早期）   |
| `AfterCardExhausted`        | `Task AfterCardExhausted(PlayerChoiceContext, CardModel, bool causedByEthereal)` | 卡牌耗尽时     |
| `AfterCardDiscarded`        | `Task AfterCardDiscarded(PlayerChoiceContext, CardModel)`                        | 卡牌丢弃时     |
| `BeforeFlush`               | `Task BeforeFlush(PlayerChoiceContext, Player)`                                  | 手牌刷新前     |
| `BeforeFlushLate`           | `Task BeforeFlushLate(PlayerChoiceContext, Player)`                              | 手牌刷新前（延迟） |
| `AfterCardChangedPiles`     | `Task AfterCardChangedPiles(CardModel, PileType, AbstractModel?)`                | 卡牌改变牌堆后   |
| `AfterCardChangedPilesLate` | `Task AfterCardChangedPilesLate(CardModel, PileType, AbstractModel?)`            | 改变牌堆后（延迟） |

### 6.5 伤害/格挡钩子

| 钩子                        | 签名                                                                                                            | 说明        |
| ------------------------- | ------------------------------------------------------------------------------------------------------------- | --------- |
| `BeforeDamageReceived`    | `Task BeforeDamageReceived(PlayerChoiceContext, Creature, decimal, ValueProp, Creature?, CardModel?)`         | 受到伤害前     |
| `AfterDamageReceived`     | `Task AfterDamageReceived(PlayerChoiceContext, Creature, DamageResult, ValueProp, Creature?, CardModel?)`     | 受到伤害后     |
| `AfterDamageReceivedLate` | `Task AfterDamageReceivedLate(PlayerChoiceContext, Creature, DamageResult, ValueProp, Creature?, CardModel?)` | 受到伤害后（延迟） |
| `AfterDamageGiven`        | `Task AfterDamageGiven(PlayerChoiceContext, Creature?, DamageResult, ValueProp, Creature, CardModel?)`        | 造成伤害后     |
| `AfterBlockGained`        | `Task AfterBlockGained(Creature, decimal, ValueProp, CardModel?)`                                             | 获得格挡后     |
| `AfterBlockBroken`        | `Task AfterBlockBroken(Creature)`                                                                             | 格挡被击破后    |
| `AfterCurrentHpChanged`   | `Task AfterCurrentHpChanged(Creature, decimal)`                                                               | HP 变化后    |

### 6.6 死亡相关钩子

| 钩子                     | 签名                                                            | 说明                     |
| ---------------------- | ------------------------------------------------------------- | ---------------------- |
| `BeforeDeath`          | `Task BeforeDeath(Creature)`                                  | 死亡前                    |
| `AfterDeath`           | `Task AfterDeath(PlayerChoiceContext, Creature, bool, float)` | 死亡后                    |
| `AfterPreventingDeath` | `Task AfterPreventingDeath(Creature)`                         | 防止死亡后（LizardTail 在此触发） |
| `ShouldDieLate`        | `bool ShouldDieLate(Creature)`                                | 是否应该死亡（延迟判断）           |

### 6.7 房间/地图钩子

| 钩子                           | 签名                                          | 说明        |
| ---------------------------- | ------------------------------------------- | --------- |
| `BeforeRoomEntered`          | `Task BeforeRoomEntered(AbstractRoom)`      | 进入房间前     |
| `AfterRoomEntered`           | `Task AfterRoomEntered(AbstractRoom)`       | 进入房间后     |
| `AfterActEntered`            | `Task AfterActEntered()`                    | 进入新 Act 后 |
| `AfterCreatureAddedToCombat` | `Task AfterCreatureAddedToCombat(Creature)` | 生物加入战斗后   |

### 6.8 其他钩子

| 钩子                   | 签名                                                                          | 说明       |
| -------------------- | --------------------------------------------------------------------------- | -------- |
| `AfterGoldGained`    | `Task AfterGoldGained(Player)`                                              | 获得金币后    |
| `AfterItemPurchased` | `Task AfterItemPurchased(Player, MerchantEntry, int)`                       | 购买物品后    |
| `AfterRewardTaken`   | `Task AfterRewardTaken(Player, Reward)`                                     | 领取奖励后    |
| `AfterPotionUsed`    | `Task AfterPotionUsed(PotionModel, Creature?)`                              | 药水使用后    |
| `AfterShuffle`       | `Task AfterShuffle(PlayerChoiceContext, Player)`                            | 洗牌后      |
| `AfterStarsGained`   | `Task AfterStarsGained(int, Player)`                                        | 获得星星后    |
| `AfterStarsSpent`    | `Task AfterStarsSpent(int, Player)`                                         | 花费星星后    |
| `AfterOrbEvoked`     | `Task AfterOrbEvoked(PlayerChoiceContext, OrbModel, IEnumerable<Creature>)` | Orb 被激发后 |

***

## 7. 数值修正/修饰方法

这些方法返回值会被累加/连锁处理（多个遗物/效果可以叠加）。

### 7.1 伤害修正

| 方法                           | 签名                                                              | 说明     | 示例                             |
| ---------------------------- | --------------------------------------------------------------- | ------ | ------------------------------ |
| `ModifyDamageAdditive`       | `decimal(Creature?, decimal, ValueProp, Creature?, CardModel?)` | 伤害加法修正 | `StrikeDummy` +3 对 Strike 标签卡牌 |
| `ModifyDamageMultiplicative` | `decimal(Creature?, decimal, ValueProp, Creature?, CardModel?)` | 伤害乘法修正 | `PenNib` 2x 每第 10 次攻击          |

```csharp
// StrikeDummy - ModifyDamageAdditive
public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
{
    if (!props.IsPoweredAttack()) return 0m;
    if (cardSource == null) return 0m;
    if (!cardSource.Tags.Contains(CardTag.Strike)) return 0m;
    if (dealer != base.Owner.Creature && cardSource.Owner != base.Owner) return 0m;
    return base.DynamicVars["ExtraDamage"].BaseValue;
}

// PenNib - ModifyDamageMultiplicative
public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
{
    if (!props.IsPoweredAttack()) return 1m;
    if (cardSource == null) return 1m;
    if (dealer != base.Owner.Creature && dealer != base.Owner.Osty) return 1m;
    if (AttackToDouble == null && AttacksPlayed == 9) return 2m;
    if (cardSource == AttackToDouble) return 2m;
    return 1m;
}
```

### 7.2 HP 损失修正

| 方法                               | 签名                                                             | 说明            | 示例                      |
| -------------------------------- | -------------------------------------------------------------- | ------------- | ----------------------- |
| `ModifyHpLostAfterOsty`          | `decimal(Creature, decimal, ValueProp, Creature?, CardModel?)` | HP损失修正（Osty后） | `TungstenRod` -1（最小值 0） |
| `ModifyHpLostBeforeOsty`         | `decimal(Creature, decimal, ValueProp, Creature?, CardModel?)` | HP损失修正（Osty前） | `TheBoot` 最小伤害提升到 5     |
| `AfterModifyingHpLostAfterOsty`  | `Task()`                                                       | HP损失修正后回调     | `TungstenRod` Flash     |
| `AfterModifyingHpLostBeforeOsty` | `Task()`                                                       | HP损失修正前回调     | `TheBoot` Flash         |

```csharp
// TungstenRod - ModifyHpLostAfterOsty
public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
{
    if (target != base.Owner.Creature) return amount;
    return Math.Max(0m, amount - base.DynamicVars["HpLossReduction"].BaseValue);
}

// TheBoot - ModifyHpLostBeforeOsty
public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
{
    if (dealer != base.Owner.Creature) return amount;
    if (!props.IsPoweredAttack()) return amount;
    if (amount < 1m) return amount;
    if (amount >= base.DynamicVars["DamageMinimum"].BaseValue) return amount;
    return base.DynamicVars["DamageMinimum"].BaseValue;
}
```

### 7.3 能量修正

| 方法                | 签名                         | 说明     | 示例                                                       |
| ----------------- | -------------------------- | ------ | -------------------------------------------------------- |
| `ModifyMaxEnergy` | `decimal(Player, decimal)` | 修改最大能量 | `Sozu`, `Ectoplasm`, `PhilosophersStone`, `VelvetChoker` |

```csharp
public override decimal ModifyMaxEnergy(Player player, decimal amount)
{
    if (player != base.Owner) return amount;
    return amount + base.DynamicVars.Energy.BaseValue;
}
```

### 7.4 抽牌修正

| 方法                       | 签名                         | 说明       | 示例                                             |
| ------------------------ | -------------------------- | -------- | ---------------------------------------------- |
| `ModifyHandDraw`         | `decimal(Player, decimal)` | 修改抽牌数    | `BagOfPreparation`, `SneckoEye`, `Pocketwatch` |
| `AfterModifyingHandDraw` | `Task()`                   | 抽牌数修改后回调 | `Pocketwatch` Flash                            |

```csharp
// BagOfPreparation
public override decimal ModifyHandDraw(Player player, decimal count)
{
    if (player != base.Owner) return count;
    if (player.Creature.CombatState.RoundNumber > 1) return count;
    return count + base.DynamicVars.Cards.BaseValue;
}
```

### 7.5 其他修正

| 方法                           | 签名                                                              | 说明             | 示例                  |
| ---------------------------- | --------------------------------------------------------------- | -------------- | ------------------- |
| `ModifyMerchantPrice`        | `decimal(Player, MerchantEntry, decimal)`                       | 修改商店价格         | `MembershipCard` 半价 |
| `ModifyXValue`               | `int(CardModel, int)`                                           | 修改 X 费用数值      | `ChemicalX` +2      |
| `ModifyPowerAmountGiven`     | `decimal(PowerModel, Creature, decimal, Creature?, CardModel?)` | 修改给予的 Power 数值 | `SneckoSkull` 毒 +1  |
| `ModifyVulnerableMultiplier` | `decimal(Creature, decimal, ValueProp, Creature?, CardModel?)`  | 修改易伤倍率         | `PaperPhrog` +0.25  |
| `ModifyWeakMultiplier`       | `decimal(Creature, decimal, ValueProp, Creature?, CardModel?)`  | 修改虚弱倍率         | `PaperKrane` -0.15  |

```csharp
// MembershipCard
public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal originalPrice)
{
    if (player != base.Owner) return originalPrice;
    return originalPrice * (base.DynamicVars["Discount"].BaseValue / 100m); // 50%
}

// ChemicalX
public override int ModifyXValue(CardModel card, int originalValue)
{
    if (base.Owner != card.Owner) return originalValue;
    return originalValue + base.DynamicVars["Increase"].IntValue;
}

// SneckoSkull
public override decimal ModifyPowerAmountGiven(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
{
    if (!(power is PoisonPower)) return amount;
    if (giver != base.Owner.Creature) return amount;
    return amount + base.DynamicVars.Poison.IntValue;
}
```

***

## 8. 条件/判断方法

这些方法返回 `bool`，用于控制游戏逻辑是否执行。

| 方法                        | 签名                              | 说明                       | 示例                           |
| ------------------------- | ------------------------------- | ------------------------ | ---------------------------- |
| `ShouldFlush`             | `bool(Player)`                  | 回合结束时是否清空手牌              | `RunicPyramid` 返回 false（不清空） |
| `ShouldPlayerResetEnergy` | `bool(Player)`                  | 是否重置能量                   | `IceCream` 仅第 1 回合重置         |
| `ShouldPlay`              | `bool(CardModel, AutoPlayType)` | 是否允许打出某卡牌                | `VelvetChoker` 超 6 张禁止       |
| `ShouldProcurePotion`     | `bool(PotionModel, Player)`     | 是否允许获得药水                 | `Sozu` 返回 false（禁止药水）        |
| `ShouldGainGold`          | `bool(decimal, Player)`         | 是否允许获得金币                 | `Ectoplasm` 返回 false（禁止金币）   |
| `ShouldDieLate`           | `bool(Creature)`                | 是否应该死亡                   | `LizardTail` 防止死亡            |
| `ShouldClearBlock`        | `bool(Creature)`                | 是否清空格挡（来自 Power，遗物用同名方法） | 通常由 Power 实现                 |

```csharp
// RunicPyramid - 不清空手牌
public override bool ShouldFlush(Player player)
{
    if (player != base.Owner) return true;
    return false;
}

// IceCream - 仅第1回合重置能量（之后能量保留）
public override bool ShouldPlayerResetEnergy(Player player)
{
    if (player.Creature.CombatState.RoundNumber == 1) return true;
    if (player != base.Owner) return true;
    return false;
}

// VelvetChoker - 限制每回合出牌数
public override bool ShouldPlay(CardModel card, AutoPlayType _)
{
    if (card.Owner != base.Owner) return true;
    return !ShouldPreventCardPlay;  // _cardsPlayedThisTurn >= 6 时禁止
}

// LizardTail - 防止死亡
public override bool ShouldDieLate(Creature creature)
{
    if (creature != base.Owner.Creature) return true;
    if (WasUsed) return true;
    return false;  // 阻止死亡，触发 AfterPreventingDeath
}
```

***

## 9. 奖励与商店方法

| 方法                     | 签名                                          | 说明       | 示例                      |
| ---------------------- | ------------------------------------------- | -------- | ----------------------- |
| `TryModifyRewards`     | `bool(Player, List<Reward>, AbstractRoom?)` | 尝试修改奖励列表 | `BlackStar` 精英战多加一个遗物奖励 |
| `TryModifyRewardsLate` | `bool(Player, List<Reward>, AbstractRoom?)` | 延迟修改奖励   | 部分特殊遗物                  |

```csharp
// BlackStar - 精英战额外遗物奖励
public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
{
    if (player != base.Owner) return false;
    if (room == null || room.RoomType != RoomType.Elite) return false;
    rewards.Add(new RelicReward(player));
    return true;
}
```

***

## 10. SavedProperty 持久化模式

使用 `[SavedProperty]` 标记需要跨存档/读档保存的字段。

```csharp
[SavedProperty]
public int AttacksPlayed
{
    get => _attacksPlayed;
    set
    {
        AssertMutable();
        _attacksPlayed = value;
        UpdateDisplay();
    }
}

[SavedProperty]
public bool WasUsed
{
    get => _wasUsed;
    set
    {
        AssertMutable();
        _wasUsed = value;
        if (IsUsedUp)
            base.Status = RelicStatus.Disabled;
    }
}

[SavedProperty]
public int TurnsSeen
{
    get => _turnsSeen;
    set
    {
        AssertMutable();
        _turnsSeen = value;
        InvokeDisplayAmountChanged();
    }
}
```

### 属性更新三原则

在 `setter` 中始终遵循：

1. 调用 `AssertMutable()` 确保处于可修改状态
2. 更新后备字段
3. 调用 `UpdateDisplay()` / `InvokeDisplayAmountChanged()` 更新 UI

***

## 11. 显示与计数器系统

### 11.1 ShowCounter 与 DisplayAmount

```csharp
public override bool ShowCounter => true;                     // 显示计数器
public override bool ShowCounter => CombatManager.Instance.IsInProgress;  // 仅战斗中显示
public override int DisplayAmount => AttacksPlayed % 10;      // 当前计数
public override int DisplayAmount => _cardsPlayedThisTurn;     // 本回合出牌数
```

### 11.2 Status 与计数联动

```csharp
// Nunchaku 模式：到达阈值时脉冲发光
private void UpdateDisplay()
{
    if (IsActivating)
    {
        base.Status = RelicStatus.Normal;
    }
    else
    {
        int threshold = base.DynamicVars.Cards.IntValue;
        base.Status = (AttacksPlayed % threshold == threshold - 1)
            ? RelicStatus.Active
            : RelicStatus.Normal;
    }
    InvokeDisplayAmountChanged();
}

// 激活动画模式
private bool IsActivating
{
    get => _isActivating;
    set
    {
        AssertMutable();
        _isActivating = value;
        UpdateDisplay();
    }
}

private async Task DoActivateVisuals()
{
    IsActivating = true;
    Flash();
    await Cmd.Wait(1f);
    IsActivating = false;
}
```

### 11.3 战斗中状态初始化

```csharp
// 战斗开始时重置
public override Task BeforeCombatStart()
{
    SkillsPlayedThisTurn = 0;
    base.Status = RelicStatus.Normal;
    return Task.CompletedTask;
}

// 新回合开始前重置
public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
{
    if (side != base.Owner.Creature.Side) return Task.CompletedTask;
    _cardsPlayedThisTurn = 0;
    InvokeDisplayAmountChanged();
    return Task.CompletedTask;
}

// 战斗结束时重置
public override Task AfterCombatEnd(CombatRoom _)
{
    _cardsPlayedThisTurn = 0;
    base.Status = RelicStatus.Normal;
    InvokeDisplayAmountChanged();
    return Task.CompletedTask;
}
```

***

## 12. Relic 指令

| 方法                             | 签名                            | 说明                         |
| ------------------------------ | ----------------------------- | -------------------------- |
| `Flash()`                      | `void()`                      | 播放遗物闪烁动画（目标为 Owner）        |
| `Flash(IEnumerable<Creature>)` | `void(IEnumerable<Creature>)` | 播放遗物闪烁动画（指定目标）             |
| `InvokeDisplayAmountChanged()` | `void()`                      | 触发 DisplayAmount 更新通知      |
| `IncrementStackCount()`        | `void()`                      | 增加堆叠计数（仅 IsStackable=true） |

### 常用的非 Relic 指令（在遗物中使用）

```csharp
// Power 相关
await PowerCmd.Apply<StrengthPower>(owner.Creature, amount, owner.Creature, null);
await PowerCmd.Apply<VulnerablePower>(enemies, amount, owner.Creature, null);
await PowerCmd.Apply<ThornsPower>(owner.Creature, amount, owner.Creature, null);
await PowerCmd.Apply<ConfusedPower>(owner.Creature, 1m, owner.Creature, null);

// Creature 相关
await CreatureCmd.GainBlock(owner.Creature, amount, null);
await CreatureCmd.Heal(owner.Creature, amount);
await CreatureCmd.GainMaxHp(owner.Creature, amount);
await CreatureCmd.Damage(context, targets, damageValue, dealer);
await CreatureCmd.Damage(context, targets, damageVar, props, dealer, cardSource);

// Player 相关
await PlayerCmd.GainEnergy(amount, owner);
await PlayerCmd.GainGold(amount, owner);

// CardPile 相关
await CardPileCmd.Draw(context, owner);
await CardPileCmd.Draw(context, count, owner);
await CardPileCmd.AddCurseToDeck<CurseOfTheBell>(owner);
await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
await CardPileCmd.RemoveFromDeck(card);

// Card 相关
await CardCmd.DiscardAndDraw(context, cards, drawCount);
CardModel clone = owner.RunState.CloneCard(cardModel);
await CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(clone, PileType.Deck));

// CardSelect 相关
List<CardModel> cards = await CardSelectCmd.FromHandForDiscard(context, owner, prefs, null, this);
CardModel chosen = await CardSelectCmd.FromChooseACardScreen(context, cards, owner);
List<CardModel> removed = await CardSelectCmd.FromDeckForRemoval(prefs, owner);
CardModel? card = await CardSelectCmd.FromDeckGeneric(prefs, owner, filter);

// Vfx 相关
VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_attack_blunt");

// Orb 相关
await OrbCmd.Channel<DarkOrb>(context, owner);
await OrbCmd.Passive(context, orb, null);

// Rewards 相关
await RewardsCmd.OfferCustom(owner, rewards);

// 等待
await Cmd.Wait(0.75f);
await Cmd.Wait(1f);
```

***

## 13. 完整 Relic 示例

### 示例 1：Anchor —— 最简单的战斗开始格挡遗物

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Anchor : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
            new BlockVar(10m, ValueProp.Unpowered)
        );

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(
            HoverTipFactory.Static(StaticHoverTip.Block)
        );

    public override async Task BeforeCombatStart()
    {
        Flash();
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, null);
    }
}
```

### 示例 2：BurningBlood —— 初始遗物（战斗胜利回血）

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BurningBlood : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
            new HealVar(6m)
        );

    public override async Task AfterCombatVictory(CombatRoom _)
    {
        if (!base.Owner.Creature.IsDead)
        {
            Flash();
            await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue);
        }
    }
}
```

### 示例 3：Vajra —— 进入战斗加力量

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Vajra : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
            new PowerVar<StrengthPower>(1m)
        );

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(
            HoverTipFactory.FromPower<StrengthPower>()
        );

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(
                base.Owner.Creature,
                base.DynamicVars.Strength.BaseValue,
                base.Owner.Creature,
                null
            );
        }
    }
}
```

### 示例 4：BagOfMarbles —— 第 1 回合敌人全体易伤

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BagOfMarbles : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
            new PowerVar<VulnerablePower>(1m)
        );

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(
            HoverTipFactory.FromPower<VulnerablePower>()
        );

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState)
    {
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<VulnerablePower>(
                combatState.HittableEnemies,
                base.DynamicVars.Vulnerable.BaseValue,
                base.Owner.Creature,
                null
            );
        }
    }
}
```

### 示例 5：Nunchaku —— 计数器 + SavedProperty + Active 状态

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Nunchaku : RelicModel
{
    private bool _isActivating;
    private int _attacksPlayed;

    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override bool ShowCounter => true;

    public override int DisplayAmount
    {
        get
        {
            if (!IsActivating)
                return AttacksPlayed % base.DynamicVars.Cards.IntValue;
            return base.DynamicVars.Cards.IntValue;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
        {
            new CardsVar(10),
            new EnergyVar(1)
        });

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(
            HoverTipFactory.ForEnergy(this)
        );

    private bool IsActivating
    {
        get => _isActivating;
        set
        {
            AssertMutable();
            _isActivating = value;
            UpdateDisplay();
        }
    }

    [SavedProperty]
    public int AttacksPlayed
    {
        get => _attacksPlayed;
        set
        {
            AssertMutable();
            _attacksPlayed = value;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (IsActivating)
        {
            base.Status = RelicStatus.Normal;
        }
        else
        {
            int threshold = base.DynamicVars.Cards.IntValue;
            base.Status = (AttacksPlayed % threshold == threshold - 1)
                ? RelicStatus.Active
                : RelicStatus.Normal;
        }
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == base.Owner && cardPlay.Card.Type == CardType.Attack)
        {
            AttacksPlayed++;
            int threshold = base.DynamicVars.Cards.IntValue;
            if (CombatManager.Instance.IsInProgress && AttacksPlayed % threshold == 0)
            {
                TaskHelper.RunSafely(DoActivateVisuals());
                await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
            }
        }
    }

    private async Task DoActivateVisuals()
    {
        IsActivating = true;
        Flash();
        await Cmd.Wait(1f);
        IsActivating = false;
    }
}
```

### 示例 6：StrikeDummy —— ModifyDamageAdditive（条件性伤害加成）

```csharp
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class StrikeDummy : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
            new DynamicVar("ExtraDamage", 3m)
        );

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 0m;
        if (cardSource == null) return 0m;
        if (!cardSource.Tags.Contains(CardTag.Strike)) return 0m;
        if (dealer != base.Owner.Creature && cardSource.Owner != base.Owner) return 0m;
        return base.DynamicVars["ExtraDamage"].BaseValue;
    }
}
```

### 示例 7：BagOfPreparation —— ModifyHandDraw（首回合额外抽牌）

```csharp
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BagOfPreparation : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
            new CardsVar(2)
        );

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner) return count;
        if (player.Creature.CombatState.RoundNumber > 1) return count;
        return count + base.DynamicVars.Cards.BaseValue;
    }
}
```

### 示例 8：RunicPyramid —— ShouldFlush（不清空手牌）

```csharp
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class RunicPyramid : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool ShouldFlush(Player player)
    {
        if (player != base.Owner) return true;
        return false;
    }
}
```

### 示例 9：PaperPhrog —— ModifyVulnerableMultiplier（易伤倍率增强）

```csharp
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class PaperPhrog : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new _003C_003Ez__ReadOnlySingleElementList<IHoverTip>(
            HoverTipFactory.FromPower<VulnerablePower>()
        );

    public decimal ModifyVulnerableMultiplier(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == base.Owner.Creature) return amount;
        if (!props.IsPoweredAttack()) return amount;
        return amount + 0.25m;
    }
}
```

### 示例 10：LizardTail —— IsUsedUp + AfterPreventingDeath（防止死亡）

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class LizardTail : RelicModel
{
    private bool _wasUsed;

    public override RelicRarity Rarity => RelicRarity.Rare;
    public override bool IsUsedUp => _wasUsed;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlySingleElementList<DynamicVar>(
            new HealVar(50m)
        );

    [SavedProperty]
    public bool WasUsed
    {
        get => _wasUsed;
        set
        {
            AssertMutable();
            _wasUsed = value;
            if (IsUsedUp)
                base.Status = RelicStatus.Disabled;
        }
    }

    public override bool ShouldDieLate(Creature creature)
    {
        if (creature != base.Owner.Creature) return true;
        if (WasUsed) return true;
        return false;   // 阻止死亡
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        WasUsed = true;
        decimal healAmount = Math.Max(
            1m,
            (decimal)creature.MaxHp * (base.DynamicVars.Heal.BaseValue / 100m)
        );
        await CreatureCmd.Heal(creature, healAmount);
    }
}
```

### 示例 11：Pocketwatch —— 条件抽牌 + 战斗内重置

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Pocketwatch : RelicModel
{
    private int _cardsPlayedThisTurn;
    private int _cardsPlayedLastTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;
    public override bool ShowCounter => CombatManager.Instance.IsInProgress;
    public override int DisplayAmount => _cardsPlayedThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new _003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
        {
            new DynamicVar("CardThreshold", 3m),
            new CardsVar(3)
        });

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner) return Task.CompletedTask;
        if (!CombatManager.Instance.IsInProgress) return Task.CompletedTask;
        _cardsPlayedThisTurn++;
        RefreshCounter();
        return Task.CompletedTask;
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player.Creature.CombatState.RoundNumber == 1) return count;
        if (player != base.Owner) return count;
        if ((decimal)_cardsPlayedLastTurn > base.DynamicVars["CardThreshold"].BaseValue)
            return count;
        return count + base.DynamicVars.Cards.BaseValue;
    }

    public override Task AfterModifyingHandDraw()
    {
        Flash();
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        CombatState combatState)
    {
        if (side == base.Owner.Creature.Side)
        {
            _cardsPlayedLastTurn = _cardsPlayedThisTurn;
            _cardsPlayedThisTurn = 0;
        }
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == base.Owner.Creature.Side)
            RefreshCounter();
        return Task.CompletedTask;
    }

    private void RefreshCounter()
    {
        base.Status = ((decimal)_cardsPlayedThisTurn <= base.DynamicVars["CardThreshold"].BaseValue)
            ? RelicStatus.Active
            : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterCombatEnd(CombatRoom _)
    {
        _cardsPlayedThisTurn = 0;
        _cardsPlayedLastTurn = 0;
        base.Status = RelicStatus.Normal;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}
```

***

## 14. 附录：常用命名空间

```csharp
using MegaCrit.Sts2.Core.Combat;                    // CombatState, CombatSide, CombatManager
using MegaCrit.Sts2.Core.Commands;                   // PowerCmd, CreatureCmd, PlayerCmd, CardCmd, VfxCmd
using MegaCrit.Sts2.Core.Entities.Cards;              // CardModel, CardPlay, CardType, CardTag, PileType
using MegaCrit.Sts2.Core.Entities.Creatures;          // Creature, DamageResult
using MegaCrit.Sts2.Core.Entities.Players;            // Player
using MegaCrit.Sts2.Core.Entities.Relics;             // RelicRarity, RelicStatus
using MegaCrit.Sts2.Core.HoverTips;                   // HoverTipFactory, IHoverTip, StaticHoverTip
using MegaCrit.Sts2.Core.Localization.DynamicVars;    // DynamicVar, PowerVar, BlockVar, HealVar, etc.
using MegaCrit.Sts2.Core.ValueProps;                  // ValueProp
using MegaCrit.Sts2.Core.Models.Powers;               // StrengthPower, VulnerabilityPower, etc.
using MegaCrit.Sts2.Core.Rooms;                       // AbstractRoom, CombatRoom
using MegaCrit.Sts2.Core.GameActions.Multiplayer;     // CardPileCmd, PlayerCmd, CreatureCmd
using MegaCrit.Sts2.Core.Helpers;                     // TaskHelper
using MegaCrit.Sts2.Core.Saves.Runs;                  // SavedProperty
using MegaCrit.Sts2.Core.Rewards;                     // RelicReward, Reward
using MegaCrit.Sts2.Core.CardSelection;               // CardSelectCmd, CardSelectorPrefs
using MegaCrit.Sts2.Core.Context;                     // LocalContext
using MegaCrit.Sts2.Core.TestSupport;                 // TestMode
```

***

> 本文档基于原版 `code/Relics/` 中的遗物代码与 `RelicModel` 基类整理。
>
> 关键文件参考：
>
> - `code/sts2/MegaCrit.Sts2.Core.Models/RelicModel.cs` (536行) —— 遗物基类
> - `code/sts2/MegaCrit.Sts2.Core.Entities.Relics/RelicRarity.cs` —— 稀有度枚举
> - `code/sts2/MegaCrit.Sts2.Core.Entities.Relics/RelicStatus.cs` —— 状态枚举
> - `code/sts2/MegaCrit.Sts2.Core.Hooks/Hook.cs` (1995行) —— 钩子分发器

