```C#
//变量：
//攻击：
new DamageVar(6, ValueProp.Move)
//多次攻击：
new RepeatVar(3)
//格挡：
new BlockVar(6, ValueProp.Move)
//能力：
new PowerVar<ExplosivesPower>(1),
new PowerVar<ExplosivesPower>("FollowupExplosivePower", 1),

//攻击单体目标
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .Targeting(play.Target)
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext)

//攻击全体目标
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .TargetingAllOpponents(CombatState)
    .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
    .Execute(choiceContext);

//攻击随机目标
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .TargetingRandomOpponents(CombatState)
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext);

//多次攻击
await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
    .FromCard(this)
    .WithHitCount(DynamicVars.Repeat.IntValue)
    .WithHitFx("vfx/vfx_attack_slash")
    .Execute(choiceContext);

//获得格挡
await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);

//施加能力
await PowerCmd.Apply<ExplosivesPower>(
    Owner.Creature,
    DynamicVars[nameof(ExplosivesPower)].BaseValue,
    Owner.Creature,
    this);

//抽牌
await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

// 获得能量
await PlayerCmd.GainEnergy(int amount, Player owner);

// 失去能量
await PlayerCmd.LoseEnergy(int amount, Player owner);

// 获得金币
await PlayerCmd.GainGold(decimal amount, Player owner);
```