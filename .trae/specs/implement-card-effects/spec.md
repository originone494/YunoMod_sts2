# 实现卡牌 Play/Upgrade 效果

## Why
根据 `进度.md` 中的计划，绝大部分卡牌（约 60+ 张）仍然标注为"未完成"，其 Play 方法和 Upgrade 方法存在简化实现或空实现。需要完整实现这些卡牌的效果逻辑，并确保构建通过。

## What Changes
- 对 `Scripts/Cards/Attack/`、`Scripts/Cards/Skill/`、`Scripts/Cards/Power/` 下已存在的卡牌类，逐张检查和补全 `OnPlay` 和 `OnUpgrade` 方法
- 确保使用了正确的 `DynamicVar`（如 `DamageVar`, `BlockVar`, `PowerVar`, `RepeatVar`, `CardsVar`, `EnergyVar`）
- 实现升级效果（数值提升、费用减少、去除消耗等）
- 对于复杂效果（自定义 Power、姿态联动等），添加注释说明暂不实现，但不简化已有逻辑
- 在 `杀戮尖塔2接口文档.md` 中记录所有使用到的 API 经验
- 每次修改后构建项目检查语法错误

## Impact
- Affected specs: 全部卡牌功能
- Affected code: `Scripts/Cards/Attack/*.cs`, `Scripts/Cards/Skill/*.cs`, `Scripts/Cards/Power/*.cs`
- Affected docs: `杀戮尖塔2接口文档.md`, `进度.md`

## Categories of Changes

### Category A: 数值型卡牌（直接伤害/格挡）
使用 `DamageVar`/`BlockVar` 声明动态数值，`OnPlay` 中调用 `DamageCmd.Attack`/`CreatureCmd.GainBlock`，`OnUpgrade` 中调用 `UpgradeValueBy`。

参考实现：`StrikeIronclad.cs`、`DefendIronclad.cs`、`ShrugItOff.cs`

### Category B: 状态附加卡牌（易伤/虚弱/中毒/流血）
在 `CanonicalVars` 中使用 `PowerVar<T>` 或 `DynamicVar` 声明状态层数，`OnPlay` 中调用 `PowerCmd.Apply<T>()`，`OnUpgrade` 提升层数。

参考实现：`PoisonedStab.cs`、`Neutralize.cs`、`Uppercut.cs`、`Thunderclap.cs`

### Category C: 多次命中/全体攻击
使用 `DamageCmd.Attack()` 的 `.WithHitCount(n)` 或 `.TargetingAllOpponents()`。

参考实现：`Whirlwind.cs`、`DaggerSpray.cs`、`FiendFire.cs`

### Category D: 抽牌/弃牌/能量相关
使用 `CardPileCmd.Draw()`、`CardPileCmd.Add()`、`PlayerCmd.GainEnergy()`、`CardCmd.Exhaust()`。
使用 `CardsVar`、`EnergyVar` 声明数值。

参考实现：`BattleTrance.cs`、`DoubleEnergy.cs`、`CalculatedGamble.cs`

### Category E: 选择牌/检索牌
使用 `CardSelectCmd.FromHand()`、`CardSelectCmd.FromSimpleGrid()` 让玩家选择卡牌。

参考实现：`TrueGrit.cs`、`Headbutt.cs`、`Armaments.cs`

### Category F: 复杂效果/自定义 Power
对于需要自定义 Power 类（如爱意、枪械姿态、日记数等），保留注释说明暂不实现，标记为复杂效果。
但 Play 方法中的基本逻辑（如触发动画、获得费用、抽牌等）仍需实现。

## Implementation Rules
1. 所有卡牌类使用拼音命名，命名空间统一
2. 使用 `base(费用, CardType.类型, CardRarity.稀有度, TargetType.目标)` 构造
3. 单目标伤害必须使用 `ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");`
4. `OnUpgrade` 必须实现具体逻辑（数值提升/费用减少），不可留空
5. 升级效果中 `EnergyCost.UpgradeBy(-1)` 表示减费
6. `DynamicVars.Damage.UpgradeValueBy(n)` 表示伤害提升n点
7. `DynamicVars.Block.UpgradeValueBy(n)` 表示格挡提升n点
8. 消耗关键词在构造函数中用 `AddKeyword(CardKeyword.Exhaust)` 或在 `CanonicalKeywords` 中声明
9. 保留属性通过 `AddKeyword(CardKeyword.Retain)` 添加
10. 灵活（Sly）通过 `AddKeyword(CardKeyword.Sly)` 添加
