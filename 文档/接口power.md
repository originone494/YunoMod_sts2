# 杀戮尖塔 2 Power 完整接口文档

## 一、核心成员（PowerModel 基类）

### 公共属性
| 属性 | 类型 | 说明 |
|------|------|------|
| `Owner` | `Creature` | Power 持有者 |
| `Applier` | `Creature?` | Power 施加者 |
| `Target` | `Creature?` | Power 目标 |
| `Amount` | `int` | 当前层数 |
| `AmountOnTurnStart` | `int` | 回合开始时数值 |
| `DynamicVars` | `DynamicVarSet` | 动态变量集合 |
| `CombatState` | `CombatState` | 当前战斗状态 |

### 基础属性（可覆写）
```csharp
// 必须覆写
public override PowerType Type => PowerType.Buff;
public override PowerStackType StackType => PowerStackType.Counter;

// 可选覆写
public override bool AllowNegative => false;
public override int DisplayAmount => Amount;
public override bool IsInstanced => false;
public override bool ShouldScaleInMultiplayer => false;
public override bool ShouldPlayVfx => true;
public override bool OwnerIsSecondaryEnemy => false;
public override bool IsVisible => true;
public override bool SkipNextDurationTick => false;
public override Color AmountLabelColor => ...;
```

---

## 二、伤害修正方法

### 1. 加性修正
```csharp
public virtual decimal ModifyDamageAdditive(
    Creature? target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)
```

### 2. 乘性修正
```csharp
public virtual decimal ModifyDamageMultiplicative(
    Creature? target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)
```

### 3. 伤害上限
```csharp
public virtual decimal ModifyDamageCap(
    Creature? target,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)
```

### 4. OSTY 后生命损失修正
```csharp
public virtual decimal ModifyHpLostAfterOsty(
    Creature target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)
```

### 5. 后期修正
```csharp
public virtual decimal ModifyHpLostAfterOstyLate(
    Creature target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)
```

### 6. 修正后回调
```csharp
public virtual Task AfterModifyingHpLostAfterOsty()
```

### 7. 伤害修正后回调
```csharp
public virtual Task AfterModifyingDamageAmount(
    CardModel? cardSource
)
```

---

## 三、格挡修正方法

### 1. 加性修正
```csharp
public virtual decimal ModifyBlockAdditive(
    Creature target,
    decimal block,
    ValueProp props,
    CardModel? cardSource,
    CardPlay? cardPlay
)
```

### 2. 乘性修正
```csharp
public virtual decimal ModifyBlockMultiplicative(
    Creature target,
    decimal block,
    ValueProp props,
    CardModel? cardSource,
    CardPlay? cardPlay
)
```

### 3. 格挡修正后回调
```csharp
public virtual Task AfterModifyingBlockAmount(
    decimal modifiedBlock,
    CardModel? cardSource,
    CardPlay? cardPlay
)
```

---

## 四、其他修正方法

### 抽牌修正
```csharp
public virtual decimal ModifyHandDraw(
    Player player,
    decimal originalCardCount
)
```

### 能量修正
```csharp
public virtual bool TryModifyEnergyCostInCombat(
    CardModel card,
    decimal originalCost,
    out decimal modifiedCost
)
```

### 格挡清除控制
```csharp
public virtual bool ShouldClearBlock(
    Creature creature
)
```

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

### 卡牌复制次数
```csharp
public virtual int ModifyCardPlayCount(
    CardModel card,
    Creature? target,
    int playCount
)
```

### Orb 值修正
```csharp
public virtual decimal ModifyOrbValue(
    Player player,
    decimal value
)
```

### 受到 Power 数值修正
```csharp
public virtual bool TryModifyPowerAmountReceived(
    PowerModel canonicalPower,
    Creature target,
    decimal amount,
    Creature? applier,
    out decimal modifiedAmount
)
```

---

## 五、生命周期钩子（按顺序）

### 灵魂绑定（保护成员）
```csharp
// 定义规范变量
protected virtual IEnumerable<DynamicVar> CanonicalVars => Array.Empty<DynamicVar>();

// 定义额外悬停提示
protected virtual IEnumerable<IHoverTip> ExtraHoverTips => Array.Empty<IHoverTip>();

// 初始化内部数据（配合 Internal Data 模式）
protected override object InitInternalData() => new Data();
```

