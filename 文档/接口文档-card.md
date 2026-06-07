# 杀戮尖塔2 卡牌 Mod 开发接口文档

基于 `code/Cards/` 中原版参考卡牌整理。

---

## 目录

1. [卡牌基类 CardModel](#1-卡牌基类-cardmodel)
2. [构造函数参数](#2-构造函数参数)
3. [核心属性/覆写项](#3-核心属性覆写项)
4. [DynamicVar 数值系统](#4-dynamicvar-数值系统)
5. [战斗指令 Commands](#5-战斗指令-commands)
6. [卡牌关键词 CardKeyword](#6-卡牌关键词-cardkeyword)
7. [卡牌标签 CardTag](#7-卡牌标签-cardtag)
8. [升级系统](#8-升级系统)
9. [HoverTip 悬停提示](#9-hovertip-悬停提示)
10. [卡牌选择器](#10-卡牌选择器)
11. [牌堆操作与 PileType](#11-牌堆操作与-piletype)
12. [VFX 特效与音效](#12-vfx-特效与音效)
13. [完整卡牌示例](#13-完整卡牌示例)
14. [附录：常用命名空间](#14-附录常用命名空间)

---

## 1. 卡牌基类 CardModel

所有卡牌继承自 `CardModel`，位于命名空间 `MegaCrit.Sts2.Core.Entities.Cards`。

```csharp
using MegaCrit.Sts2.Core.Entities.Cards;

public sealed class ExampleCard : CardModel
{
    public ExampleCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }
}
```

---

## 2. 构造函数参数

```csharp
public CardName() : base(energyCost, cardType, cardRarity, targetType)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| `energyCost` | `int` | 能量消耗 |
| `cardType` | `CardType` | 卡牌类型 |
| `cardRarity` | `CardRarity` | 稀有度 |
| `targetType` | `TargetType` | 目标类型 |

### CardType

| 值 | 说明 |
|----|------|
| `CardType.Attack` | 攻击牌 |
| `CardType.Skill` | 技能牌 |
| `CardType.Power` | 能力牌 |
| `CardType.Status` | 状态牌 |
| `CardType.Curse` | 诅咒牌 |

### CardRarity

| 值 | 说明 |
|----|------|
| `CardRarity.Basic` | 基础 |
| `CardRarity.Common` | 普通 |
| `CardRarity.Uncommon` | 罕见 |
| `CardRarity.Rare` | 稀有 |
| `CardRarity.Ancient` | 远古（相当于原版 Special） |
| `CardRarity.Event` | 活动（不出现在卡池） |

### TargetType

| 值 | 说明 |
|----|------|
| `TargetType.AnyEnemy` | 单个敌人 |
| `TargetType.AllEnemies` | 全体敌人 |
| `TargetType.Self` | 自身 |
| `TargetType.None` | 无目标 |

---

## 3. 核心属性/覆写项

| 成员 | 类型 | 说明 | 示例 |
|------|------|------|------|
| `CanonicalVars` | `IEnumerable<DynamicVar>` | 卡牌数值定义 | `new DamageVar(6m, ValueProp.Move)` |
| `CanonicalTags` | `HashSet<CardTag>` | 卡牌标签 | `[CardTag.Strike]` |
| `CanonicalKeywords` | `IEnumerable<CardKeyword>` | 卡牌关键词 | `[CardKeyword.Exhaust]` |
| `ExtraHoverTips` | `IEnumerable<IHoverTip>` | 悬停提示 | `HoverTipFactory.FromPower<WeakPower>()` |
| `GainsBlock` | `bool` | 是否获得格挡 | `true` |
| `HasEnergyCostX` | `bool` | 是否为 X 费用 | `true` |
| `ExtraRunAssetPaths` | `IEnumerable<string>` | 额外资源预加载 | `NGroundFireVfx.AssetPaths` |
| `VisualCardPool` | `CardPoolModel` | 卡牌所属卡池（用于图鉴显示） | `ModelDb.CardPool<IroncladCardPool>()` |

### 生命周期方法

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `OnPlay(PlayerChoiceContext, CardPlay)` | `Task` | 打出卡牌时触发 |
| `OnUpgrade()` | `void` | 卡牌升级时触发 |
| `AfterDowngraded()` | `void` | 卡牌降级后触发（可用于恢复永久性变更） |

---

## 4. DynamicVar 数值系统

DynamicVar 用于定义卡牌上展示的动态数值，定义在 `CanonicalVars` 中。每个变量会自动生成类型化的属性访问器。

### 4.1 DynamicVar 类型

| 类型 | 构造参数 | 属性名 | 说明 |
|------|----------|--------|------|
| `DamageVar(decimal, ValueProp)` | 基础值, 数值属性 | `.Damage` | 伤害值 |
| `BlockVar(decimal, ValueProp)` | 基础值, 数值属性 | `.Block` | 格挡值 |
| `PowerVar<TPower>(decimal)` | 基础值 | `.Weak` / `.Vulnerable` / `.Poison` 等 | 状态层数，属性名为 Power 类名 |
| `RepeatVar(int)` | 基础值 | `.Repeat` | 重复次数 |
| `EnergyVar(int)` | 基础值 | `.Energy` | 能量值 |
| `CardsVar(int)` | 基础值 | `.Cards` | 卡牌数量 |
| `DynamicVar(string, decimal)` | 名称, 基础值 | 通过字符串索引访问 | 自定义数值 |

### 4.2 ValueProp

| 值 | 说明 |
|----|------|
| `ValueProp.Move` | 可变动数值（受力量等影响） |
| `ValueProp.None` | 固定数值 |

### 4.3 访问方式

```csharp
// 类型化属性访问（推荐）
DynamicVars.Damage.BaseValue    // decimal
DynamicVars.Damage.IntValue     // int（取整）
DynamicVars.Block.BaseValue
DynamicVars.Repeat.IntValue
DynamicVars.Energy.IntValue
DynamicVars.Cards.BaseValue
DynamicVars.Weak.BaseValue      // PowerVar<WeakPower>
DynamicVars.Vulnerable.BaseValue // PowerVar<VulnerablePower>
DynamicVars.Poison.BaseValue    // PowerVar<PoisonPower>

// 字符串索引访问
DynamicVars["StrengthPower"].BaseValue
DynamicVars["EchoForm"].BaseValue
DynamicVars["Increase"].BaseValue

// 升级数值
DynamicVars.Damage.UpgradeValueBy(3m)
DynamicVars.Repeat.UpgradeValueBy(1)
DynamicVars["StrengthPower"].UpgradeValueBy(1m)
DynamicVars["Increase"].UpgradeValueBy(1m)

// 直接修改基础值（Claw 模式）
DynamicVars.Damage.BaseValue += extraDamage;
```

### 4.4 定义顺序

`CanonicalVars` 中定义的顺序即卡面上的显示顺序。

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new DamageVar(8m, ValueProp.Move),       // 显示为 8 伤害
    new PowerVar<VulnerablePower>(2m),       // 显示为 2 层易伤
];
```

---

## 5. 战斗指令 Commands

战斗指令位于命名空间 `MegaCrit.Sts2.Core.Commands`。

### 5.1 伤害指令 DamageCmd

```csharp
// 基础单段攻击
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)         // 单个目标
    .Execute(choiceContext);

// 带打击特效
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_slash")  // VFX 路径
    .Execute(choiceContext);

// 带特效 + 音效
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
    .Execute(choiceContext);

// 多段攻击（WithHitCount）
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitCount(DynamicVars.Repeat.IntValue)  // 段数
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext);

// 全体 AOE 攻击
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .TargetingAllOpponents(CombatState)         // 全体敌人
    .WithHitCount(num)
    .WithHitFx("vfx/vfx_giant_horizontal_slash")
    .Execute(choiceContext);

// 攻击者特效（AttackerFx）
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .TargetingAllOpponents(CombatState)
    .WithHitCount(2)
    .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(Owner.Creature, color, goingRight: true))
    .Execute(choiceContext);

// 自定义 VFX Node
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
    .Execute(choiceContext);

// 伤害前回调（BeforeDamage）
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(cardPlay.Target)
    .WithHitCount(cardCount)
    .BeforeDamage(async () =>
    {
        // 每次攻击命中前执行
        SfxCmd.Play("event:/sfx/characters/attack_fire");
    })
    .Execute(choiceContext);
```

### DamageCmd 链式方法汇总

| 方法 | 说明 |
|------|------|
| `.FromCard(CardModel)` | 设置伤害来源卡牌 |
| `.Targeting(Creature)` | 设置单个目标 |
| `.TargetingAllOpponents(CombatState)` | 设置全体敌人为目标 |
| `.WithHitCount(int)` | 设置攻击段数 |
| `.WithHitFx(string, string, string)` | 设置打击特效 VFX |
| `.WithHitVfxNode(Func<Creature, Node>)` | 设置自定义 VFX Node |
| `.WithAttackerFx(Func<Node>)` | 设置攻击者特效 |
| `.BeforeDamage(Func<Task>)` | 每次伤害前回调 |
| `.Execute(PlayerChoiceContext)` | 执行指令 |

### 5.2 格挡指令

```csharp
await CreatureCmd.GainBlock(
    Owner.Creature,         // 获得格挡的生物
    DynamicVars.Block,      // 格挡值（DynamicVar）
    cardPlay                // 卡牌打出上下文
);
```

### 5.3 Power 指令

```csharp
await PowerCmd.Apply<WeakPower>(
    cardPlay.Target,               // 目标 Creature
    DynamicVars.Weak.BaseValue,    // 层数
    Owner.Creature,                // 来源 Creature
    this                           // 来源卡牌（可为 null）
);

// 能力牌用法
await PowerCmd.Apply<BarricadePower>(
    Owner.Creature,
    1m,                           // 固定层数
    Owner.Creature,
    this
);

await PowerCmd.Apply<DemonFormPower>(
    Owner.Creature,
    DynamicVars["StrengthPower"].BaseValue,
    Owner.Creature,
    this
);
```

### 5.4 能量指令

```csharp
// 获得等量的当前能量（Double Energy）
await PlayerCmd.GainEnergy(
    Owner.PlayerCombatState.Energy,  // 当前能量值
    Owner
);

// 获得固定能量（Adrenaline）
await PlayerCmd.GainEnergy(
    DynamicVars.Energy.IntValue,
    Owner
);
```

### 5.5 抽牌/摸牌指令

```csharp
// 抽牌
await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

// 弃牌并抽等量的牌（Calculated Gamble）
IEnumerable<CardModel> cards = PileType.Hand.GetPile(Owner).Cards;
int cardsToDraw = cards.Count();
await CardCmd.DiscardAndDraw(choiceContext, cards, cardsToDraw);
```

### 5.6 消耗/放逐指令

```csharp
// 消耗单张卡牌
await CardCmd.Exhaust(choiceContext, card);

// 遍历手牌全部消耗（Fiend Fire）
foreach (CardModel item in handCards)
{
    await CardCmd.Exhaust(choiceContext, item);
}
```

### 5.7 添加卡牌指令

```csharp
// 添加生成的卡牌到手牌（Dual Wield）
CardModel clone = selection.CreateClone();
await CardPileCmd.AddGeneratedCardToCombat(
    clone,
    PileType.Hand,
    addedByPlayer: true
);
```

### 5.8 升级指令

```csharp
// 升级卡牌（Apotheosis）
if (allCard != this && allCard.IsUpgradable)
{
    CardCmd.Upgrade(allCard);
}
```

### 5.9 球体指令

```csharp
// 激发闪电球（Ball Lightning）
await OrbCmd.Channel<LightningOrb>(choiceContext, Owner);

// 激发冰霜球（Coolheaded）
await OrbCmd.Channel<FrostOrb>(choiceContext, Owner);
```

### 5.10 动画指令

```csharp
// 触发生物动画（Cast 动作）
await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
```

### 5.11 特效/音效指令

```csharp
// 播放音效
SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
SfxCmd.Play("event:/sfx/characters/silent/silent_dagger_spray");
SfxCmd.Play("event:/sfx/characters/attack_fire");

// 播放全屏特效
VfxCmd.PlayFullScreenInCombat("vfx/vfx_adrenaline");

// 添加 VFX Node 到场景
NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfxNode);
NRun.Instance?.GlobalUi.AddChildSafely(vfxNode);
```

---

## 6. 卡牌关键词 CardKeyword

### 6.1 关键词列表

| 值 | 说明 |
|----|------|
| `CardKeyword.Exhaust` | 消耗（放逐） |
| `CardKeyword.Ethereal` | 虚无（回合结束时消耗） |
| `CardKeyword.Innate` | 固有（开局在手牌） |
| `CardKeyword.Retain` | 保留（回合结束不弃） |

### 6.2 定义方式

```csharp
// 覆写 CanonicalKeywords 属性
public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [CardKeyword.Exhaust, CardKeyword.Innate];
```

### 6.3 动态添加/移除

```csharp
// 添加关键词（在构造函数或升级中）
AddKeyword(CardKeyword.Retain);

// 移除关键词（Echo Form 升级移除虚无）
RemoveKeyword(CardKeyword.Ethereal);
```

---

## 7. 卡牌标签 CardTag

### 7.1 标签列表

| 值 | 说明 |
|----|------|
| `CardTag.Strike` | 打击标签 |
| `CardTag.Defend` | 防御标签 |

### 7.2 定义方式

```csharp
protected override HashSet<CardTag> CanonicalTags =>
    [CardTag.Strike];
// 或
protected override HashSet<CardTag> CanonicalTags =>
    [CardTag.Defend];
```

---

## 8. 升级系统

### 8.1 升级数值

```csharp
protected override void OnUpgrade()
{
    // 伤害增加
    DynamicVars.Damage.UpgradeValueBy(3m);

    // 格挡增加
    DynamicVars.Block.UpgradeValueBy(3m);

    // Power 层数增加
    DynamicVars.Poison.UpgradeValueBy(1m);
    DynamicVars.Vulnerable.UpgradeValueBy(1m);
    DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    DynamicVars["Increase"].UpgradeValueBy(1m);

    // 重复次数增加
    DynamicVars.Repeat.UpgradeValueBy(1);

    // 抽牌数增加
    DynamicVars.Cards.UpgradeValueBy(1m);

    // 能量值增加
    DynamicVars.Energy.UpgradeValueBy(1m);
}
```

### 8.2 升级费用

```csharp
// 费用减少 1
EnergyCost.UpgradeBy(-1);
// 或
base.EnergyCost.UpgradeBy(-1);
```

### 8.3 升级关键词

```csharp
// 添加保留
AddKeyword(CardKeyword.Retain);

// 移除虚无
RemoveKeyword(CardKeyword.Ethereal);
```

---

## 9. HoverTip 悬停提示

命名空间：`MegaCrit.Sts2.Core.HoverTips`

### 9.1 从 Power 生成提示

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [HoverTipFactory.FromPower<VulnerablePower>()];

// 多个 Power 提示
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    HoverTipFactory.FromPower<WeakPower>(),
    HoverTipFactory.FromPower<PoisonPower>(),
];
```

### 9.2 从球体生成提示

```csharp
protected override IEnumerable<IHoverTip> ExtraHoverTips =>
[
    HoverTipFactory.Static(StaticHoverTip.Channeling),
    HoverTipFactory.FromOrb<LightningOrb>(),
];
```

### 9.3 静态提示

```csharp
// 格挡提示
HoverTipFactory.Static(StaticHoverTip.Block);

// 充能提示
HoverTipFactory.Static(StaticHoverTip.Channeling);
```

### 9.4 能量提示

```csharp
// Double Energy 使用
base.EnergyHoverTip
```

### 9.5 HoverTipFactory 方法汇总

| 方法 | 说明 |
|------|------|
| `HoverTipFactory.FromPower<TPower>()` | 从 Power 类型生成提示 |
| `HoverTipFactory.FromOrb<TOrb>()` | 从球体类型生成提示 |
| `HoverTipFactory.Static(StaticHoverTip)` | 静态文本提示 |

---

## 10. 卡牌选择器

命名空间：`MegaCrit.Sts2.Core.CardSelection`

```csharp
using MegaCrit.Sts2.Core.CardSelection;

// 从手牌中选择（Dual Wield）
CardModel selection = (await CardSelectCmd.FromHand(
    prefs: new CardSelectorPrefs(
        base.SelectionScreenPrompt,  // 提示文字
        1                            // 选择数量
    ),
    context: choiceContext,
    player: base.Owner,
    filter: delegate(CardModel c)
    {
        // 筛选条件
        CardType type = c.Type;
        return type == CardType.Attack || type == CardType.Power;
    },
    source: this
)).FirstOrDefault();

if (selection != null)
{
    // 克隆并添加到战斗
    CardModel clone = selection.CreateClone();
    await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, addedByPlayer: true);
}
```

---

## 11. 牌堆操作与 PileType

### 11.1 PileType

| 值 | 说明 |
|----|------|
| `PileType.Draw` | 抽牌堆 |
| `PileType.Hand` | 手牌 |
| `PileType.Discard` | 弃牌堆 |
| `PileType.Exhaust` | 消耗牌堆 |
| `PileType.Deck` | 牌组（非战斗中） |

### 11.2 常用操作

```csharp
// 获取手牌列表
var handCards = PileType.Hand.GetPile(Owner).Cards;

// 获取玩家所有卡牌（包括各牌堆）
var allCards = Owner.PlayerCombatState.AllCards;

// 筛选特定类型的卡牌
var claws = Owner.PlayerCombatState.AllCards.OfType<Claw>();

// 获取当前能量
int currentEnergy = Owner.PlayerCombatState.Energy;
```

---

## 12. VFX 特效与音效

### 12.1 VFX 路径

| 路径 | 说明 | 来源卡牌 |
|------|------|----------|
| `vfx/vfx_attack_slash` | 斩击特效 | StrikeIronclad |
| `vfx/vfx_attack_blunt` | 钝击特效 | Bash, Bludgeon, BallLightning |
| `vfx/vfx_giant_horizontal_slash` | 巨型横斩特效 | Whirlwind |
| `vfx/vfx_dramatic_stab` | 穿刺特效 | PoisonedStab |
| `vfx/vfx_adrenaline` | 肾上腺素全屏特效 | Adrenaline |
| `vfx/vfx_heavy_blunt` | 重击特效 | - |
| `vfx/vfx_bloody_impact` | 流血冲击特效 | - |
| `vfx/vfx_rock_shatter` | 碎石特效 | - |

### 12.2 音效路径（FMOD）

| 路径 | 说明 | 来源卡牌 |
|------|------|----------|
| `event:/sfx/characters/ironclad/ironclad_whirlwind` | 旋风斩音效 | Whirlwind |
| `event:/sfx/characters/silent/silent_dagger_spray` | 匕首雨音效 | DaggerSpray |
| `event:/sfx/characters/attack_fire` | 火焰攻击音效 | FiendFire |
| `blunt_attack.mp3` | 钝击音效（内嵌资源） | Bash, Bludgeon |

### 12.3 VFX Node 类型

| Node 类型 | 说明 | 来源卡牌 |
|-----------|------|----------|
| `NGroundFireVfx` | 地面火焰特效 | FiendFire |
| `NHorizontalLinesVfx` | 水平线特效 | Whirlwind |
| `NSmokyVignetteVfx` | 烟雾晕影特效 | Whirlwind |
| `NDaggerSprayFlurryVfx` | 匕首雨攻击者特效 | DaggerSpray |
| `NDaggerSprayImpactVfx` | 匕首雨命中特效 | DaggerSpray |
| `NScratchVfx` | 爪击特效 | Claw |

---

## 13. 完整卡牌示例

### 13.1 基础攻击牌（Strike）

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class StrikeIronclad : CardModel
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(6m, ValueProp.Move)];

    public StrikeIronclad()
        : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
```

### 13.2 基础防御牌（Defend）

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DefendIronclad : CardModel
{
    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(5m, ValueProp.Move)];

    public DefendIronclad()
        : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
```

### 13.3 攻击+易伤牌（Bash）

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Bash : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m)
    ];

    public Bash()
        : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(
            cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
```

### 13.4 攻击+中毒牌（PoisonedStab）

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class PoisonedStab : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PoisonPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new PowerVar<PoisonPower>(3m)
    ];

    public PoisonedStab()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);
        await PowerCmd.Apply<PoisonPower>(
            cardPlay.Target, DynamicVars.Poison.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Poison.UpgradeValueBy(1m);
    }
}
```

### 13.5 能力牌（DemonForm）

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DemonForm : CardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<StrengthPower>(2m)];

    public DemonForm()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<DemonFormPower>(
            Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    }
}
```

### 13.6 X 费用 AOE 牌（Whirlwind）

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Whirlwind : CardModel
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5m, ValueProp.Move)];

    public Whirlwind()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int xValue = ResolveEnergyXValue();

        // VFX + SFX
        SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(xValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_giant_horizontal_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
```

### 13.7 能量+抽牌（Adrenaline）

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Adrenaline : CardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [base.EnergyHoverTip];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    public Adrenaline()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMe(Owner))
        {
            VfxCmd.PlayFullScreenInCombat("vfx/vfx_adrenaline");
        }
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
```

### 13.8 弃牌+抽牌（CalculatedGamble）

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class CalculatedGamble : CardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    public CalculatedGamble()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<CardModel> cards = PileType.Hand.GetPile(Owner).Cards;
        int cardsToDraw = cards.Count();
        await CardCmd.DiscardAndDraw(choiceContext, cards, cardsToDraw);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
```

### 13.9 消耗手牌增伤（FiendFire）

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class FiendFire : CardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7m, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    public FiendFire()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 消耗全部手牌
        List<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        int cardCount = handCards.Count;

        foreach (CardModel card in handCards)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }

        // 按手牌数量造成多段伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(cardCount)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .BeforeDamage(async () =>
            {
                SfxCmd.Play("event:/sfx/characters/attack_fire");
            })
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
```

---

## 14. 附录：常用命名空间

| 命名空间 | 包含内容 |
|----------|----------|
| `MegaCrit.Sts2.Core.Commands` | 所有战斗指令（DamageCmd, PowerCmd, CardCmd, CardPileCmd, PlayerCmd, CreatureCmd, SfxCmd, VfxCmd, OrbCmd） |
| `MegaCrit.Sts2.Core.Entities.Cards` | 卡牌实体（CardModel, CardType, CardRarity, TargetType, CardKeyword, CardTag, PileType） |
| `MegaCrit.Sts2.Core.Entities.Creatures` | 生物实体（Creature） |
| `MegaCrit.Sts2.Core.Entities.Players` | 玩家实体 |
| `MegaCrit.Sts2.Core.Entities.Powers` | Power 实体（PowerType, PowerStackType） |
| `MegaCrit.Sts2.Core.Entities.Relics` | 遗物实体（RelicRarity） |
| `MegaCrit.Sts2.Core.GameActions.Multiplayer` | 多人/单人上下文（PlayerChoiceContext, CardPlay） |
| `MegaCrit.Sts2.Core.Localization.DynamicVars` | DynamicVar 系统（DamageVar, BlockVar, PowerVar, RepeatVar, EnergyVar, CardsVar, DynamicVar） |
| `MegaCrit.Sts2.Core.ValueProps` | ValueProp 枚举（Move, None） |
| `MegaCrit.Sts2.Core.HoverTips` | HoverTip 提示系统（HoverTipFactory, IHoverTip, StaticHoverTip） |
| `MegaCrit.Sts2.Core.Models.Powers` | 原版 Power 实现（StrengthPower, WeakPower, VulnerablePower, PoisonPower, VigorPower 等） |
| `MegaCrit.Sts2.Core.Models.Orbs` | 球体类型（LightningOrb, FrostOrb） |
| `MegaCrit.Sts2.Core.Models.CardPools` | 原版卡池（IroncladCardPool 等） |
| `MegaCrit.Sts2.Core.CardSelection` | 卡牌选择器（CardSelectCmd, CardSelectorPrefs） |
| `MegaCrit.Sts2.Core.Context` | 本地玩家上下文（LocalContext） |
| `MegaCrit.Sts2.Core.Nodes.Rooms` | 房间节点（NCombatRoom） |
| `MegaCrit.Sts2.Core.Nodes.Vfx` | VFX 特效节点（NGroundFireVfx, NDaggerSprayFlurryVfx 等） |
| `MegaCrit.Sts2.Core.Helpers` | 工具方法（AddChildSafely 等） |
