using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  double skillCastTime,targetCriticalUntil;int resolvedCastHits;float targetCriticalBonus;
  public double TargetCriticalRemaining=>Result==BattleResult.Fighting?Math.Max(0,targetCriticalUntil-Time):0;
  public float TargetCriticalBonus=>TargetCriticalRemaining>0?targetCriticalBonus:0;
  public float EffectiveCriticalChancePercent {get{float chance=Dragon.criticalChance*100f+(Time<criticalBuffUntil?criticalBuffPercent:0)+TargetCriticalBonus;if(EnemyBurning&&Has(AugmentMechanic.Inferno))chance+=Value(AugmentMechanic.Inferno,50);return Math.Max(0,Math.Min(100,chance));}}
  void ApplyFinalTargetModifier(int damage){if(resolvedCastHits!=ScheduledSkillHits||damage<=0||Result!=BattleResult.Fighting)return;if(Dragon.skill.statusOnFinalHit)ApplySkillStatus();if(Dragon.skill.targetCriticalBonus>0&&Dragon.skill.targetCriticalDuration>0){targetCriticalBonus=Dragon.skill.targetCriticalBonus;targetCriticalUntil=Time+Dragon.skill.targetCriticalDuration;Feedback?.Invoke("화산의 약점 · 현재 적 치명타 확률 +"+targetCriticalBonus+"%p");}}
  // A BattleModel owns exactly one target. No modifier can leak into another encounter.
  public void CancelTargetModifier(){if(Dragon.skill.targetCriticalBonus>0){pendingSkillHits=0;pendingFirstSkillHit=false;}targetCriticalUntil=0;targetCriticalBonus=0;}
 }
}
