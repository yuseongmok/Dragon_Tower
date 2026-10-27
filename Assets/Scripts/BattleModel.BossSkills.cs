using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  BossSkillDefinition[] bossSkills;double[] bossReady;int bossIndex=-1,bossCursor,bossParalysisFailures;
  double bossStart,bossEnd,bossPhaseStart,bossStunDelay,bossWaitStart=-1;
  double bossStormUntil;float bossStormPenalty;bool stormDodge;
  double bossCoreCharge;float bossCoreRate,bossCoreDodgeReduction;int bossShotTotal;
  public bool VoidClawMirrored {get;private set;}
  int crossReverseAfter=99;
  public float CrossRotationQuarter {
   get {int completed=Math.Max(0,BossShotNumber-1);float progress=BossState==BossSkillState.Travel?BossPhaseProgress:BossState==BossSkillState.Recovery?1:0;
    return (completed<=crossReverseAfter?completed:2*crossReverseAfter-completed)+(completed>=crossReverseAfter?-progress:progress);}
  }
  public bool CrossRotatingCounterclockwise=>Math.Max(0,BossShotNumber-1)>=crossReverseAfter;
  int frostWeaknessCap;float frostWeaknessPercent;
  public bool VoidBossBattle=>Dragon.combatRulesVersion>=13&&!Dragon.legacyBalance&&Enemy.bossPattern==EnemyBossPattern.BarkanBoundary&&HasBossSkills;
  public bool VoidExpandedBattle=>VoidBossBattle&&Dragon.combatRulesVersion>=14;
  public bool VoidBoundaryCollapsed {get;private set;}
  public double VoidBoundaryCollapsedAt {get;private set;}=-100;
  bool voidCastCollapsed;double voidNextPatternAt;
  public int BossPlannedShots=>bossShotTotal;
  public bool FrostPalaceBattle=>Dragon.combatRulesVersion>=10&&!Dragon.legacyBalance&&Enemy.bossPattern==EnemyBossPattern.FrostRecovery&&HasBossSkills;
  public int FrostWeaknessStacks {get;private set;}
  public int BossBarrierHealAmount {get;private set;}
  public double FrostCrystalReflectedAt {get;private set;}=-100;
  public bool SentryCoreBattle=>Dragon.combatRulesVersion>=9&&!Dragon.legacyBalance&&Enemy.bossPattern==EnemyBossPattern.SentryParalysis&&HasBossSkills;
  public float BossCoreCharge=>(float)bossCoreCharge;
  public int BossCoreDodges {get;private set;}
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
  public double BossDamageRemaining=>!BossSkillBusy?0:ActiveBossSkill.stormDuration>0||BossState==BossSkillState.Recovery?0:ActiveBossSkill.kind==BossAttackKind.Gimmick&&BossBarrierOutcome!=BossGimmickOutcome.Failed?BossPhaseRemaining+(BossBarrierActive?0:ActiveBossSkill.barrierWindow):BossState==BossSkillState.Travel?(ActiveBossSkill.damageOnLaunch?0:BossPhaseRemaining):BossPhaseRemaining+(BossState==BossSkillState.Warning||BossState==BossSkillState.Interval?ShotPreparation(ActiveBossSkill):0)+(ActiveBossSkill.damageOnLaunch?0:ShotTravel(ActiveBossSkill));
  public bool BossPreparation=>BossSkillBusy&&(BossState==BossSkillState.Warning||BossState==BossSkillState.Preparation);
  public bool BossResistingStun=>BossPreparation&&EnemyStunned&&bossStunDelay>=1.5-1e-8;
  public string BossSkillWarning=>!BossPreparation?"":(LibraryBossBattle&&ActiveBossSkill.bookDurability>0&&BossBookSpell>=0?(BossBookSpell==0?"파괴의 서":"속박의 서"):VoidExpandedBattle&&BossBarrierOutcome==BossGimmickOutcome.Failed?"봉합 붕괴":ActiveBossSkill.displayName)+(ActiveBossSkill.warningHint??(ActiveBossSkill.damageOnLaunch?" · ":" ↓ 전방 충격파 · "))+(BossResistingStun?"경직 저항":"준비 중");
  void InitializeBossSkills(){
   if(Dragon.legacyBalance||Dragon.combatRulesVersion<5)return;
   bossSkills=Array.FindAll(Enemy.bossSkills??Array.Empty<BossSkillDefinition>(),s=>s!=null&&s.Valid&&Dragon.combatRulesVersion>=s.minimumRulesVersion);
   bossReady=new double[bossSkills.Length];for(int i=0;i<bossSkills.Length;i++)bossReady[i]=Math.Max(0,bossSkills[i].firstDelay);
   if(SentryCoreBattle)foreach(var s in bossSkills){bossCoreRate=Math.Max(bossCoreRate,s.coreChargePerSecond);bossCoreDodgeReduction=Math.Max(bossCoreDodgeReduction,s.coreDodgeDischarge);}
   if(FrostPalaceBattle)foreach(var s in bossSkills){frostWeaknessCap=Math.Max(frostWeaknessCap,s.barrierWeaknessMaxStacks);frostWeaknessPercent=Math.Max(frostWeaknessPercent,s.barrierWeaknessPerStack);}
  }
  double BossStep(double step){
   // New encounters split idle clocks at scheduled events as well as skill phases.
   // Without this, waiting for the next rendered frame accumulates drift across casts.
   step=SolarStep(step);
   if(PrimordialBarrierStrikeRemaining>1e-8&&!EnemyTimeStopped)step=Math.Min(step,PrimordialBarrierStrikeRemaining);
   if(PrimordialIntroducing)return Math.Min(step,Math.Max(.000001,PrimordialIntroDuration-primordialIntroElapsed));
   if(PrimordialTransitioning)return Math.Min(step,Math.Max(.000001,primordialTransitionUntil-Time));
   if((VoidExpandedBattle||PrimordialBossBattle)&&!BossSkillBusy&&voidNextPatternAt>Time+1e-8)step=Math.Min(step,voidNextPatternAt-Time);
   if(BookBindingActive&&bookSlowUntil>Time+1e-8)step=Math.Min(step,bookSlowUntil-Time);
   if((SentryCoreBattle||FrostPalaceBattle||LibraryBossBattle||SolarBossBattle||VoidBossBattle||PrimordialBossBattle)&&!BossSkillBusy&&!EnemyTimeStopped){
    if(NextEnemyStrike>Time+1e-8)step=Math.Min(step,NextEnemyStrike-Time);
    for(int i=0;i<bossReady.Length;i++){
     if(bossReady[i]>Time+1e-8)step=Math.Min(step,bossReady[i]-Time);
     double deadline=bossSkills[i].firstDelay+bossSkills[i].maximumInitialAttackWait;
     if(deadline>Time+1e-8)step=Math.Min(step,deadline-Time);
    }
   }
   if(SentryCoreBattle&&!EnemyTimeStopped&&bossCoreRate>0&&bossCoreCharge<100-1e-8&&!(BossSkillBusy&&ActiveBossSkill.requiresFullCore))step=Math.Min(step,(100-bossCoreCharge)/bossCoreRate);
   if(bossStormUntil>Time+1e-8)step=Math.Min(step,bossStormUntil-Time);
   if(!BossSkillBusy||EnemyTimeStopped||BossPreparation&&EnemyStunned&&bossStunDelay<1.5)return step;
   return bossEnd>Time+1e-8?Math.Min(step,bossEnd-Time):step;
  }
  void AdvanceBossClock(double delta){
   AdvanceSolarClock(delta);
   if(SentryCoreBattle&&!EnemyTimeStopped&&!(BossSkillBusy&&ActiveBossSkill.requiresFullCore))bossCoreCharge=Math.Min(100,bossCoreCharge+delta*bossCoreRate);
   if(!BossSkillBusy)return;
   NextEnemyStrike+=delta; // Normal attacks have no advancing gauge during skill/recovery.
   if(EnemyTimeStopped){if(PrimordialSpectacle&&primordialBarrierStrikeAt>=0)primordialBarrierStrikeAt+=delta;bossEnd+=delta;bossPhaseStart+=delta;return;}
   if(BossPreparation&&ActiveBossSkill.interruptible&&EnemyStunned){
    double hold=Math.Min(delta,Math.Min(Math.Max(0,stunUntil-Time),Math.Max(0,1.5-bossStunDelay)));
    bossEnd+=hold;bossPhaseStart+=hold;bossStunDelay+=hold;
   }
  }
  bool BossConditionMet(BossSkillDefinition s)=>((s.attackVfx!="contract_apocalypse"&&s.attackVfx!="contract_apocalypse_collision")||!PlayerStunned)&&PrimordialCanSelect(s)&&(!PrimordialBossBattle||s.requiredBossPhase==0||s.requiredBossPhase==PrimordialPhase)&&(enemyAttackCount>=s.minimumNormalAttacks||Time>=s.firstDelay+s.maximumInitialAttackWait)&&EnemyHP*100.0/Math.Max(1,CurrentEnemyMaxHP)>=s.minHPPercent&&EnemyHP*100.0/Math.Max(1,CurrentEnemyMaxHP)<=s.maxHPPercent;
  void BossPhase(BossSkillState state,double duration){BossState=state;bossPhaseStart=Time;bossEnd=Time+duration;}
  void TickBossSkills(){
   // Only the outstanding fraction of a storm dodge is normalized on expiry.
   // Dragon stats (including augment reductions) are never mutated.
   if(stormDodge&&Time+1e-8>=bossStormUntil){
    if(DodgeReady>Time){DodgeReady=Time+(DodgeReady-Time)/(1+bossStormPenalty);if(!DodgeCooldownReleased)DodgeReady=Math.Max(DodgeReady,DodgeUntil+.15);}
    stormDodge=false;
   }
   if(!HasBossSkills||Result!=BattleResult.Fighting||EnemyTimeStopped||PrimordialCinematic)return;
   if(bossIndex<0){
    // Latch between casts; never change an already telegraphed attack mid-flight.
    if(VoidExpandedBattle&&!VoidBoundaryCollapsed&&EnemyHP*100.0<=CurrentEnemyMaxHP*40.0){VoidBoundaryCollapsed=true;VoidBoundaryCollapsedAt=Time;}
    for(int j=0;j<bossSkills.Length;j++){
     int i=(bossCursor+j)%bossSkills.Length;var s=bossSkills[i];
     // A full core reserves the next free skill slot; it never overlaps an active attack.
     if(SentryCoreBattle&&s.requiresFullCore!=(bossCoreCharge>=100-1e-8))continue;
     if((VoidExpandedBattle||PrimordialBossBattle)&&Time+1e-8<voidNextPatternAt)continue;
     if(Time+1e-8<bossReady[i]||!BossConditionMet(s)||!SolarCanStart(s))continue;
     // Finish an already telegraphed normal strike before selecting a skill.
     if(NextEnemyStrike-Time<=enemyWindupDuration)continue;
     if(bossWaitStart<0)bossWaitStart=Time;
     if(EnemyStunned&&(s.requiresUnstunnedStart||Time-bossWaitStart<1.5))continue;
     if(!s.statusOnly&&Time+s.warningTime+s.preparationTime+(s.damageOnLaunch?0:s.damageTime)<DodgeReady+.25)continue;
     BossState=BossSkillState.Selection;bossIndex=i;bossCursor=(i+1)%bossSkills.Length;
     if(s.id=="first_contract_end"){FirstContractEndUsed=true;FirstContractEndSuccesses=0;bigBangPressure=0;}
     if(PrimordialSpectacle&&s.oncePerEncounter)PrimordialFinaleUsed=true;
     primordialBarrierStrikeAt=-1;primordialBarrierStrikeDone=false;
     bossStart=Time;bossStunDelay=0;bossWaitStart=-1;BossCastCount++;BossSkillHasImpacted=false;BossBarrierOutcome=BossGimmickOutcome.None;BossBarrierHealAmount=0;
     bossShotTotal=s.sequenceShots>0?(s.followupChance>0?(Roll(s.followupChance)?s.sequenceShots:1):s.sequenceShots):s.directionalShots>0?s.directionalShots:s.illusionShots>0?s.illusionShots:s.lowHPShots>1&&EnemyHP*100.0/Math.Max(1,CurrentEnemyMaxHP)<=s.lowHPShotThreshold?s.lowHPShots:1;
     voidCastCollapsed=VoidExpandedBattle&&VoidBoundaryCollapsed;
     if(voidCastCollapsed&&s.id=="void_cut")bossShotTotal=s.sequenceShots;
     crossReverseAfter=s.crossReversals&&random()<.65?(random()<.5?3:5):99;
     BossShotNumber=s.sequenceShots>0||s.directionalShots>0||s.lowHPShots>0||s.illusionShots>0?1:0;if(s.directionalShots>0)ChooseBossDirection();
     if(LibraryBossBattle){BossBookSpell=-1;BossBrokenBook=-1;BossIllusionFake=false;realIllusionShots=0;ChooseIllusion(s);}
     dodgeAttemptedForStrike=false;BossPhase(BossSkillState.Warning,s.warningTime*(s.kind==BossAttackKind.Gimmick?1+Math.Min(.25,EnemySlowPower/100.0):1));
     return;
    }
    BossState=BossCastCount==0?BossSkillState.Idle:BossSkillState.Cooldown;return;
   }
   TickPrimordialBarrierStrike();
   if(Time+1e-8<bossEnd)return;
   var skill=ActiveBossSkill;
   switch(BossState){
    case BossSkillState.Warning:
     if(SolarBossBattle&&skill.solarMirror)LaunchBossSkill(skill);
     else if(LibraryBossBattle&&skill.bookDurability>0)LaunchBossSkill(skill);
     else if(skill.kind==BossAttackKind.Gimmick)LaunchBossSkill(skill);
     else if(skill.stormDuration>0)LaunchBossSkill(skill);
     else BeginBossShotWarning(skill);break;
    case BossSkillState.Interval:
     if(LibraryBossBattle&&skill.bookDurability>0){BeginBookSpell();break;}
     BossShotNumber++;if(skill.directionalShots>0)ChooseBossDirection();if(LibraryBossBattle)ChooseIllusion(skill);bossStunDelay=0;BeginBossShotWarning(skill);break;
    case BossSkillState.BookChoice:
     FinishBookChoice();break;
    case BossSkillState.Preparation:
     // A front blade never launches while the current cooldown leaves no response window.
     // Once launched, a mistimed/early dodge is still a legitimate player error.
     if(skill.directionalShots>0&&BossDirection==BossBladeDirection.Front&&!DodgeCooldownReleased&&Time+ShotTravel(skill)<DodgeReady+.25){bossEnd=DodgeReady+.25-ShotTravel(skill);break;}
     LaunchBossSkill(skill);break;
    case BossSkillState.Barrier:
     FinishBossBarrier(false);break;
    case BossSkillState.Travel:
     if(!skill.damageOnLaunch&&!skill.solarMirror)ResolveBossSkillImpact(skill);
     if(skill.attackVfx=="contract_end"&&BossShotNumber==bossShotTotal&&FirstContractEndSuccesses==bossShotTotal){
      stunUntil=Math.Max(stunUntil,Time+2);Feedback?.Invoke("계약의 핵 붕괴 · 공격 기회");
     }
     if(skill.attackVfx=="contract_continuous_cross"&&bossShotTotal>BossShotNumber){BossShotNumber++;ChooseBossDirection();BossPhase(BossSkillState.Travel,Math.Max(ShotTravel(skill),EffectiveDodgeCooldown*.5+.2));break;}
     if(LibraryBossBattle&&skill.bookDurability>0&&bookPendingMask!=0)BossPhase(BossSkillState.Interval,skill.interShotDelay);
     else if(bossShotTotal>BossShotNumber&&BossShotNumber>0)BossPhase(BossSkillState.Interval,skill.interShotDelay);
     else BossPhase(BossSkillState.Recovery,skill.attackVfx=="contract_end"?(FirstContractEndSuccesses==3?2:.6):skill.lingerTime+skill.recovery*(voidCastCollapsed?.75:1));break;
    case BossSkillState.Recovery:
     bossReady[bossIndex]=Math.Max(bossStart+skill.cooldown*(voidCastCollapsed?.9:1),Time);
     bossIndex=-1;BossState=BossSkillState.Cooldown;
     NextEnemyStrike=Math.Max(Time+Math.Max(Enemy.interval,Dragon.dodgeCooldown+.25),DodgeReady+.25);
     if(VoidExpandedBattle||PrimordialBossBattle)voidNextPatternAt=skill.patternRestAfter>0?Time+skill.patternRestAfter:NextEnemyStrike+.05;
     nextEnemyDamageMultiplier=1;EnemyIntent="일반 공격";enemyWindupDuration=WindupDuration;
     if(LibraryBossBattle||SolarBossBattle||VoidBossBattle||PrimordialBossBattle)TickBossSkills(); // Dispatch an already-ready spell at this event, not the next rendered frame.
     break;
   }
  }
  void ChooseBossDirection(){var s=ActiveBossSkill;if(s.attackVfx=="contract_apocalypse_collision"){BossDirection=BossShotNumber>=3||random()<.5?BossBladeDirection.Front:BossBladeDirection.Left;return;}if(s.randomClawJudgment){if(BossShotNumber==1)VoidClawMirrored=random()<.5;BossDirection=random()<.5?BossBladeDirection.Front:BossBladeDirection.Left;return;}if(s.attackVfx=="contract_spear_echo"||s.attackVfx=="contract_apocalypse"){BossDirection=random()<.5?BossBladeDirection.Front:BossBladeDirection.Left;return;}if(PrimordialSpectacle&&s.shotDirections!=null&&s.shotDirections.Length>0){BossDirection=s.shotDirections[Math.Min(s.shotDirections.Length-1,Math.Max(0,BossShotNumber-1))];return;}BossDirection=(BossBladeDirection)Math.Min(2,(int)(random()*3));}
  void BeginBossShotWarning(BossSkillDefinition skill){
   double duration=ShotPreparation(skill)*(1+Math.Min(.25,EnemySlowPower/100.0));
   // Establish a fair response window once, before this shot's ground telegraph.
   // Dodging after the telegraph starts must not postpone impact indefinitely.
   if((SentryCoreBattle||FrostPalaceBattle||LibraryBossBattle||SolarBossBattle||VoidBossBattle||PrimordialBossBattle)&&!DodgeCooldownReleased)duration=Math.Max(duration,DodgeReady+.25-Time-ShotTravel(skill));
   BossPhase(BossSkillState.Preparation,duration);
  }
  void LaunchBossSkill(BossSkillDefinition skill){
   // Shared launch rule for skills and timed barriers; no second scheduler.
   if(EnemyParalyzed&&bossParalysisFailures<2&&Roll(20)){bossParalysisFailures++;Feedback?.Invoke("마비! "+skill.displayName+" 발동 실패");BossPhase(BossSkillState.Recovery,skill.recovery);return;}
   bossParalysisFailures=0;
   if(SolarBossBattle&&skill.solarMirror){if(SolarPhaseRemaining<skill.damageTime+SolarTransitionWarning){BossPhase(BossSkillState.Recovery,skill.recovery);return;}solarMirrorSerial++;BossPhase(BossSkillState.Travel,ShotTravel(skill));return;}
   if(LibraryBossBattle&&skill.bookDurability>0&&BossState==BossSkillState.Warning){StartBookChoice(skill);return;}
   if(skill.stormDuration>0){bossStormUntil=Time+skill.stormDuration;bossStormPenalty=skill.dodgeCooldownPenalty;BossPhase(BossSkillState.Recovery,skill.recovery);return;}
   if((VoidExpandedBattle||PrimordialBossBattle)&&skill.barrierFailureTelegraph>0&&BossBarrierOutcome==BossGimmickOutcome.Failed){BossPhase(BossSkillState.Travel,ShotTravel(skill));return;}
   if(skill.kind==BossAttackKind.Gimmick){
    EnemyShieldMaxHP=skill.barrierDurability;
    if(FrostPalaceBattle){EnemyShieldMaxHP=Math.Max(1,(int)Math.Ceiling(EnemyShieldMaxHP*(1-Math.Min(.9f,FrostWeaknessStacks*frostWeaknessPercent/100f))));FrostWeaknessStacks=0;}
    enemyShieldHP=EnemyShieldMaxHP;enemyShieldUntil=Time+skill.barrierWindow;
    BossPhase(BossSkillState.Barrier,skill.barrierWindow);
    if(PrimordialSpectacle&&skill.barrierStrikeDelay>0)primordialBarrierStrikeAt=Math.Max(Time+skill.barrierStrikeDelay,DodgeReady+1);
   }else{BossPhase(BossSkillState.Travel,skill.attackVfx=="contract_continuous_cross"?Math.Max(ShotTravel(skill),EffectiveDodgeCooldown*.5+.2):ShotTravel(skill));if(skill.damageOnLaunch)ResolveBossSkillImpact(skill);}
  }
  void FinishBossBarrier(bool broken){
   if(!BossBarrierActive)return;var skill=ActiveBossSkill;
   BossBarrierOutcome=broken?BossGimmickOutcome.Broken:BossGimmickOutcome.Failed;
   if((VoidExpandedBattle||PrimordialBossBattle)&&skill.barrierFailureTelegraph>0){
    enemyShieldHP=0;enemyShieldUntil=0;
    if(broken)BossPhase(BossSkillState.Recovery,skill.breakRecovery);
    else{bossStunDelay=0;BossPhase(BossSkillState.Preparation,Math.Max(skill.barrierFailureTelegraph,DodgeReady+.25-Time-ShotTravel(skill)));}
    return;
   }
   BossPhase(BossSkillState.Recovery,broken?skill.breakRecovery:skill.recovery);
   if(broken){enemyShieldHP=0;enemyShieldUntil=0;}
   else if(FrostPalaceBattle&&skill.barrierFailureHealPercent>0){
    enemyShieldHP=0;enemyShieldUntil=0;int before=EnemyHP;
    EnemyHP=Math.Min(CurrentEnemyMaxHP,EnemyHP+(int)Math.Ceiling(CurrentEnemyMaxHP*skill.barrierFailureHealPercent/100f));
    BossBarrierHealAmount=EnemyHP-before;BossImpactAt=Time;BossSkillHasImpacted=true;
    Cue?.Invoke(CombatCue.BossSkill,0);Feedback?.Invoke("빙결 회복 · HP +"+BossBarrierHealAmount);
   }
   else{BossImpactAt=Time;BossImpactCount++;BossSkillHasImpacted=true;ResolveForgeBarrierFailure();}
  }
  void ResolveBossSkillImpact(BossSkillDefinition skill){
   BossImpactAt=Time;BossImpactCount++;BossSkillHasImpacted=true;
   BossLastImpactDirection=BossDirection;
   if(skill.attackVfx=="contract_big_bang"){
    ResolveEnemyStrike(Enemy.damage*skill.damageMultiplier*(1-BigBangReduction),skill.element,Time,false,skill.guardable,false);
    dodgeAttemptedForStrike=false;return;
   }
   if(skill.statusOnly){
    bool avoided=ProtectedSkillActive||skill.dodgeable&&Time<DodgeUntil;
    if(!avoided){ApplyBossSkillStatus(skill);Feedback?.Invoke(skill.displayName+" · 기절");}
    else Feedback?.Invoke(skill.displayName+" · 회피 성공");
    dodgeAttemptedForStrike=false;return;
   }
   if(LibraryBossBattle&&skill.illusionShots>0&&BossIllusionFake){dodgeAttemptedForStrike=false;return;}
   if(LibraryBossBattle&&skill.bookDurability>0&&BossBookSpell==1){bookSlowUntil=Time+skill.basicSlowDuration;bookSlowPercent=skill.basicSlowPercent;dodgeAttemptedForStrike=false;return;}
   // Side lanes are empty at rest. Only the active dodge window enters their hit area.
   // Use the normal mitigation pipeline; other invulnerability/shields remain valid.
   bool side=skill.directionalShots>0&&BossDirection!=BossBladeDirection.Front;
   if(skill.id=="first_contract_end"){
    bool correct=side?Time>=DodgeUntil:ProtectedSkillActive||Time<DodgeUntil;
    if(correct)FirstContractEndSuccesses++;
   }
   if(side&&Time>=DodgeUntil){dodgeAttemptedForStrike=false;return;}
   int before=PlayerHP;
   bool coreDodge=SentryCoreBattle&&skill.dodgeable&&!side&&!ProtectedSkillActive&&Time<DodgeUntil;
   bool crystalDodge=FrostPalaceBattle&&skill.dodgeWeakensBarrier&&skill.dodgeable&&!side&&!ProtectedSkillActive&&Time<DodgeUntil;
   ResolveEnemyStrike(skill.damage>0?skill.damage:Enemy.damage*ShotDamage(skill),skill.element,Time,skill.dodgeable&&!side,skill.guardable,false);
   if(coreDodge){bossCoreCharge=Math.Max(0,bossCoreCharge-bossCoreDodgeReduction);BossCoreDodges++;}
   if(crystalDodge){FrostWeaknessStacks=Math.Min(frostWeaknessCap,FrostWeaknessStacks+1);FrostCrystalReflectedAt=Time;}
   if(SentryCoreBattle&&skill.requiresFullCore)bossCoreCharge=0;
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





