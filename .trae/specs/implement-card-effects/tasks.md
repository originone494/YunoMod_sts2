# Tasks

## 第一阶段：基础数值卡牌（简单伤害/格挡/状态）

- [x] Task 1: 初始攻击/防御卡牌 - 打击(DaJi/YunoStrikeCard)、防御(FangYu/YunoDefendCard)
  - 补全 Play 方法：正确处理伤害/格挡的 DynamicVar
  - 实现 Upgrade：数值提升

- [x] Task 2: 初始特殊卡牌 - 掩护射击(YanHuSheJiCard)、出血/刺伤(ChuXueCard?)
  - 实现枪械标签、虚弱赋予、流血赋予逻辑
  - Upgrade 效果：次数/层数提升

- [x] Task 3: 简单罕见攻击卡 - 从左边而来(CongZuoBianErLaiCard)、我会好好做的(WoHuiHaoHaoZuoDeCard)
  - 实现预见(Scry)、抽牌、伤害转移等效果
  - Upgrade 效果：数值提升

- [x] Task 4: 罕见攻击卡 - 音爆(YinBaoCard)、杀完了(ShaWanLeCard)、血溅(XueJianCard)
  - 全体状态赋予、AOE 伤害、易伤联动格挡逻辑
  - Upgrade 效果：层数/数值提升

- [x] Task 5: 罕见技能/能力卡 - 虚诈(XuZhaCard)、捷音(JieYinCard)、保护你(BaoHuNiCard)
  - 重伤(重伤)转移、流血格挡联动、爱意联动
  - Upgrade：去消耗、次数提升等

- [x] Task 6: 罕见技能/能力卡 - 刺突(CiTuCard)、亮刃(LiangRenCard)、暗中观察(AnZhongGuanChaCard)、无意中听(WuYiZhongTingCard)
  - 弃牌堆交互、检索刃牌、爱意/姿态联动
  - Upgrade 效果：数值/次数提升、减费

## 第二阶段：交互类卡牌（抽牌/弃牌/选择性）

- [x] Task 7: 罕见卡牌 - 去看星星(QuKanXingXingCard)、就是你(JiuShiNiCard)、未来偏差(WeiLaiPianChaCard)、电击枪(DianJiQiangCard)
  - 弃牌获得爱意、伤害倍率、生命阈值效果、状态翻倍
  - Upgrade 效果：数值提升、减费

- [x] Task 8: 罕见技能 - 由乃救我(YouNaiJiuWoCard)、解毒剂(JieDuJiCard)、交换(JiaoHuanCard)、看破(KanPoCard)
  - 高格挡+预见、中毒计数返还费用、力量/敏捷交换、姿态联动格挡
  - Upgrade 效果：格挡提升、减费

- [x] Task 9: 罕见卡牌 - 绝境(JueJingCard)、爱意溢出(AiYiYiChuCard)、占有欲(ZhanYouYuCard)、血败(XueBaiCard)
  - 低血免费/疯狂免疫/伤害百分比调整/流血联动重伤
  - Upgrade 效果：抽牌增加、减费

- [x] Task 10: 罕见卡牌 - 别想逃(BieXiangTaoCard)、挑飞(TiaoFeiCard)、你说什么(NiShuoShenMeCard)、邂逅(XieHouCard)
  - 弃牌堆捞牌/保留/流血触发+抽牌/爱意减费
  - Upgrade：捞牌数+1、格挡提升、抽牌

- [x] Task 11: 罕见+稀有卡牌 - 我推(WoTuiCard)、我锤(WoChuiCard)、失衡(ShiHengCard)、偏执(PianZhiCard)
  - 爱意获取/力量临时降低/敏捷力量取高者/消耗牌获力量
  - Upgrade：数值提升、减费

## 第三阶段：复杂效果卡牌

- [x] Task 12: 普通卡牌 - 闪光弹(ShanGuangDanCard)、迫停(PoStop/PoTingCard)、超负荷(ChaoFuHeCard)、弱点击破(RuoDianJiPoCard)
  - 未造成伤害条件抽牌/枪械+检索攻击牌/抽弃牌/手牌保留成长
  - Upgrade 效果：抽牌数提升、次数提升

- [ ] Task 13: 普通卡牌 - 失手(ShiShouCard)、拿起未来(NaQiWeiLaiCard)、殉情(XunQingCard)、扫射(SaoSheCard)
  - 手牌数量联动下回合抽牌/生成未来讯息/格挡+爱意/全体多次枪械
  - Upgrade 效果：数值提升

