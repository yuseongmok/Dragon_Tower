using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  BossSkillDefinition[] bossSkills;double[] bossReady;int bossIndex=-1,bossCursor,bossParalysisFailures;
  double bossStart,bossEnd,bossPhaseStart,bossStunDelay,bossWaitStart=-1;
  double bossStormUntil;float bossStormPenalty;bool stormDodge;
  public bool GarudaDecisionBattle=>Dragon.combatRulesVersion>=8&&!Dragon.legacyBalance&&Enemy.bossPattern==EnemyBossPattern.GarudaRend&&HasBossSkills;
  public bool BossStormActive=>Result==BattleResult.Fighting&&Time<bossStormUntil;
  public double BossStormRemaining=>BossStormActive?bossStormUntil-Time:0;
  public double EffectiveDodgeCooldown=>(Dragon.dodgeCooldown>0?Dragon.dodgeCooldown:DodgeCooldown)*(BossStormActive?1+bossStormPenalty:1);
  public int BossShotNumber {get;private set;}
  public BossBladeDirection BossDirection {get;private set;}
  public BossBladeDirection BossLastImpactDirection {get;private set;}
  public BossSkillState BossState {get;private set;}
  public BossSkillDefinition ActiveBossSkill=>bossIndex<0?null:bossSkills[bossIndex];
  public bool HasBossSkills=>bossSkills!=null&&bossSkills.Length>0;
  public bool BossSkillBusy=>Result==BattleResult.Fighting&&bossIndex>=0;
  public double BossImpactAt {get;private set;}=-100;
  public int BossCastCount {get;private set;}
  public int BossImpactCount {get;private set;}
  public bool BossSkillHasImpacted {get;private set;}
  public bool BossBarrierActive=>BossSkillBusy&&BossState==BossSkillState.Barrier;
  public BossGimmickOutcome BossBarrierOutcome {get;private set;}
  public double BossPhaseRemaining=>Math.Max(0,bossEnd-Time);
  public float BossPhaseProgress=>(float)Math.Max(0,Math.Min(1,(Time-bossPhaseStart)/Math.Max(.0001,bossEnd-bossPhaseStart)));
  public double BossDamageRemaining=>!BossSkillBusy?0:ActiveBossSkill.stormDuration>0||BossState==BossSkillState.Recovery?0:ActiveBossSkill.kind==BossAttackKind.Gimmick?BossPhaseRemaining+(BossBarrierActive?0:ActiveBossSkill.barrierWindow):BossState==BossSkillState.Travel?(ActiveBossSkill.damageOnLaunch?0:BossPhaseRemaining):BossPhaseRemaining+(BossState==BossSkillState.Warning||BossState==BossSkillState.Interval?ActiveBossSkill.preparationTime:0)+(ActiveBossSkill.damageOnLaunch?0:ActiveBossSkill.damageTime);
  public bool BossPreparation=>BossSkillBusy&&(BossState==BossSkillState.Warning||BossState==BossSkillState.Preparation);
  public bool BossResistingStun=>BossPreparation&&EnemyStunned&&bossStunDelay>=1.5-1e-8;
  public string BossSkillWarning=>!BossPreparation?"":ActiveBossSkill.displayName+(ActiveBossSkill.warningHint??(ActiveBossSkill.damageOnLaunch?" · ":" ↓ 전방 충격파 · "))+(BossResistingStun?"경직 저항":"준비 중");
  void InitializeBossSkills(){
   if(Dragon.legacyBalance||Dragon.combatRulesVersion<5)return;
   bossSkills=Array.FindAll(Enemy.bossSkills??Array.Empty<BossSkillDefinition>(),s=>s!=null&&s.Valid&&Dragon.combatRulesVersion>=s.minimumRulesVersion);
   bossReady=new double[bossSkills.Length];for(int i=0;i<bossSkills.Length;i++)bossReady[i]=Math.Max(0,bossSkills[i].firstDelay);
  }
  double BossStep(double step){
   if(bossStormUntil>Time+1e-8)step=Math.Min(step,bossStormUntil-Time);
   if(!BossSkillBusy||EnemyTimeStopped||BossPreparation&&EnemyStunned&&bossStunDelay<1.5)return step;
   return bossEnd>Time+1e-8?Math.Min(step,bossEnd-Time):step;
  }
  void AdvanceBossClock(double delta){
   if(!BossSkillBusy)return;
   NextEnemyStrike+=delta; // Normal attacks have no advancing gauge during skill/recovery.
   if(EnemyTimeStopped){bossEnd+=delta;bossPhaseStart+=delta;return;}
   if(BossPreparation&&ActiveBossSkill.interruptible&&EnemyStunned){
    double hold=Math.Min(delta,Math.Min(Math.Max(0,stunUntil-Time),Math.Max(0,1.5-bossStunDelay)));
    bossEnd+=hold;bossPhaseStart+=hold;bossStunDelay+=hold;
   }
  }
  bool BossConditionMet(BossSkillDefinition s)=>(enemyAttackCount>=s.minimumNormalAttacks||Time>=s.firstDelay+s.maximumInitialAttackWait)&&EnemyHP*100.0/Math.Max(1,CurrentEnemyMaxHP)>=s.minHPPercent&&EnemyHP*100.0/Math.Max(1,CurrentEnemyMaxHP)<=s.maxHPPercent;
  void BossPhase(BossSkillState state,double duration){BossState=state;bossPhaseStart=Time;bossEnd=Time+duration;}
  void TickBossSkills(){
   // Only the outstanding fraction of a storm dodge is normalized on expiry.
   // Dragon stats (including augment reductions) are never mutated.
   if(stormDodge&&Time+1e-8>=bossStormUntil){
    if(DodgeReady>Time){DodgeReady=Time+(DodgeReady-Time)/(1+bossStormPenalty);if(!DodgeCooldownReleased)DodgeReady=Math.Max(DodgeReady,DodgeUntil+.15);}
    stormDodge=false;
   }
   if(!HasBossSkills||Result!=BattleResult.Fighting||EnemyTimeStopped)return;
   if(bossIndex<0){
    for(int j=0;j<bossSkills.Length;j++){
     int i=(bossCursor+j)%bossSkills.Length;var s=bossSkills[i];
     if(Time+1e-8<bossReady[i]||!BossConditionMet(s))continue;
     // Finish an already telegraphed normal strike before selecting a skill.
     if(NextEnemyStrike-Time<=enemyWindupDuration)continue;
     if(bossWaitStart<0)bossWaitStart=Time;
     if(EnemyStunned&&(s.requiresUnstunnedStart||Time-bossWaitStart<1.5))continue;
     if(Time+s.warningTime+s.preparationTime+(s.damageOnLaunch?0:s.damageTime)<DodgeReady+.25)continue;
     BossState=BossSkillState.Selection;bossIndex=i;bossCursor=(i+1)%bossSkills.Length;
     bossStart=Time;bossStunDelay=0;bossWaitStart=-1;BossCastCount++;BossSkillHasImpacted=false;BossBarrierOutcome=BossGimmickOutcome.None;
     BossShotNumber=s.directionalShots>0?1:0;if(BossShotNumber>0)ChooseBossDirection();
     dodgeAttemptedForStrike=false;BossPhase(BossSkillState.Warning,s.warningTime*(s.kind==BossAttackKind.Gimmick?1+Math.Min(.25,EnemySlowPower/100.0):1));
     return;
    }
    BossState=BossCastCount==0?BossSkillState.Idle:BossSkillState.Cooldown;return;
   }
   if(Time+1e-8<bossEnd)return;
   var skill=ActiveBossSkill;
   switch(BossState){
    case BossSkillState.Warning:
     if(skill.kind==BossAttackKind.Gimmick)LaunchBossSkill(skill);
     else if(skill.stormDuration>0)LaunchBossSkill(skill);
     else BeginBossShotWarning(skill);break;
    case BossSkillState.Interval:
     BossShotNumber++;ChooseBossDirection();bossStunDelay=0;BeginBossShotWarning(skill);break;
    case BossSkillState.Preparation:
     // A front blade never launches while the current cooldown leaves no response window.
     // Once launched, a mistimed/early dodge is still a legitimate player error.
     if(skill.directionalShots>0&&BossDirection==BossBladeDirection.Front&&!DodgeCooldownReleased&&Time+skill.damageTime<DodgeReady+.25){bossEnd=DodgeReady+.25-skill.damageTime;break;}
     LaunchBossSkill(skill);break;
    case BossSkillState.Barrier:
     FinishBossBarrier(false);break;
    case BossSkillState.Travel:
     if(!skill.damageOnLaunch)ResolveBossSkillImpact(skill);
     if(skill.directionalShots>BossShotNumber&&BossShotNumber>0)BossPhase(BossSkillState.Interval,skill.interShotDelay);
     else BossPhase(BossSkillState.Recovery,skill.lingerTime+skill.recovery);break;
    case BossSkillState.Recovery:
     bossReady[bossIndex]=Math.Max(bossStart+skill.cooldown,Time);
     bossIndex=-1;BossState=BossSkillState.Cooldown;
     NextEnemyStrike=Math.Max(Time+Math.Max(Enemy.interval,Dragon.dodgeCooldown+.25),DodgeReady+.25);
     nextEnemyDamageMultiplier=1;EnemyIntent="일반 공격";enemyWindupDuration=WindupDuration;break;
   }
  }
  void ChooseBossDirection(){BossDirection=(BossBladeDirection)Math.Min(2,(int)(random()*3));}
  void BeginBossShotWarning(BossSkillDefinition skill){BossPhase(BossSkillState.Preparation,skill.preparationTime*(1+Math.Min(.25,EnemySlowPower/100.0)));}
  void LaunchBossSkill(BossSkillDefinition skill){
   // Shared launch rule for skills and timed barriers; no second scheduler.
   if(EnemyParalyzed&&bossParalysisFailures<2&&Roll(20)){bossParalysisFailures++;Feedback?.Invoke("마비! "+skill.displayName+" 발동 실패");BossPhase(BossSkillState.Recovery,skill.recovery);return;}
   bossParalysisFailures=0;
   if(skill.stormDuration>0){bossStormUntil=Time+skill.stormDuration;bossStormPenalty=skill.dodgeCooldownPenalty;BossPhase(BossSkillState.Recovery,skill.recovery);return;}
   if(skill.kind==BossAttackKind.Gimmick){
    EnemyShieldMaxHP=skill.barrierDurability;enemyShieldHP=EnemyShieldMaxHP;enemyShieldUntil=Time+skill.barrierWindow;
    BossPhase(BossSkillState.Barrier,skill.barrierWindow);
   }else{BossPhase(BossSkillState.Travel,skill.damageTime);if(skill.damageOnLaunch)ResolveBossSkillImpact(skill);}
  }
  void FinishBossBarrier(bool broken){
   if(!BossBarrierActive)return;var skill=ActiveBossSkill;
   BossBarrierOutcome=broken?BossGimmickOutcome.Broken:BossGimmickOutcome.Failed;
   BossPhase(BossSkillState.Recovery,broken?skill.breakRecovery:skill.recovery);
   if(broken){enemyShieldHP=0;enemyShieldUntil=0;}
   else{BossImpactAt=Time;BossImpactCount++;BossSkillHasImpacted=true;ResolveForgeBarrierFailure();}
  }
  void ResolveBossSkillImpact(BossSkillDefinition skill){
   BossImpactAt=Time;BossImpactCount++;BossSkillHasImpacted=true;
   BossLastImpactDirection=BossDirection;
   // Side lanes are empty at rest. Only the active dodge window enters their hit area.
   // Use the normal mitigation pipeline; other invulnerability/shields remain valid.
   bool side=skill.directionalShots>0&&BossDirection!=BossBladeDirection.Front;
   if(side&&Time>=DodgeUntil){dodgeAttemptedForStrike=false;return;}
   int before=PlayerHP;
   ResolveEnemyStrike(skill.damage>0?skill.damage:Enemy.damage*skill.damageMultiplier,skill.element,Time,skill.dodgeable&&!side,skill.guardable,false);
   if(PlayerHP<before&&Result==BattleResult.Fighting)ApplyBossSkillStatus(skill);
   dodgeAttemptedForStrike=false;
  }
  // Explicit future gimmick interrupt hook; no attack/HP/story trigger is connected here.
  public bool TryInterruptBossSkill(string key){
   if(!(BossPreparation||BossSkillBusy&&BossState==BossSkillState.Interval)||!ActiveBossSkill.interruptible||string.IsNullOrEmpty(key)||key!=ActiveBossSkill.interruptKey)return false;
   BossPhase(BossSkillState.Recovery,ActiveBossSkill.recovery);return true;
  }
  void ApplyBossSkillStatus(BossSkillDefinition skill){
   double until=Time+Math.Max(0,skill.statusDuration);
   switch(skill.status){case CombatStatusEffect.Stun:playerStunUntil=Math.Max(playerStunUntil,until);break;case CombatStatusEffect.Slow:playerSlowUntil=Math.Max(playerSlowUntil,until);break;case CombatStatusEffect.Paralyze:playerParalyzeUntil=Math.Max(playerParalyzeUntil,until);break;}
  }
 }
}

