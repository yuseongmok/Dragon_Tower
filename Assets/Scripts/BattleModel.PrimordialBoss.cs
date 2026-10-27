using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  public bool PrimordialBossBattle=>Dragon.combatRulesVersion>=15&&!Dragon.legacyBalance&&Enemy.bossPattern==EnemyBossPattern.PrimordialContract;
  public bool PrimordialAnimated=>PrimordialBossBattle&&Dragon.combatRulesVersion>=17;
  public const double PrimordialIntroDuration=4.2;
  bool primordialIntroDone;double primordialIntroElapsed;
  public bool PrimordialIntroducing=>PrimordialAnimated&&!primordialIntroDone;
  public bool PrimordialCinematic=>PrimordialIntroducing||PrimordialTransitioning;
  public float PrimordialIntroProgress=>PrimordialIntroducing?(float)Math.Max(0,Math.Min(1,primordialIntroElapsed/PrimordialIntroDuration)):1;
  public float PrimordialHealthReveal=>PrimordialIntroducing?ClampCinema((PrimordialIntroProgress-.38f)/.48f):PrimordialTransitioning&&PrimordialAnimated?ClampCinema((PrimordialTransitionProgress-.60f)/.32f):1;
  static float ClampCinema(float x)=>(float)Math.Max(0,Math.Min(1,x));
  bool TickPrimordialIntro(double delta){
   if(!PrimordialIntroducing)return false;
   primordialIntroElapsed+=delta;
   if(primordialIntroElapsed+1e-8<PrimordialIntroDuration)return true;
   primordialIntroDone=true;
   NextEnemyStrike=Time+Math.Max(Enemy.interval,EffectiveDodgeCooldown+.25);
   for(int i=0;i<bossReady.Length;i++)bossReady[i]=Time+bossSkills[i].firstDelay;
   foreach(var dot in poisonSources){dot.next=Time+dot.period;dot.initialCredit=0;}
   sourceDotTime=Time;poisonFraction=burnFraction=0;
   AttackReady=DodgeReady=Time;Feedback?.Invoke("최초의 계약자 · 전투 시작");return true;
  }
  public bool PrimordialSpectacle=>PrimordialBossBattle&&Dragon.combatRulesVersion>=16;
  public bool FirstContractEndUsed {get;private set;}
  public int FirstContractEndSuccesses {get;private set;}
  public bool PrimordialFinaleUsed {get;private set;}
  public bool PrimordialFinaleActive=>PrimordialSpectacle&&BossSkillBusy&&ActiveBossSkill.oncePerEncounter;
  public bool PrimordialRescued=>PrimordialSpectacle&&BossSkillBusy&&ActiveBossSkill.id=="void_chains"&&BossBarrierOutcome==BossGimmickOutcome.Broken;
  double primordialBarrierStrikeAt=-1;bool primordialBarrierStrikeDone;
  public double PrimordialBarrierStrikeRemaining=>PrimordialSpectacle&&BossBarrierActive&&!primordialBarrierStrikeDone&&primordialBarrierStrikeAt>=0?Math.Max(0,primordialBarrierStrikeAt-Time):-1;
  bool PrimordialCanSelect(BossSkillDefinition s){
   if(!PrimordialSpectacle)return true;
   bool endReady=PrimordialPhase==1&&!FirstContractEndUsed&&EnemyHP*10<=CurrentEnemyMaxHP*3&&Array.Exists(bossSkills,x=>x.id=="first_contract_end");
   if(s.id=="first_contract_end")return endReady&&!PlayerStunned;
   if(endReady&&!PlayerStunned)return false;
   bool finale=PrimordialPhase==2&&!PrimordialFinaleUsed&&EnemyHP*4<=CurrentEnemyMaxHP&&Array.Exists(bossSkills,x=>x.oncePerEncounter);
   return s.oncePerEncounter?finale:!finale;
  }
  float ShotValue(float[] values,float fallback)=>PrimordialSpectacle&&values!=null&&values.Length>0?values[Math.Min(values.Length-1,Math.Max(0,BossShotNumber-1))]:fallback;
  float ShotPreparation(BossSkillDefinition s)=>ShotValue(s.shotPreparationTimes,s.preparationTime);
  float ShotTravel(BossSkillDefinition s)=>ShotValue(s.shotTravelTimes,s.damageTime);
  float ShotDamage(BossSkillDefinition s)=>ShotValue(s.shotDamageMultipliers,s.damageMultiplier);
  void TickPrimordialBarrierStrike(){
   if(!PrimordialSpectacle||!BossBarrierActive||primordialBarrierStrikeDone||primordialBarrierStrikeAt<0||Time+1e-8<primordialBarrierStrikeAt)return;
   primordialBarrierStrikeDone=true;ResolveBossSkillImpact(ActiveBossSkill);
  }
  public int PrimordialPhase {get;private set;}=1;
  bool primordialPending,primordialCleaned;
  double primordialTransitionUntil,primordialTransitionStart;
  public bool PrimordialTransitioning=>PrimordialBossBattle&&primordialPending;
  public float PrimordialTransitionProgress=>primordialPending?(float)Math.Max(0,Math.Min(1,(Time-primordialTransitionStart)/(PrimordialAnimated?5.2:3))):1;
  void GatePrimordialDefeat(){
   if(!PrimordialBossBattle||PrimordialPhase!=1||primordialPending||EnemyHP>0)return;
   // Keep the encounter alive. No kill event, reward or overflow can cross the phase boundary.
   EnemyHP=1;primordialPending=true;primordialCleaned=false;primordialTransitionStart=Time;primordialTransitionUntil=Time+(PrimordialAnimated?5.2:3);
   bossIndex=-1;BossState=BossSkillState.Idle;BossSkillHasImpacted=false;BossBarrierOutcome=BossGimmickOutcome.None;
   enemyShieldHP=0;enemyShieldUntil=0;NextEnemyStrike=primordialTransitionUntil+Enemy.interval;
   Feedback?.Invoke("봉인이 무너집니다 · 최초의 계약자가 당신을 보호합니다");
  }
  bool TickPrimordialTransition(double delta){
   if(!PrimordialTransitioning)return false;
   // Cleanup occurs outside damage-source enumerations, including poison and item callbacks.
   if(!primordialCleaned){primordialCleaned=true;if(PrimordialAnimated){itemEvents.Clear();itemHits.Clear();itemLaunches.Clear();}pendingSkillHits=0;ScheduledSkillHits=0;CancelProtectedSkill();CancelTimeDomain();CancelCelestial();CancelPersistentAttack();CancelAttackEmpower();CancelPoisonAcceleration();CancelAugmentEffects();CancelChargedBeam();CancelSlowSignature();CancelTargetModifier();CancelCompletionDefense();CancelCompletionHeal();CancelDodgeRelease();}
   Time+=delta;
   if(Time+1e-8<primordialTransitionUntil)return true;
   primordialPending=false;PrimordialPhase=2;EnemyHP=Enemy.maxHP;
   burnUntil=stunUntil=slowUntil=paralyzeUntil=0;burnDamage=poisonDamage=0;temporaryPoisonUntil=0;
   burnSources.Clear();poisonSources.RemoveAll(s=>!s.id.StartsWith("passive:",StringComparison.Ordinal));
   foreach(var s in poisonSources){s.next=Time+s.period;s.initialCredit=0;}
   sourceDotTime=Time;poisonFraction=burnFraction=0;tentacleUntil=0;
   bossCursor=0;bossWaitStart=-1;bossStunDelay=0;bossParalysisFailures=0;BossShotNumber=0;bossShotTotal=0;BossImpactAt=-100;
   for(int i=0;i<bossReady.Length;i++)bossReady[i]=Time+bossSkills[i].firstDelay;
   NextEnemyStrike=Time+Math.Max(Enemy.interval,EffectiveDodgeCooldown+.25);voidNextPatternAt=NextEnemyStrike+.05;
   DodgeReady=Time;AttackReady=Time;nextEnemyDamageMultiplier=1;EnemyIntent="일반 공격";enemyWindupDuration=WindupDuration;
   HealPlayer((int)Math.Ceiling(Dragon.maxHP*.2));
   Feedback?.Invoke("공허에 잠식된 계약자 · 2페이즈");return true;
  }
 }
}
