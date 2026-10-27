using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  public const double SolarNoonDuration=15,SolarMidnightDuration=10,SolarTransitionWarning=3,SolarTransitionGrace=1;
  double solarPhaseUntil=SolarNoonDuration,solarGraceUntil;bool solarStrikeLocked,solarStrikeNight,solarBasicAttempt,solarSkillReflected;
  int solarMirrorSerial,solarSkillMirrorSerial=-1;
  public bool SolarBossBattle=>Dragon.combatRulesVersion>=12&&!Dragon.legacyBalance&&Enemy.bossPattern==EnemyBossPattern.UrielReflection&&HasBossSkills;
  public int SolarPhaseIndex {get;private set;}
  public bool SolarMidnight=>SolarPhaseIndex%2==1;
  public double SolarPhaseRemaining=>Math.Max(0,solarPhaseUntil-Time);
  public double SolarPhaseDuration=>SolarMidnight?SolarMidnightDuration:SolarNoonDuration;
  public bool SolarTransitionPending=>SolarBossBattle&&SolarPhaseRemaining<=SolarTransitionWarning;
  public double SolarTransitionAt {get;private set;}=-100;
  public bool SolarAttackMidnight=>solarStrikeLocked?solarStrikeNight:SolarMidnight;
  public bool SolarMirrorActive=>SolarBossBattle&&BossSkillBusy&&ActiveBossSkill.solarMirror&&BossState==BossSkillState.Travel;
  public bool SolarMirrorPreparing=>SolarBossBattle&&BossPreparation&&ActiveBossSkill.solarMirror;
  public double SolarReflectionAt {get;private set;}=-100;
  public int SolarReflectionCount {get;private set;}
  public int SolarReflectionDamage {get;private set;}
  public double SolarAttackImpactAt {get;private set;}=-100;
  public bool SolarLastAttackMidnight {get;private set;}
  double SolarStep(double step){
   if(!SolarBossBattle||EnemyTimeStopped)return step;
   step=SolarEventStep(step,solarPhaseUntil);step=SolarEventStep(step,solarPhaseUntil-SolarTransitionWarning);step=SolarEventStep(step,solarGraceUntil);if(!BossSkillBusy)step=SolarEventStep(step,NextEnemyStrike-enemyWindupDuration);
   return step;
  }
  double SolarEventStep(double step,double due)=>due>Time+1e-8?Math.Min(step,due-Time):step;
  void AdvanceSolarClock(double delta){if(!SolarBossBattle||!EnemyTimeStopped)return;solarPhaseUntil+=delta;if(solarGraceUntil>Time)solarGraceUntil+=delta;}
  void TickSolarCycle(){
   if(!SolarBossBattle||Result!=BattleResult.Fighting||EnemyTimeStopped)return;
   // Lock the rule when this attack becomes visible, before changing the world clock.
   if(!BossSkillBusy&&!solarStrikeLocked&&NextEnemyStrike-Time<=enemyWindupDuration+1e-8){solarStrikeLocked=true;solarStrikeNight=SolarMidnight;}
   while(Time+1e-8>=solarPhaseUntil){SolarPhaseIndex++;SolarTransitionAt=solarPhaseUntil;solarPhaseUntil+=SolarPhaseDuration;solarGraceUntil=Time+SolarTransitionGrace;if(!solarStrikeLocked&&!BossSkillBusy)NextEnemyStrike=Math.Max(NextEnemyStrike,solarGraceUntil+enemyWindupDuration);}
  }
  bool SolarCanStart(BossSkillDefinition s)=>!SolarBossBattle||!s.solarMirror||SolarPhaseIndex!=1&&Time>=solarGraceUntil&&SolarPhaseRemaining>=s.warningTime+s.damageTime+s.recovery+SolarTransitionWarning&&!solarStrikeLocked;
  void SolarAttackScheduled(){if(!SolarBossBattle)return;solarStrikeLocked=false;NextEnemyStrike=Math.Max(NextEnemyStrike,solarGraceUntil+enemyWindupDuration);}
  void ArmSolarSkill(){if(!SolarBossBattle)return;solarSkillMirrorSerial=SolarMirrorActive?solarMirrorSerial:-1;solarSkillReflected=false;}
  void SolarDirectHit(bool skill,int damage){
   if(!SolarMirrorActive||damage<=0||EnemyHP<=0||Result!=BattleResult.Fighting)return;
   if(skill){if(solarSkillMirrorSerial!=solarMirrorSerial||solarSkillReflected)return;solarSkillReflected=true;}else{if(!solarBasicAttempt)return;solarBasicAttempt=false;}
   int before=PlayerHP;ApplyReflection(damage);SolarReflectionDamage=before-PlayerHP;SolarReflectionAt=Time;SolarReflectionCount++;
  }
 }
}
