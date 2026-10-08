using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  public const double BasePoisonTickInterval=2;
  double temporaryPoisonUntil;int temporaryPoisonDamage;
  int CurrentPoisonTickDamage=>DotDamage(Math.Max(poisonDamage,Time<temporaryPoisonUntil?temporaryPoisonDamage:0));
  void ApplyTemporaryPoison(int damage,float duration){bool active=EnemyPoisoned;if(Time>=temporaryPoisonUntil)temporaryPoisonDamage=0;temporaryPoisonDamage=Math.Max(temporaryPoisonDamage,damage);temporaryPoisonUntil=Math.Max(temporaryPoisonUntil,Time+duration);if(!active)passivePoisonNext=Time+PoisonTickInterval;}
  double poisonTempoUntil;float poisonTempoFactor=1;
  public bool EnemyPoisonAccelerated=>Result==BattleResult.Fighting&&Time<poisonTempoUntil-1e-8;
  public double PoisonAccelerationRemaining=>EnemyPoisonAccelerated?poisonTempoUntil-Time:0;
  public double PoisonTickInterval=>Math.Max(.25,BasePoisonTickInterval*DotFactor*(EnemyPoisonAccelerated?poisonTempoFactor:1));
  public int PoisonTickCount{get;private set;}
  public double LastPoisonTickTime{get;private set;}=-1;
  void AcceleratePoison(float duration,float factor){if(duration<=0||Result!=BattleResult.Fighting)return;TickPoison();if(Result!=BattleResult.Fighting)return;factor=Math.Max(.05f,Math.Min(1,factor));float old=EnemyPoisonAccelerated?poisonTempoFactor:1;if(EnemyPoisoned)passivePoisonNext=Time+Math.Max(0,passivePoisonNext-Time)*factor/old;poisonTempoFactor=factor;poisonTempoUntil=Time+duration;}
  public void CancelPoisonAcceleration(){if(poisonTempoUntil>0&&EnemyPoisoned)passivePoisonNext=Time+Math.Max(0,passivePoisonNext-Time)/poisonTempoFactor;poisonTempoUntil=0;poisonTempoFactor=1;}
  void TickPoison(){
   // Process deadlines chronologically, including expiry between two ticks. A due
   // tick at expiry fires once; the following interval returns to the base rate.
   while(Result==BattleResult.Fighting){bool expires=poisonTempoUntil>0&&poisonTempoUntil<=Time+1e-8;bool ticks=(poisonDamage>0||passivePoisonNext<=temporaryPoisonUntil+1e-8&&temporaryPoisonDamage>0)&&passivePoisonNext<=Time+1e-8;if(!expires&&!ticks)break;
    if(expires&&(!ticks||poisonTempoUntil<passivePoisonNext)){double at=poisonTempoUntil;if(EnemyPoisoned)passivePoisonNext=at+Math.Max(0,passivePoisonNext-at)/poisonTempoFactor;poisonTempoUntil=0;poisonTempoFactor=1;continue;}
    double tickAt=passivePoisonNext;int tickDamage=DotDamage(Math.Max(poisonDamage,tickAt<=temporaryPoisonUntil+1e-8?temporaryPoisonDamage:0));DamageEnemy(tickDamage);PoisonTickCount++;LastPoisonTickTime=tickAt;Cue?.Invoke(CombatCue.SkillHit,tickDamage);Feedback?.Invoke("중독 피해  −"+tickDamage);passivePoisonNext+=Math.Max(.25,BasePoisonTickInterval*DotFactor*(poisonTempoUntil>tickAt?poisonTempoFactor:1));if(EnemyHP==0)Result=BattleResult.Victory;
    if(poisonTempoUntil>0&&poisonTempoUntil<=tickAt){poisonTempoUntil=0;poisonTempoFactor=1;}
   }
  }
 }
}
