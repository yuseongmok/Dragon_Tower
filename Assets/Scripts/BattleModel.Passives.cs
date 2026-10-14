using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  public bool NewPassives=>!Dragon.legacyBalance&&Dragon.combatRulesVersion>=3;
  public int PassiveLevel=>PassiveImprintRules.Clamp(Dragon.passiveImprintLevel);
  public int PassiveProcCount{get;private set;}
  public int PassiveRollCount{get;private set;}
  public int PassiveDamage{get;private set;}
  public int PassiveHealing{get;private set;}
  public int AuroraStacks=>NewPassives&&Time<auroraUntil?auroraStacks:0;
  public double TentacleRemaining=>Math.Max(0,tentacleUntil-Time);
  public bool BossPassiveStunProtected=>NewPassives&&IsPassiveBoss&&(EnemyStunned||Time<bossStunGuardUntil||bossNeedsAttack);
  public double BossStunGuardRemaining=>Math.Max(0,bossStunGuardUntil-Time);
  int auroraStacks,lastPassiveSkillCast=-1;double auroraUntil,bossStunGuardUntil,tentacleUntil,tentacleTick,tentacleLast;
  bool bossNeedsAttack;double tentacleFraction;
  public bool EnableSolarRecoveryCooldown{get;set;} // Diagnostic candidate, production deliberately leaves disabled.
  double solarRecoveryReady;
  bool IsPassiveBoss=>Enemy.isBoss||Enemy.bossPattern!=EnemyBossPattern.None;
  float PassiveBasis=>Dragon.baseAttackDamage>0?Dragon.baseAttackDamage:Dragon.attackDamage;
  int CurrentAttack=>NewPassives?(int)Math.Round(Dragon.attackDamage*(1+AuroraStacks*.02),MidpointRounding.AwayFromZero):Dragon.attackDamage;
  bool OwnPassiveWard=>NewPassives&&augmentWards.TryGetValue("passive:iron",out var w)&&w.hp>0&&Time<w.until;
  float PassiveDirectBonus(bool skill)=>!NewPassives?0:(OwnPassiveWard?10:0)+(skill&&Dragon.passiveMechanic==DragonPassiveMechanic.HardenedShell?PassiveImprintRules.TitanBonus(PassiveLevel,Dragon.maxHP,Dragon.passiveBaseMaxHP):0);
  bool PassiveRoll(){float p=PassiveImprintRules.Chance(Dragon.passiveMechanic,PassiveLevel);if(p<=0)return false;PassiveRollCount++;if(!Roll(p))return false;PassiveProcCount++;return true;}
  void InitializeNewPassives(){if(!NewPassives)return;if(Dragon.passiveMechanic==DragonPassiveMechanic.HolyLight&&!Dragon.passiveCooldownApplied){float basis=Dragon.baseSkillCooldown>0?Dragon.baseSkillCooldown:Dragon.skill.cooldown;Dragon.skill=Dragon.skill.Copy();Dragon.skill.cooldown=Dragon.skill.cooldown<=0?0:Math.Max(basis*.4f,Dragon.skill.cooldown-basis*PassiveImprintRules.CooldownReduction(PassiveLevel)/100);Dragon.passiveCooldownApplied=true;}if(Dragon.passiveMechanic==DragonPassiveMechanic.HydraVenom){SetSourceDot(true,"passive:venom",PassiveBasis*PassiveImprintRules.DotPercent(PassiveLevel)/100.0,PassiveImprintRules.PoisonPeriod(PassiveLevel),double.PositiveInfinity);PassiveProcCount++;}}
  void NewPassiveCast(){if(!NewPassives)return;switch(Dragon.passiveMechanic){
   case DragonPassiveMechanic.VoidAccelerator:if(Time<SkillReady)PassiveProcCount++;break;
   case DragonPassiveMechanic.JetStream:AttackReady=Time+Math.Max(0,AttackReady-Time)*(1-PassiveImprintRules.Value(PassiveLevel,15,40)/100);PassiveProcCount++;break;
   case DragonPassiveMechanic.Tentacle:if(PassiveRoll()){if(tentacleUntil<=Time){tentacleLast=Time;tentacleTick=Time+1;}tentacleUntil=Time+PassiveImprintRules.Duration(Dragon.passiveMechanic,PassiveLevel);}break;
   case DragonPassiveMechanic.IronArmor:if(PassiveRoll())AddWard("passive:iron",PercentHP(10),5);break;
   case DragonPassiveMechanic.Phoenix:if((!EnableSolarRecoveryCooldown||Time>=solarRecoveryReady)&&PassiveRoll()){int before=PlayerHP;HealPercent(PassiveImprintRules.Value(PassiveLevel,10,30));PassiveHealing+=PlayerHP-before;solarRecoveryReady=Time+3;}break;
  }}
  void NewPassiveSkillHit(int damage){if(!NewPassives||damage<=0||Result!=BattleResult.Fighting||lastPassiveSkillCast==augmentCastSerial)return;lastPassiveSkillCast=augmentCastSerial;
   if(Dragon.passiveMechanic==DragonPassiveMechanic.StormAffinity){if((Dragon.skill.elementType==ElementType.Wind||Dragon.skill.elementType==ElementType.Lightning)&&PassiveRoll())ApplyParalyze(2);return;}
   NewAttackStatus(false);
  }
  void NewAttackStatus(bool basic){if(!NewPassives||Result!=BattleResult.Fighting)return;var k=Dragon.passiveMechanic;
   if(k==DragonPassiveMechanic.ElectricShock&&!basic)return;
   if(k!=DragonPassiveMechanic.FlameBreath&&k!=DragonPassiveMechanic.FreezingGaze&&k!=DragonPassiveMechanic.TimeRift&&k!=DragonPassiveMechanic.ElectricShock)return;
   if(k==DragonPassiveMechanic.TimeRift&&BossPassiveStunProtected)return;
   if(!PassiveRoll())return;float duration=PassiveImprintRules.Duration(k,PassiveLevel);
   switch(k){case DragonPassiveMechanic.FlameBreath:SetSourceDot(false,"passive:ember",PassiveBasis*PassiveImprintRules.DotPercent(PassiveLevel)/100.0,1,StatusDuration(duration));break;
    case DragonPassiveMechanic.FreezingGaze:ApplySlow(duration,20);break;
    case DragonPassiveMechanic.TimeRift:ApplyStun(duration);if(IsPassiveBoss){bossStunGuardUntil=stunUntil+2;bossNeedsAttack=true;}break;
    case DragonPassiveMechanic.ElectricShock:ApplyParalyze(1);break;}
  }
  int NewBasicExtra(int original){if(!NewPassives||original<=0||Result!=BattleResult.Fighting)return 0;NewAttackStatus(true);if(Dragon.passiveMechanic!=DragonPassiveMechanic.RuyiOrb||!PassiveRoll())return 0;int hit=DamageEnemy((int)Math.Round(original*.30,MidpointRounding.AwayFromZero));PassiveDamage+=hit;if(EnemyHP==0)Result=BattleResult.Victory;return hit;}
  void NewDodgeSuccess(){if(!NewPassives||Dragon.passiveMechanic!=DragonPassiveMechanic.Mirage)return;auroraStacks=Math.Min(5,AuroraStacks+1);auroraUntil=Time+PassiveImprintRules.Duration(Dragon.passiveMechanic,PassiveLevel);PassiveProcCount++;}
  void NewDodgeFailure(){if(NewPassives&&Dragon.passiveMechanic==DragonPassiveMechanic.Mirage){auroraStacks=0;auroraUntil=0;}}
  void NewEnemyAttackResolved(bool normal,bool paralyzed){if(NewPassives&&normal&&!paralyzed&&Time>=stunUntil)bossNeedsAttack=false;}
  int IncomingElementDamage(int raw){if(!NewPassives||Dragon.passiveMechanic!=DragonPassiveMechanic.FrozenBlade||Enemy.elementType!=ElementType.Lightning||!ElementRules.HasAdvantage(Enemy.elementType,Dragon.elementType))return ElementRules.Damage(raw,Enemy.elementType,Dragon.elementType);double extra=ElementRules.Damage(raw,Enemy.elementType,Dragon.elementType)-raw;return raw+(int)Math.Round(extra*(1-PassiveImprintRules.Value(PassiveLevel,0,50)/100.0),MidpointRounding.AwayFromZero);}
  float AutoItemCooldown(float cd)=>Math.Max(0,cd)*(1-(NewPassives&&Dragon.passiveMechanic==DragonPassiveMechanic.HolyLight?Math.Min(60,PassiveImprintRules.CooldownReduction(PassiveLevel)):0)/100f);
  void TickNewPassives(){if(!NewPassives)return;
   TickSourceDots();if(Result!=BattleResult.Fighting)return;
   if(tentacleUntil<=tentacleLast)return;double end=Math.Min(Time,tentacleUntil);if(end>tentacleLast){tentacleFraction+=(end-tentacleLast)*PassiveBasis*.2;tentacleLast=end;}
   if(Time+1e-8>=tentacleTick||Time+1e-8>=tentacleUntil){int amount=(int)Math.Floor(tentacleFraction+1e-6);tentacleFraction-=amount;int hit=DamageEnemy(amount);PassiveDamage+=hit;if(hit>0)Feedback?.Invoke("촉수 피해 −"+hit);while(tentacleTick<=Time+1e-8)tentacleTick+=1;if(EnemyHP==0)Result=BattleResult.Victory;}
  }
 }
}