### 应用与移除
```csharp
// Power 被应用后
public virtual Task AfterApplied(
    Creature? applier,
    CardModel? cardSource
)

// Power 被移除后
public virtual Task AfterRemoved(
    Creature oldOwner
)

// 持有者死亡时是否移除
public virtual bool ShouldPowerBeRemovedAfterOwnerDeath()

// 持有者死亡是否触发致命
public virtual bool ShouldOwnerDeathTriggerFatal()
```

### 卡牌相关
```csharp
// 卡牌打出前
public virtual Task BeforeCardPlayed(
    CardPlay cardPlay
)

// 卡牌打出后
public virtual Task AfterCardPlayed(
    PlayerChoiceContext choiceContext,
    CardPlay cardPlay
)

// 卡牌抽到后
public virtual Task AfterCardDrawn(
    PlayerChoiceContext choiceContext,
    CardModel card,
    bool fromHandDraw
)

// 卡牌耗尽后
public virtual Task AfterCardExhausted(
    PlayerChoiceContext choiceContext,
    CardModel card,
    bool causedByEthereal
)

// 卡牌丢弃后
public virtual Task AfterCardDiscarded(
    PlayerChoiceContext choiceContext,
    CardModel card
)
```

### 回合相关
```csharp
// 回合结束前
public virtual Task BeforeTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side
)

// 回合结束后
public virtual Task AfterTurnEnd(
    PlayerChoiceContext choiceContext,
    CombatSide side
)

// 某方回合开始时
public virtual Task AfterSideTurnStart(
    CombatSide side,
    CombatState combatState
)

// 玩家回合开始
public virtual Task AfterPlayerTurnStart(
    PlayerChoiceContext choiceContext,
    Player player
)

// 抽牌前
public virtual Task BeforeHandDraw(
    Player player,
    PlayerChoiceContext choiceContext,
    CombatState combatState
)
```

### 伤害相关
```csharp
// 受到伤害前
public virtual Task BeforeDamageReceived(
    PlayerChoiceContext choiceContext,
    Creature target,
    decimal amount,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)

// 受到伤害后
public virtual Task AfterDamageReceived(
    PlayerChoiceContext choiceContext,
    Creature target,
    DamageResult result,
    ValueProp props,
    Creature? dealer,
    CardModel? cardSource
)

// 造成伤害后
public virtual Task AfterDamageGiven(
    PlayerChoiceContext choiceContext,
    Creature? dealer,
    DamageResult results,
    ValueProp props,
    Creature target,
    CardModel? cardSource
)

// 获得格挡后
public virtual Task AfterBlockGained(
    Creature creature,
    decimal amount,
    ValueProp props,
    CardModel? cardSource
)
```

### 其他回调
```csharp
// 阻止格挡清除后
public virtual Task AfterPreventingBlockClear(
    AbstractModel preventer,
    Creature creature
)

// Power 数值变化后
public virtual Task AfterPowerAmountChanged(
    PowerModel power,
    decimal amount,
    Creature? applier,
    CardModel? cardSource
)

// HP 变化后
public virtual Task AfterCurrentHpChanged(
    Creature creature,
    decimal delta
)

// 攻击前
public virtual Task BeforeAttack(
    AttackCommand command
)

// 攻击后
public virtual Task AfterAttack(
    AttackCommand command
)

// 刷新前
public virtual Task BeforeFlush(
    PlayerChoiceContext choiceContext,
    Player player
)
```

---

## 六、核心指令（PowerCmd）

```csharp
// 施加 Power
PowerCmd.Apply<TPower>(target, amount, applier, cardSource)

// 移除
PowerCmd.Remove(power)

// 递减
PowerCmd.Decrement(power)
PowerCmd.TickDownDuration(power)  // 敌方回合结束时

// 修改数值
PowerCmd.ModifyAmount(power, delta, applier, cardSource)
PowerCmd.SetAmount(power, amount, applier, cardSource)

// 闪烁
PowerCmd.Flash(power)
Flash()  // 实例方法
```

---

## 七、获取 Power

```csharp
TPower? power = creature.GetPower<TPower>();
int amount = creature.GetPowerAmount<TPower>();
bool hasPower = creature.HasPower<TPower>();
```

---

## 八、常用动态变量类型

```csharp
new DynamicVar("Key", defaultValue)
new DamageVar(damage, ValueProp.Move)
new StringVar("Key")
new PowerVar<TPower>(amount)
```