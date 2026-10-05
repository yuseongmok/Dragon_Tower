using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  bool timeDomainBusy,timeDomainStarted;double timeDomainStart,timeDomainEnd;long storedBasicDamage;int storedBasicHits;
  public bool TimeDomainBusy=>timeDomainBusy&&Result==BattleResult.Fighting;
  public bool EnemyTimeStopped=>TimeDomainBusy&&timeDomainStarted&&Time<timeDomainEnd-1e-8;
  public double TimeDomainRemaining=>EnemyTimeStopped?Math.Max(0,timeDomainEnd-Time):0;
  public double TimeDomainDuration{get;private set;}
  public long StoredBasicDamage=>TimeDomainBusy?storedBasicDamage:0;
  public int StoredBasicHits=>TimeDomainBusy?storedBasicHits:0;
  public int LastTimeRelease{get;private set;}
  bool BeginTimeDomain(){CancelTimeDomain();timeDomainBusy=true;timeDomainStart=Time+Math.Max(0,Dragon.skill.timeDomainDelay);TimeDomainDuration=Enemy.timeStopImmune?0:Math.Max(0,Dragon.skill.timeDomainDuration*Math.Max(0,Enemy.timeStopDurationMultiplier));timeDomainEnd=timeDomainStart+TimeDomainDuration;Cue?.Invoke(CombatCue.SkillCast,0);TickTimeDomain();return true;}
  public void CancelTimeDomain(){timeDomainBusy=timeDomainStarted=false;storedBasicDamage=0;storedBasicHits=0;timeDomainStart=timeDomainEnd=0;}
  double TimeDomainStep(double step){if(!TimeDomainBusy)return step;double next=timeDomainStarted?timeDomainEnd:timeDomainStart;return next>Time+1e-8?Math.Min(step,next-Time):step;}
  void PauseEnemyTime(double delta){if(!TimeDomainBusy)return;double paused=Math.Max(0,Math.Min(Time+delta,timeDomainEnd)-Math.Max(Time,timeDomainStart));if(paused<=0)return;NextEnemyStrike+=paused;nextSpecialAt+=paused;if(enemyShieldHP>0)enemyShieldUntil+=paused;if(reflectUntil>Time)reflectUntil+=paused;}
  void TickTimeDomain(){if(!TimeDomainBusy)return;if(!timeDomainStarted&&Time>=timeDomainStart-1e-8){timeDomainStarted=true;Cue?.Invoke(CombatCue.TimeStopped,0);}if(Time<timeDomainEnd-1e-8)return;
   bool critical;int skillDamage=Hit(Dragon.skill.damage,EffectiveSkillElement,true,out critical,true);long total=storedBasicDamage+skillDamage;CancelTimeDomain();LastTimeRelease=DamageEnemy((int)Math.Min(int.MaxValue,total));if(EnemyHP==0&&Result==BattleResult.Fighting)Result=BattleResult.Victory;Cue?.Invoke(CombatCue.TimeReleased,LastTimeRelease);Feedback?.Invoke("시간의 태엽 · 해방 −"+LastTimeRelease);
  }
 }
}
