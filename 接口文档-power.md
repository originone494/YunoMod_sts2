# 杀戮尖塔2 Power Mod 开发接口文档

基于 `code/Powers/` 中原版参考 Power 整理。

---

## 目录

1. [PowerModel 基类](#1-powermodel-基类)
2. [核心属性/覆写项](#2-核心属性覆写项)
3. [Type 与 StackType](#3-type-与-stacktype)
4. [Amount 与显示系统](#4-amount-与显示系统)
5. [伤害修正管线](#5-伤害修正管线)
6. [格挡修正管线](#6-格挡修正管线)
7. [其他修正方法](#7-其他修正方法)
8. [生命周期钩子（按执行顺序）](#8-生命周期钩子按执行顺序)
9. [Power 指令 Commands](#9-power-指令-commands)
10. [内部数据模式 Internal Data](#10-内部数据模式-internal-data)
11. [DynamicVar 在 Power 中的使用](#11-dynamicvar-在-power-中的使用)
12. [HoverTip 模式](#12-hovertip-模式)
13. [完整 Power 示例](#13-完整-power-示例)
14. [附录：常用命名空间](#14-附录常用命名空间)

---

## 1. PowerModel 基类

所有 Power 继承自 `PowerModel`，位于命名空间 `MegaCrit.Sts2.Core.Models`。

```csharp
using MegaCrit.Sts2.Core.Models;

public sealed class ExamplePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
```

`PowerModel` 继承自 `AbstractModel`，提供以下核心成员：

| 成员 | 类型 | 说明 |
|------|------|------|
| `Owner` | `Creature` | Power 的持有者 |
| `Applier` | `Creature?` | Power 的来源（谁施加的） |
| `Target` | `Creature?` | Power 的目标 |
| `Amount` | `int` | 当前层数/数值 |
| `AmountOnTurnStart` | `int` | 回合开始时的数值 |
| `DynamicVars` | `DynamicVarSet` | 动态变量集 |
| `CombatState` | `CombatState` | 当前战斗状态 |
| `Type` | `PowerType` | Power 类型（Buff/Debuff） |
| `StackType` | `PowerStackType` | 堆叠方式（Counter/Single） |
| `Flash()` | `void` | 闪烁 Power 图标（受保护方法） |
| `InvokeDisplayAmountChanged()` | `void` | 触发显示值刷新 |

---

## 2. 核心属性/覆写项

### 基本属性

```csharp
// Power 类型：Buff（增益）/ Debuff（减益）
public override PowerType Type => PowerType.Buff;

// 堆叠方式：Counter（计数）/ Single（单层）
public override PowerStackType StackType => PowerStackType.Counter;
```

### 特殊属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `AllowNegative` | `bool` | `false` | 允许 Amount 为负数（StrengthPower、DexterityPower、FocusPower） |
| `DisplayAmount` | `int` | `Amount` | 覆盖显示值（SlowPower 显示 10 倍值） |
| `IsInstanced` | `bool` | `false` | 每个实例独立（NightmarePower、TheBombPower） |
| `ShouldScaleInMultiplayer` | `bool` | `false` | 多人模式缩放（RegenPower、ArtifactPower） |
| `ShouldPlayVfx` | `bool` | `true` | 是否播放 VFX |
| `OwnerIsSecondaryEnemy` | `bool` | `false` | 标记持有者为次要敌人（MinionPower） |
| `IsVisible` | `bool` | 自动 | 是否对当前玩家可见 |
| `SkipNextDurationTick` | `bool` | `false` | 跳过下次持续 tick |

### 使用示例

```csharp
// StrengthPower - 允许负数
public override bool AllowNegative => true;

// SlowPower - 自定义显示值（Amount * 10）
public override int DisplayAmount => base.DynamicVars["SlowAmount"].IntValue * 10;

// TheBombPower - 每个实例独立
public override bool IsInstanced => true;

// MinionPower - 不播放 VFX，标记为次要敌人
public override bool ShouldPlayVfx => false;
public override bool OwnerIsSecondaryEnemy => true;
```

---

## 3. Type 与 StackType

### PowerType

| 值 | 说明 | 示例 |
|----|------|------|
| `PowerType.Buff` | 增益效果 | StrengthPower, BarricadePower, VigorPower |
| `PowerType.Debuff` | 减益效果 | VulnerablePower, WeakPower, PoisonPower |

### PowerStackType

| 值 | 说明 | Amount 含义 | 示例 |
|----|------|-------------|------|
| `PowerStackType.Counter` | 计数堆叠 | 层数有意义，可叠加 | StrengthPower(3层+3力量), VulnerablePower(2层) |
| `PowerStackType.Single` | 单层 | 层数无意义(0或1) | BarricadePower, CorruptionPower, ConfusedPower |

### 颜色系统

Amount 标签颜色由 `AmountLabelColor` 属性控制，默认逻辑：

```csharp
public virtual Color AmountLabelColor
{
    get
    {
        if (GetTypeForAmount(Amount) != PowerType.Debuff)
            return _normalAmountLabelColor;  // StsColors.cream（奶油色）
        return _debuffAmountLabelColor;       // StsColors.red（红色）
    }
}
```

可手动覆写，如 PoisonPower：
```csharp
public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
```

---

## 4. Amount 与显示系统

### Amount 相关 API

| 成员 | 说明 |
|------|------|
| `Amount` | 当前层数（自动 clamp 到 -999999999 ~ 999999999） |
| `AmountOnTurnStart` | 回合开始时的层数快照 |
| `DisplayAmount` | 显示值（可覆写） |
| `TypeForCurrentAmount` | 根据当前 Amount 判断的 PowerType |
| `GetTypeForAmount(decimal)` | 根据指定数值判断 PowerType（会考虑 AllowNegative） |
| `ShouldRemoveDueToAmount()` | Amount 是否为 0（是否应移除） |

### 移除规则

```csharp
// 默认逻辑：Amount 为 0 时自动移除
// 但如果 AllowNegative=true，则 Amount == 0 时才移除
// 如果 Type=Debuff 且 Amount 变成负数（且 !AllowNegative），转为 Buff
public bool ShouldRemoveDueToAmount()
{
    if (AllowNegative || Amount > 0)
    {
        if (AllowNegative) return Amount == 0;
        return false;
    }
    return true;
}
```

---

## 5. 伤害修正管线

伤害计算按以下顺序经过多个修正阶段，每个阶段都可以被多个 Power 修改：

### 1. Additive 加性修正

```csharp
public virtual decimal ModifyDamageAdditive(
    Creature? target,      // 受伤目标
    decimal amount,        // 当前伤害值
    ValueProp props,       // 伤害属性
    Creature? dealer,      // 攻击者
    CardModel? cardSource  // 卡牌来源
)
```

- **StrengthPower**: 攻击者是自己且是 PoweredAttack 时，增加 `Amount` 伤害
- **VigorPower**: 攻击者是自己且是 PoweredAttack 时，增加当前 Vigor 层数
- **AccuracyPower**: 仅当卡牌有 `CardTag.Shiv` 时增加伤害
- **LeadershipPower**: 友方单位攻击时增加伤害

### 2. Multiplicative 乘性修正

```csharp
public virtual decimal ModifyDamageMultiplicative(
    Creature? target,      // 受伤目标
    decimal amount,        // 当前伤害值
    ValueProp props,       // 伤害属性
    Creature? dealer,      // 攻击者
    CardModel? cardSource  // 卡牌来源
)
```
返回值：倍率（1.0 = 不变）

- **VulnerablePower**: 目标是持有者时，返回 1.5（受到 1.5 倍伤害）
- **WeakPower**: 攻击者是持有者时，返回 0.75（造成 0.75 倍伤害）
- **DoubleDamagePower**: 攻击者是持有者时，返回 2.0
- **SlowPower**: 目标是持有者时，返回 `1 + 0.1 * SlowAmount`
- **ConquerorPower**: 根据卡牌类型返回倍率

### 3. Damage Cap 伤害上限

```csharp
public virtual decimal ModifyDamageCap(
    Creature? target,      // 受伤目标
    ValueProp props,       // 伤害属性
    Creature? dealer,      // 攻击者
    CardModel? cardSource  // 卡牌来源
)
```
返回值：最大允许伤害

- **IntangiblePower**: 目标是持有者时，返回 1（受到伤害不超过 1，有 TheBoot 时为 5）

### 4. HpLostAfterOsty OSTY 后生命损失修正

```csharp
public virtual decimal ModifyHpLostAfterOsty(
    Creature target,       // 受伤目标
    decimal amount,        // 当前损失 HP
    ValueProp props,       // 伤害属性
    Creature? dealer,      // 攻击者
    CardModel? cardSource  // 卡牌来源
)
```

- **IntangiblePower**: 经过伤害上限后，进一步限制 HP 损失

### 5. HpLostAfterOstyLate 后期修正

```csharp
public virtual decimal ModifyHpLostAfterOstyLate(
    Creature target,       // 受伤目标
    decimal amount,        // 当前损失 HP
    ValueProp props,       // 伤害属性
    Creature? dealer,      // 攻击者
    CardModel? cardSource  // 卡牌来源
)
```

- **BufferPower**: 目标是持有者时，返回 0（完全抵消伤害）

### 6. AfterModifyingHpLostAfterOsty 修正后回调

```csharp
public virtual async Task AfterModifyingHpLostAfterOsty()
```

- **IntangiblePower**: 调用 `Flash()` 闪烁图标
- **BufferPower**: 调用 `PowerCmd.Decrement(this)` 减少层数

### 7. AfterModifyingDamageAmount 伤害修正后回调

```csharp
public virtual Task AfterModifyingDamageAmount(CardModel? cardSource)
```

- **SlowPower**: 调用 `Flash()` 闪烁图标
- **IntangiblePower**: 调用 `Flash()` 闪烁图标

---

## 6. 格挡修正管线

### 1. Additive 加性修正

```csharp
public virtual decimal ModifyBlockAdditive(
    Creature target,        // 获得格挡的目标
    decimal block,          // 当前格挡值
    ValueProp props,        // 格挡属性
    CardModel? cardSource,  // 卡牌来源
    CardPlay? cardPlay      // 卡牌游玩上下文
)
```
返回值：加值（0 = 不修改）

- **DexterityPower**: 卡牌持有者是自己时，增加 `Amount` 格挡

### 2. Multiplicative 乘性修正

```csharp
public virtual decimal ModifyBlockMultiplicative(
    Creature target,        // 获得格挡的目标
    decimal block,          // 当前格挡值
    ValueProp props,        // 格挡属性
    CardModel? cardSource,  // 卡牌来源
    CardPlay? cardPlay      // 卡牌游玩上下文
)
```
返回值：倍率（1.0 = 不变）

- **FrailPower**: 目标是持有者时，返回 0.75（获得 0.75 倍格挡）

### 3. AfterModifyingBlockAmount 格挡修正后回调

```csharp
public virtual Task AfterModifyingBlockAmount(
    decimal modifiedBlock,
    CardModel? cardSource,
    CardPlay? cardPlay
)
```

---

## 7. 其他修正方法

### 抽牌修正

```csharp
public virtual decimal ModifyHandDraw(Player player, decimal originalCardCount)
```

- **DrawCardsNextTurnPower**: 增加抽牌数

### 能量修正

```csharp
public virtual bool TryModifyEnergyCostInCombat(
    CardModel card,       // 卡牌
    decimal originalCost, // 原始费用
    out decimal modifiedCost  // 修改后费用
)
```
返回 `true` 表示修改生效

- **CorruptionPower**: 技能牌费用变为 0

### 格挡清除控制

```csharp
public virtual bool ShouldClearBlock(Creature creature)
```
返回 `false` 表示不清除格挡

- **BarricadePower**: 持有者不清理格挡
- **BlurPower**: 持有者不清理格挡

### 卡牌游玩结果（使用后去向）

```csharp
public virtual (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
    CardModel card,
    bool isAutoPlay,
    ResourceInfo resources,
    PileType originalPileType,
    CardPilePosition position
)
```

- **CorruptionPower**: 技能牌改为进入弃牌堆（Exhaust → Discard）

### 卡牌复制次数

```csharp
public virtual int ModifyCardPlayCount(
    CardModel card,
    Creature? target,
    int playCount
)
```

- **DuplicationPower**: 修改卡牌游玩次数（+1）

### Orb 值修正

```csharp
public virtual decimal ModifyOrbValue(Player player, decimal value)
```

- **FocusPower**: 修改 Orb 相关数值

### 受到 Power 数值修正

```csharp
public virtual bool TryModifyPowerAmountReceived(
    PowerModel canonicalPower,  // Power 原型
    Creature target,            // 目标
    decimal amount,             // 原数值
    Creature? applier,          // 施加者
    out decimal modifiedAmount  // 修改后数值
)
```
返回 `true` 表示修改生效

- **ArtifactPower**: 如果是 Debuff 类型，`modifiedAmount = 0`（完全抵挡），然后 `AfterModifyingPowerAmountReceived` 中自减

### 卡牌自动打出

```csharp
// MayhemPower: 玩家回合开始时从抽牌堆自动打出
public override async Task AfterPlayerTurnStart(
    PlayerChoiceContext choiceContext,
    Player player
)
```

---

## 8. 生命周期钩子（按执行顺序）

### 灵魂绑定（静态成员）

```csharp
// 定义 Power 的规范变量
protected virtual IEnumerable<DynamicVar> CanonicalVars
    => Array.Empty<DynamicVar>();

// 定义额外的悬停提示
protected virtual IEnumerable<IHoverTip> ExtraHoverTips
    => Array.Empty<IHoverTip>();
```

### 应用与移除

#### AfterApplied — Power 被应用后

```csharp
public virtual Task AfterApplied(Creature? applier, CardModel? cardSource)
```

- **BarricadePower**: 记录施加者名字到 StringVar
- **RitualPower**: 初始化时无额外操作（通过其他钩子加力量）

#### AfterRemoved — Power 被移除后

```csharp
public virtual Task AfterRemoved(Creature oldOwner)
```

#### ShouldPowerBeRemovedAfterOwnerDeath

```csharp
public virtual bool ShouldPowerBeRemovedAfterOwnerDeath()
```
默认 `true`。MinionPower 覆写为 `false`。

#### ShouldOwnerDeathTriggerFatal

```csharp
public virtual bool ShouldOwnerDeathTriggerFatal()
```
默认 `true`。

### 卡牌相关

#### BeforeCardPlayed — 卡牌打出前

```csharp
public virtual Task BeforeCardPlayed(CardPlay cardPlay)
```

- **RupturePower**: 记录自伤卡牌信息
- **StormPower**: 记录法术牌计数

#### AfterCardPlayed — 卡牌打出后

```csharp
public virtual Task AfterCardPlayed(
    PlayerChoiceContext choiceContext,
    CardPlay cardPlay
)
```

- **RagePower**: 打出攻击牌后获得格挡
- **StormPower**: 打出法术牌后生成闪电
- **SlowPower**: 增加 SlowAmount 计数
- **JuggernautPower**: 获得格挡后对随机敌人造成伤害

#### AfterCardDrawn — 卡牌抽到后

```csharp
public virtual Task AfterCardDrawn(
    PlayerChoiceContext choiceContext,
    CardModel card,
    bool fromHandDraw
)
```

- **ConfusedPower**: 随机化卡牌费用

#### AfterCardExhausted — 卡牌耗尽后

```csharp
public virtual Task AfterCardExhausted(
    PlayerChoiceContext choiceContext,
    CardModel card,
    bool causedByEthereal
)
```

- **DarkEmbracePower**: 抽牌（若为灵体化则计数）
- **FeelNoPainPower**: 获得格挡

#### AfterCardDiscarded — 卡牌丢弃后

```csharp
public virtual Task AfterCardDiscarded(
    PlayerChoiceContext choiceContext,
    CardModel card
)
```

### 回合相关

#### BeforeTurnEnd — 回合结束前

```csharp
public virtual Task BeforeTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side
)
```

- **TheBombPower**: 炸弹倒计时，到 0 时爆炸

#### AfterTurnEnd — 回合结束后

```csharp
public virtual Task AfterTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side
)
```

- **VulnerablePower**: 敌方回合结束时 `TickDownDuration`
- **WeakPower**: 敌方回合结束时 `TickDownDuration`
- **IntangiblePower**: 敌方回合结束时 `TickDownDuration`
- **RegenPower**: 回血后递减
- **FlameBarrierPower**: 回合结束时移除
- **DarkEmbracePower**: 结算灵体化额外抽牌

#### AfterSideTurnStart — 某方回合开始时

```csharp
public virtual Task AfterSideTurnStart(
    CombatSide side,
    CombatState combatState
)
```

- **DemonFormPower**: 玩家回合开始时获得力量
- **PoisonPower**: 敌方回合开始时受到毒伤害
- **NoxiousFumesPower**: 己方回合开始时给全体敌人上毒
- **BlurPower**: 玩家回合开始时递减
- **RitualPower**: 回合开始时获得力量
- **SlowPower**: 敌方回合开始时重置 SlowAmount
- **WraithFormPower**: 回合开始时失去敏捷

#### AfterPlayerTurnStart — 玩家回合开始

```csharp
public virtual Task AfterPlayerTurnStart(
    PlayerChoiceContext choiceContext,
    Player player
)
```

- **MayhemPower**: 自动打出牌堆顶牌
- **LoopPower**: 触发 Orb 被动效果
- **PrepTimePower**: 获得 Vigor
- **ShadowStepPower**: 获得 DoubleDamage 后移除自身

#### BeforeHandDraw — 抽牌前

```csharp
public virtual Task BeforeHandDraw(
    Player player,
    PlayerChoiceContext choiceContext,
    CombatState combatState
)
```

- **InfiniteBladesPower**: 往手牌加入小刀
- **SentryModePower**: 往手牌加入 SweepingGaze
- **NightmarePower**: 往手牌加入梦魇复制牌

### 伤害相关

#### BeforeDamageReceived — 受到伤害前

```csharp
public virtual Task BeforeDamageReceived(
    PlayerChoiceContext choiceContext,
    Creature target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)
```

- **ThornsPower**: 反弹伤害给攻击者

#### AfterDamageReceived — 受到伤害后

```csharp
public virtual Task AfterDamageReceived(
    PlayerChoiceContext choiceContext,
    Creature target,
    DamageResult result,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)
```

- **FlameBarrierPower**: 火焰屏障反弹伤害
- **RupturePower**: 自伤后获得力量

#### AfterDamageGiven — 造成伤害后

```csharp
public virtual Task AfterDamageGiven(
    PlayerChoiceContext choiceContext,
    Creature? dealer,
    DamageResult results,
    ValueProp props,
    Creature target,
    CardModel? cardSource
)
```

- **EnvenomPower**: 上毒

#### AfterBlockGained — 获得格挡后

```csharp
public virtual Task AfterBlockGained(
    Creature creature,
    decimal amount,
    ValueProp props,
    CardModel? cardSource
)
```

- **JuggernautPower**: 对随机敌人造成伤害

### 其他回调

#### AfterPreventingBlockClear — 阻止格挡清除后

```csharp
public virtual Task AfterPreventingBlockClear(
    AbstractModel preventer,
    Creature creature
)
```

- **BlurPower**: 判断是否是自己阻止的清除，若是则 Flash

#### AfterPowerAmountChanged — Power 数值变化后

```csharp
public virtual Task AfterPowerAmountChanged(
    PowerModel power,
    decimal amount,
    Creature? applier,
    CardModel? cardSource
)
```

- **ShroudPower**: DoomPower 层数变化时获得格挡

#### AfterCurrentHpChanged — HP 变化后

```csharp
public virtual Task AfterCurrentHpChanged(
    Creature creature,
    decimal delta
)
```

- **NecroMasteryPower**: 监控 HP 变化

#### BeforeAttack / AfterAttack — 攻击前后

```csharp
public virtual Task BeforeAttack(AttackCommand command)
public virtual Task AfterAttack(AttackCommand command)
```

- **VigorPower**: 攻击前记录攻击命令和当前层数，攻击后按差值减少

#### BeforeFlush — 刷新前

```csharp
public virtual Task BeforeFlush(
    PlayerChoiceContext choiceContext,
    Player player
)
```

---

## 9. Power 指令 Commands

位于命名空间 `MegaCrit.Sts2.Core.Commands`：

### 核心指令

| 指令 | 说明 |
|------|------|
| `PowerCmd.Apply<TPower>(target, amount, applier, cardSource)` | 对目标施加 Power |
| `PowerCmd.Remove(power)` | 移除 Power |
| `PowerCmd.Decrement(power)` | 层数减 1 |
| `PowerCmd.TickDownDuration(power)` | 回合结束时递减（仅敌方） |
| `PowerCmd.ModifyAmount(power, delta, applier, cardSource)` | 修改层数（增减） |
| `PowerCmd.SetAmount(power, amount, applier, cardSource)` | 设置层数 |
| `PowerCmd.Flash(power)` | 闪烁图标 |

### 使用示例

```csharp
// 施加 Power
await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
await PowerCmd.Apply<PoisonPower>(combatState.HittableEnemies, Amount, Owner, null);

// 递减
await PowerCmd.Decrement(this);

// 回合结束时递减
await PowerCmd.TickDownDuration(this);

// 修改数值
await PowerCmd.ModifyAmount(this, -amountWhenAttackStarted, null, null);

// 移除
await PowerCmd.Remove(this);

// 闪烁（实例方法）
Flash();
```

### 获取 Power

```csharp
// 获取 Creature 上的 Power
StrengthPower? power = creature.GetPower<StrengthPower>();
int amount = creature.GetPowerAmount<StrengthPower>();
bool hasPower = creature.HasPower<StrengthPower>();
```

---

## 10. 内部数据模式 Internal Data

当 Power 需要在多个钩子之间共享状态时，使用私有嵌套类 `Data` + `InitInternalData` / `GetInternalData<T>` 模式。

### 标准模式

```csharp
public sealed class ExamplePower : PowerModel
{
    // 1. 定义私有 Data 类存储状态
    private class Data
    {
        public AttackCommand? commandToModify;
        public int amountWhenAttackStarted;
        public int etherealCount;
        public Dictionary<CardModel, int> playedCards = new();
    }

    // 2. 覆写 InitInternalData 初始化
    protected override object InitInternalData() => new Data();

    // 3. 使用 GetInternalData<Data>() 获取
    public override Task BeforeAttack(AttackCommand command)
    {
        var data = GetInternalData<Data>();
        data.commandToModify = command;
        data.amountWhenAttackStarted = Amount;
        return Task.CompletedTask;
    }

    public override async Task AfterAttack(AttackCommand command)
    {
        var data = GetInternalData<Data>();
        if (command == data.commandToModify)
            await PowerCmd.ModifyAmount(this, -data.amountWhenAttackStarted, null, null);
    }
}
```

### 真实示例

**VigorPower** — 跟踪当前攻击命令：

```csharp
private class Data
{
    public AttackCommand? commandToModify;
    public int amountWhenAttackStarted;
}

protected override object InitInternalData() => new Data();

public override Task BeforeAttack(AttackCommand command)
{
    if (command.Attacker != Owner) return Task.CompletedTask;
    if (!command.DamageProps.IsPoweredAttack()) return Task.CompletedTask;
    var data = GetInternalData<Data>();
    if (data.commandToModify != null) return Task.CompletedTask;
    data.commandToModify = command;
    data.amountWhenAttackStarted = Amount;
    return Task.CompletedTask;
}

public override decimal ModifyDamageAdditive(...)
{
    var data = GetInternalData<Data>();
    // 使用 data.commandToModify 判断
    return Amount;
}

public override async Task AfterAttack(AttackCommand command)
{
    var data = GetInternalData<Data>();
    if (command == data.commandToModify)
        await PowerCmd.ModifyAmount(this, -data.amountWhenAttackStarted, null, null);
}
```

**DarkEmbracePower** — 跟踪灵体化计数：

```csharp
private class Data
{
    public int etherealCount;
}

protected override object InitInternalData() => new Data();

public override async Task AfterCardExhausted(PlayerChoiceContext, CardModel card, bool causedByEthereal)
{
    if (causedByEthereal)
        GetInternalData<Data>().etherealCount++;
    else
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
}

public override async Task AfterTurnEnd(PlayerChoiceContext, CombatSide side)
{
    if (side == CombatSide.Player)
    {
        var data = GetInternalData<Data>();
        await CardPileCmd.Draw(choiceContext, Amount * data.etherealCount, Owner.Player);
        data.etherealCount = 0;
    }
}
```

**IsInstanced 模式** — TheBombPower（每个炸弹实例独立，无需 Data 类）：

```csharp
public override bool IsInstanced => true;  // 每个实例独立存储 DynamicVars

protected override IEnumerable<DynamicVar> CanonicalVars
    => [new DamageVar(40m, ValueProp.Unpowered)];

public void SetDamage(decimal damage)
{
    AssertMutable();
    DynamicVars.Damage.BaseValue = damage;
}
```

**NightmarePower** — IsInstanced + 自定义设置方法：

```csharp
// 需要 IsInstanced 确保每个实例有独立的 selectedCard
public void SetSelectedCard(CardModel card)
{
    AssertMutable();
    ((StringVar)DynamicVars["Card"]).StringValue = card.Id.Entry;
}
```

---

## 11. DynamicVar 在 Power 中的使用

### DynamicVar

```csharp
// 带默认值的命名变量
protected override IEnumerable<DynamicVar> CanonicalVars
    => [new DynamicVar("SlowAmount", 0m)];

// 访问
DynamicVars["SlowAmount"].BaseValue++;        // 修改值
DynamicVars["SlowAmount"].IntValue            // 获取整数值
DynamicVars["SlowAmount"].BaseValue           // 获取十进制值
```

### DamageVar

```csharp
// 伤害变量（带 ValueProp）
protected override IEnumerable<DynamicVar> CanonicalVars
    => [new DamageVar(40m, ValueProp.Unpowered)];

// 访问
DynamicVars.Damage.BaseValue = damage;
// 或
DynamicVars["Damage"].BaseValue
```

### StringVar

```csharp
// 字符串变量
protected override IEnumerable<DynamicVar> CanonicalVars
    => [new StringVar("ApplierName")];

// 访问
((StringVar)DynamicVars["ApplierName"]).StringValue = applier.Monster.Title.GetFormattedText();
```

### 完整示例

```csharp
// SlowPower - 使用命名 DynamicVar
private const string _slowAmountKey = "SlowAmount";

public override int DisplayAmount => DynamicVars["SlowAmount"].IntValue * 10;

protected override IEnumerable<DynamicVar> CanonicalVars
    => [new DynamicVar("SlowAmount", 0m)];

public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
{
    DynamicVars["SlowAmount"].BaseValue++;
    InvokeDisplayAmountChanged();
    return Task.CompletedTask;
}

public override decimal ModifyDamageMultiplicative(...)
{
    if (target != Owner) return 1m;
    if (!props.IsPoweredAttack()) return 1m;
    return 1m + 0.1m * DynamicVars["SlowAmount"].BaseValue;
}

public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
{
    if (side != Owner.Side) return Task.CompletedTask;
    DynamicVars["SlowAmount"].BaseValue = 0m;
    InvokeDisplayAmountChanged();
    return Task.CompletedTask;
}
```

---

## 12. HoverTip 模式

### 引用其他 Power

```csharp
// DemonFormPower - 引用 StrengthPower
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.FromPower<StrengthPower>()];

// NoxiousFumesPower - 引用 PoisonPower
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.FromPower<PoisonPower>()];

// EnvenomPower - 引用 PoisonPower
// WraithFormPower - 引用 DexterityPower
```

### 静态 HoverTip

```csharp
// BarricadePower - 显示 Block 提示
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.Static(StaticHoverTip.Block)];

// 其他使用 StaticHoverTip.Block 的 Power：
// BlurPower, DexterityPower, FeelNoPainPower, FrailPower, RagePower
```

### 引用卡牌

```csharp
// InfiniteBladesPower - 引用小刀卡牌
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.FromCard<Shiv>()];

// SentryModePower - 引用 SweepingGaze
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.FromCard<SweepingGaze>()];

// SwordSagePower - 引用带提示的卡牌
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.FromCardWithCardHoverTips<SovereignBlade>()];
```

### 引用关键词

```csharp
// DarkEmbracePower - 引用 Exhaust 关键词
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

// CorruptionPower - 引用 Exhaust 关键词
```

### 引用 Forge

```csharp
// SeekingEdgePower
protected override IEnumerable<IHoverTip> ExtraHoverTips
    => [HoverTipFactory.FromForge()];
```

---

## 13. 完整 Power 示例

### 示例 1：简单 Buff（StrengthPower）

```csharp
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class StrengthPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => true;

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (Owner != dealer) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        return Amount;
    }
}
```

### 示例 2：Debuff 回合递减（VulnerablePower）

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class VulnerablePower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => [new DynamicVar("DamageIncrease", 1.5m)];

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return DynamicVars["DamageIncrease"].BaseValue;
    }

    public override async Task AfterTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy)
            await PowerCmd.TickDownDuration(this);
    }
}
```

### 示例 3：Single 类型 + 格挡控制（BarricadePower）

```csharp
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class BarricadePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => [new StringVar("ApplierName")];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Applier != null && Applier.IsMonster)
            ((StringVar)DynamicVars["ApplierName"]).StringValue
                = Applier.Monster.Title.GetFormattedText();
        return Task.CompletedTask;
    }

    public override bool ShouldClearBlock(Creature creature)
    {
        if (Owner != creature) return true;
        return false;
    }
}
```

### 示例 4：Internal Data 模式（VigorPower）

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class VigorPower : PowerModel
{
    private class Data
    {
        public AttackCommand? commandToModify;
        public int amountWhenAttackStarted;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData() => new Data();

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker != Owner) return Task.CompletedTask;
        if (!command.DamageProps.IsPoweredAttack()) return Task.CompletedTask;
        var data = GetInternalData<Data>();
        if (data.commandToModify != null) return Task.CompletedTask;
        data.commandToModify = command;
        data.amountWhenAttackStarted = Amount;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (Owner != dealer) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        var data = GetInternalData<Data>();
        if (data.commandToModify != null
            && cardSource != data.commandToModify.ModelSource)
            return 0m;
        return Amount;
    }

    public override async Task AfterAttack(AttackCommand command)
    {
        var data = GetInternalData<Data>();
        if (command == data.commandToModify)
            await PowerCmd.ModifyAmount(
                this, -data.amountWhenAttackStarted, null, null);
    }
}
```

### 示例 5：IsInstanced + 自定义方法（TheBombPower）

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class TheBombPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => true;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => [new DamageVar(40m, ValueProp.Unpowered)];

    public void SetDamage(decimal damage)
    {
        AssertMutable();
        DynamicVars.Damage.BaseValue = damage;
    }

    public override async Task BeforeTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side) return;
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }
        Flash();
        await CreatureCmd.Damage(choiceContext,
            CombatState.HittableEnemies, DynamicVars.Damage, Owner);
        await PowerCmd.Remove(this);
    }
}
```

### 示例 6：AfterCardExhausted + InternalData（DarkEmbracePower）

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class DarkEmbracePower : PowerModel
{
    private class Data
    {
        public int etherealCount;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override object InitInternalData() => new Data();

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature != Owner) return;
        if (causedByEthereal)
            GetInternalData<Data>().etherealCount++;
        else
            await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }

    public override async Task AfterTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player)
        {
            var data = GetInternalData<Data>();
            await CardPileCmd.Draw(
                choiceContext, Amount * data.etherealCount, Owner.Player);
            data.etherealCount = 0;
        }
    }
}
```

### 示例 7：能量修正 + 卡牌去向（CorruptionPower）

```csharp
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class CorruptionPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public override bool TryModifyEnergyCostInCombat(
        CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (card.Type == CardType.Skill)
        {
            modifiedCost = 0;
            return true;
        }
        modifiedCost = originalCost;
        return false;
    }

    public override (PileType, CardPilePosition)
        ModifyCardPlayResultPileTypeAndPosition(
            CardModel card, bool isAutoPlay,
            ResourceInfo resources,
            PileType originalPileType,
            CardPilePosition position)
    {
        if (card.Type == CardType.Skill)
            return (PileType.Exhaust, position);
        return (originalPileType, position);
    }
}
```

### 示例 8：VFX + 多目标 Power（NoxiousFumesPower）

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class NoxiousFumesPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => [HoverTipFactory.FromPower<PoisonPower>()];

    public override async Task AfterSideTurnStart(
        CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side) return;
        Flash();
        await PowerCmd.Apply<PoisonPower>(
            CombatState.HittableEnemies, Amount, Owner, null);
    }
}
```

### 示例 9：DisplayAmount 覆盖 + 重置逻辑（SlowPower）

```csharp
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class SlowPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount
        => DynamicVars["SlowAmount"].IntValue * 10;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => [new DynamicVar("SlowAmount", 0m)];

    public override Task AfterCardPlayed(
        PlayerChoiceContext context, CardPlay cardPlay)
    {
        DynamicVars["SlowAmount"].BaseValue++;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return 1m + 0.1m * DynamicVars["SlowAmount"].BaseValue;
    }

    public override Task AfterSideTurnStart(
        CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side) return Task.CompletedTask;
        DynamicVars["SlowAmount"].BaseValue = 0m;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}
```

---

## 14. 附录：常用命名空间

```csharp
using MegaCrit.Sts2.Core.Models;                          // PowerModel 基类
using MegaCrit.Sts2.Core.Models.Powers;                   // 具体 Power 实现
using MegaCrit.Sts2.Core.Entities.Powers;                 // PowerType, PowerStackType
using MegaCrit.Sts2.Core.Entities.Creatures;              // Creature
using MegaCrit.Sts2.Core.Entities.Cards;                  // CardModel, CardType, CardTag
using MegaCrit.Sts2.Core.Commands;                        // PowerCmd, CreatureCmd, CardPileCmd
using MegaCrit.Sts2.Core.Combat;                          // CombatSide, CombatState
using MegaCrit.Sts2.Core.ValueProps;                      // ValueProp
using MegaCrit.Sts2.Core.HoverTips;                       // HoverTipFactory, StaticHoverTip
using MegaCrit.Sts2.Core.Localization.DynamicVars;        // DynamicVar, StringVar, DamageVar
using MegaCrit.Sts2.Core.GameActions.Multiplayer;         // PlayerChoiceContext
using MegaCrit.Sts2.Core.Context;                         // CombatManager
```
