using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  int empowerPhase,empowerHits;double empowerBegins,empowerEnds;
  public bool AttackEmpowered=>Result==BattleResult.Fighting&&empowerPhase==2&&Time>=empowerBegins&&Time<empowerEnds;
  public bool AttackEmpowerPriming=>Result==BattleResult.Fighting&&empowerPhase==1;
  public double AttackEmpowerRemaining=>AttackEmpowered?Math.Max(0,empowerEnds-Time):0;
  public double AttackEmpowerStartedAt=>empowerBegins;
  public int EmpoweredHitCount=>empowerHits;
  public int EmpoweredHitPattern{get;private set;}
  bool BeginAttackEmpower(){CancelAttackEmpower();empowerPhase=1;empowerHits=0;EmpoweredHitPattern=0;empowerBegins=Time+Math.Max(0,Dragon.skill.attackEmpowerDelay);empowerEnds=empowerBegins+Math.Max(0,Dragon.skill.attackEmpowerDuration);pendingSkillHits=0;pendingFirstSkillHit=false;ScheduledSkillHits=0;Cue?.Invoke(CombatCue.SkillCast,0);TickAttackEmpower();return true;}
  double AttackEmpowerStep(double step){double due=empowerPhase==1?empowerBegins:empowerPhase==2?empowerEnds:double.MaxValue;double left=due-Time;return left>0?Math.Min(step,left):step;}
  void TickAttackEmpower(){if(Result!=BattleResult.Fighting){CancelAttackEmpower();return;}if(empowerPhase==1&&Time>=empowerBegins){empowerPhase=2;Cue?.Invoke(CombatCue.EmpowerStarted,0);}if(empowerPhase==2&&Time>=empowerEnds){empowerPhase=0;Cue?.Invoke(CombatCue.EmpowerEnded,0);}}
  // Invoked once only after the real basic attack, never by the extra hit itself.
  void ApplyEmpoweredAttack(int basicDamage){if(basicDamage<=0||!AttackEmpowered||EnemyHP<=0||Dragon.skill.attackEmpowerDamage<=0)return;bool critical;int damage=Hit(Dragon.skill.attackEmpowerDamage,Dragon.skill.elementType,true,out critical);EmpoweredHitPattern=empowerHits%3;empowerHits++;Cue?.Invoke(CombatCue.EmpoweredAttackHit,damage);}
  public void CancelAttackEmpower(){empowerPhase=0;empowerBegins=empowerEnds=0;}
 }
}
