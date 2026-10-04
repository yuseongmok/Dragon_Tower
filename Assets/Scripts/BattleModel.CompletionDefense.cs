using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  bool completionDefenseCast,completionDefenseEarned;
  double defenseFrom,defenseUntil;
  public double CompletionDefenseRemaining=>Result==BattleResult.Fighting&&completionDefenseEarned&&Time>=defenseFrom?Math.Max(0,defenseUntil-Time):0;
  public bool CompletionDefenseActive=>CompletionDefenseRemaining>0;
  public double SignatureCastElapsed=>Math.Max(0,Time-skillCastTime);
  public double SignatureReturnOffset=>Math.Max(0,protectedSkillUntil-skillCastTime);
  void BeginCompletionDefense(){
   completionDefenseEarned=false;completionDefenseCast=Dragon.skill.completionDefenseDuration>0&&Dragon.skill.completionDefensePercent>0;
   defenseFrom=defenseUntil=0;if(!completionDefenseCast)return;
   // Existing double-cast augments extend the protected presentation, not its buff.
   double extra=Math.Max(0,Dragon.skill.HitOffset(ScheduledSkillHits-1)-Dragon.skill.HitOffset(Math.Max(1,Dragon.skill.hitCount)-1));
   protectedSkillUntil+=extra;defenseFrom=protectedSkillUntil;defenseUntil=defenseFrom+Dragon.skill.completionDefenseDuration;
  }
  void NotifyCompletionDefenseHit(){if(completionDefenseCast&&resolvedCastHits>=ScheduledSkillHits){completionDefenseEarned=true;completionDefenseCast=false;}}
  int ApplyCompletionDefense(int damage)=>CompletionDefenseActive?Math.Max(0,(int)Math.Round(damage*(1-Math.Max(0,Math.Min(100,Dragon.skill.completionDefensePercent))/100.0),MidpointRounding.AwayFromZero)):damage;
  public void CancelCompletionDefense(){completionDefenseCast=completionDefenseEarned=false;defenseFrom=defenseUntil=0;}
 }
}