- [ ] Task 14: 普通卡牌 - 补刀(BuDaoCard)、胁迫(XiePoCard)、斧击(FuJiCard)、杀了你(ShaLiaoNiCard)
  - 低血量斩杀/易伤追加伤害/易伤赋予/弃牌堆回手成长
  - Upgrade 效果：斩杀阈值增加、数值提升

- [ ] Task 15: 普通卡牌 - 厮杀(SiShaCard)、得手(DeShouCard)、相信我(XiangXinWoCard)、和计划一样(HeJiHuaYiYangCard)
  - 随机多次伤害/姿态联动格挡/选择(爱意或伤害)/预见联动
  - Upgrade 效果：次数提升、数值提升

- [ ] Task 16: 普通卡牌 - 信你一次(XinNiYiCiCard)、走吧(ZouBaCard)、胜者未来(ShengZheWeiLaiCard)
  - 格挡+未来讯息/获得费用/预见并增伤
  - Upgrade：格挡提升、费用+1、预见提升

- [ ] Task 17: 无色卡牌 - 球体(QiuTiCard)、未来讯息(WeiLaiXunXiCard)
  - 日记联动伤害/预见抽牌消耗
  - Upgrade 效果：数值提升、抽牌+1

- [ ] Task 18: 稀有卡牌 - 正义执行(ZhengYiZhiXingCard)、逆向命运(NiXiangMingYunCard)、BadEnd(BadEndCard)
  - 自伤AOE+包扎/大量预见+抽牌/自伤减血出牌
  - Upgrade 效果：数值提升、减费

- [ ] Task 19: 稀有卡牌 - 一击必杀(YiJiBiShaCard)、去死(QuSiCard)、淬血(CuiXueCard)、自伤(ZiShangCard)
  - 斧头高伤+费/检索匕首/流血触发/自伤过牌
  - Upgrade 效果：伤害提升、去消耗、抽牌增加

- [ ] Task 20: 稀有卡牌 - 你没用了(NiMeiYongLeCard)、陷机(XianJiCard)、威胁(WeiXieCard)
  - 击杀加最大生命/抽牌联动流血/标记伤害返还
  - Upgrade 效果：生命提升、层数提升、减费

- [ ] Task 21: 稀有卡牌 - 因果律大殿堂(YinGuoLvDaDianTangCard)、孤望(GuWangCard)、终焉(ZhongYanCard)、孤独走向永远(GuDuZouXiangYongYuanCard)
  - 日记联动+生成球体/爱意+高伤/状态联动伤害
  - Upgrade 效果：减费、伤害提升

- [ ] Task 22: 稀有卡牌 - 横切(HengQieCard)、预知(YuZhiCard)、爱慕(AiMuCard)、你找死(NiZhenZhaoSICard)
  - 流血翻倍AOE/预见联动力量/抽牌获爱意/进入疯狂
  - Upgrade 效果：减费、增加效果

- [ ] Task 23: 稀有卡牌 - 这一餐献给你(ZheYiCanXianGeiNiCard)、下一个就是你(XiaGeYiGeJiuShiNiCard)、紧急正义呼唤(JinJiZhengYiHuHuanCard)
  - 免易伤虚弱/回合重伤+费/低血最大生命值伤害
  - Upgrade 效果：减费

- [ ] Task 24: 稀有卡牌 - 以牙还牙(YiYaHuanYaCard)、包扎(BaoZaCard)、刀轮舞(DaoLunWuCard)、预知梦(YuZhiMengCard)、枪械精通(QiangXieJingTongCard)
  - 复仇效果/包扎Power/每牌打小刀/预见0费/枪械姿态强化
  - Upgrade 效果：减费

- [ ] Task 25: 遗漏卡牌补充（确保护所有进度.md中的卡牌都被覆盖）
  - 检查是否遗漏了某张卡牌
  - DuQiCard(毒气) - 中毒Power升级
  - TianChong(填冲/瞄准射击) - MiaoZhunSheJiCard费用联动
  - 乘胜追击(ChengShengZhuiJiCard) - 预见回手成长
  - 以假乱真(YiJiaLuanZhenCard) - 已完成，跳过

- [ ] Task 26: 最终构建验证 + 进度.md更新 + 接口文档补充
  - 构建项目，确保所有语法无误
  - 更新进度.md中对应卡牌的完成状态
  - 在杀戮尖塔2接口文档.md中记录所有API使用经验

## Task Dependencies
- Tasks 1-25 之间无直接依赖，可按任意顺序实现
- Task 26 依赖所有 1-25 完成
