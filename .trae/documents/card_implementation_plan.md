# yuno-mod 卡牌实现计划

## 一、项目概述

根据进度表，当前已有6张卡牌完成实现，还需实现22张未完成的卡牌。本计划将按照优先级逐步实现这些卡牌的 `Play` 方法和 `Upgrade` 方法。

## 二、进度分析

| 序号 | 卡牌名称 | 文件名 | 类型 | 当前状态 | 优先级 |
|:---:|---------|--------|------|---------|:-----:|
| 7 | 我会好好做的 | WoHuiHaoHaoZuoDeCard.cs | Attack | 未完成 | 高 |
| 8 | 清空枪械 | QingKongDanJiaCard.cs | Skill | 未完成 | 高 |
| 9 | 音爆 | YinBaoCard.cs | Attack | 未完成 | 高 |
| 10 | 杀完了 | ShaWanLeCard.cs | Skill | 未完成 | 高 |
| 11 | 血溅 | XueJianCard.cs | Attack | 未完成 | 高 |
| 12 | 虚诈 | XuZhaCard.cs | Skill | 未完成 | 中 |
| 13 | 捷音 | JieYinCard.cs | Skill | 未完成 | 中 |
| 14 | 保护你 | BaoHuNiCard.cs | Attack | 未完成 | 中 |
| 15 | 刺突 | CiTuCard.cs | Attack | 未完成 | 中 |
| 16 | 你给我小心点 | NiGeiWoXiaoXinDianCard.cs | Power | 未完成 | 中 |
| 17 | 亮刃 | LiangRenCard.cs | Skill | 未完成 | 中 |
| 18 | 暗中观察 | AnZhongGuanChaCard.cs | Power | 未完成 | 中 |
| 19 | 无意中听 | WuYiZhongTingCard.cs | Power | 未完成 | 低 |
| 20 | 去看星星 | QuKanXingXingCard.cs | Skill | 未完成 | 低 |
| 21 | 就是你？ | JiuShiNiCard.cs | Skill | 未完成 | 低 |
| 22 | 未来偏差 | WeiLaiPianChaCard.cs | Skill | 未完成 | 低 |
| 23 | 电击枪 | DianJiQiangCard.cs | Skill | 未完成 | 低 |
| 24 | 由乃救我！ | YouNaiJiuWoCard.cs | Skill | 未完成 | 低 |
| 25 | 解毒剂 | JieDuJiCard.cs | Skill | 未完成 | 低 |
| 26 | 交换 | JiaoHuanCard.cs | Skill | 未完成 | 低 |
| 27 | 看破 | KanPoCard.cs | Power | 未完成 | 低 |
| 28 | 绝境 | - | Skill | 未完成 | 低 |

## 三、实现策略

### 3.1 技术要点

1. **命名约定**：卡牌类使用拼音命名（已遵循）
2. **继承结构**：所有卡牌继承自 `AbstractTemplateBaseCard`
3. **核心方法**：
   - `OnPlay()`: 卡牌打出时的效果
   - `OnUpgrade()`: 卡牌升级效果
4. **DynamicVar类型**：
   - `DamageVar`: 伤害值
   - `BlockVar`: 格挡值
   - `PowerVar<T>`: 能力层数
   - `RepeatVar`: 重复次数

### 3.2 复杂效果处理

对于难以实现的复杂效果（如姿态检测、自定义能力转移等），将：
1. 添加注释说明待实现内容
2. 实现基础功能保证代码可编译
3. 在接口文档中记录实现难点

## 四、实现步骤

### Phase 1: 高优先级卡牌（第7-11张）

| 步骤 | 卡牌 | 文件路径 | 预期效果 |
|:---:|------|---------|---------|
| 1 | 我会好好做的 | Scripts/Cards/Attack/WoHuiHaoHaoZuoDeCard.cs | 造成伤害，基础伤害永久提升，消耗 |
| 2 | 清空枪械 | Scripts/Cards/Skill/QingKongDanJiaCard.cs | 根据枪械层数获得能量/抽牌/力量 |
| 3 | 音爆 | Scripts/Cards/Attack/YinBaoCard.cs | 全体伤害，施加易伤和虚弱，消耗 |
| 4 | 杀完了 | Scripts/Cards/Skill/ShaWanLeCard.cs | 全体伤害，施加易伤并翻倍 |
| 5 | 血溅 | Scripts/Cards/Attack/XueJianCard.cs | 造成伤害，根据易伤获得格挡 |

### Phase 2: 中优先级卡牌（第12-18张）

继续实现剩余卡牌...

### Phase 3: 低优先级卡牌（第19-28张）

实现剩余复杂卡牌...

## 五、验证流程

1. 每实现一张卡牌后执行 `dotnet build` 检查编译错误
2. 更新 `进度.md` 标记完成状态
3. 在 `杀戮尖塔2接口文档.md` 记录实现经验和难点

## 六、风险评估

| 风险 | 描述 | 应对策略 |
|------|------|---------|
| 复杂能力效果 | 如姿态检测、自定义能力等需要深入了解API | 先实现基础功能，留下注释 |
| API不熟悉 | 某些命令和方法可能使用不正确 | 参考原版卡牌实现 |
| 编译错误 | 命名空间引用、类型转换等问题 | 逐张卡牌构建验证 |

## 七、依赖文件

- `Scripts/Base/AbstractTemplateBaseCard.cs` - 基础模板类
- `code/Cards/` - 原版卡牌参考
- `code/cards.json` - 卡牌配置参考
- `进度.md` - 进度跟踪
- `杀戮尖塔2接口文档.md` - 技术文档