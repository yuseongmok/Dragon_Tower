using System;
namespace DragonTower {
 public enum ChargedBeamPhase { Idle,Charging,Priming,Beam,Recovery,Failed }
 public sealed partial class BattleModel {
  ChargedBeamPhase beamPhase;double chargeBegan,beamPhaseAt,chargeDue,beamDue,beamEnds,chargedCooldownUntil;
  public ChargedBeamPhase BeamPhase=>Result==BattleResult.Fighting?beamPhase:ChargedBeamPhase.Idle;
  public bool ChargedBeamBusy=>BeamPhase==ChargedBeamPhase.Charging||ChargedBeamCommitted;
  public bool ChargedBeamCommitted=>BeamPhase==ChargedBeamPhase.Priming||BeamPhase==ChargedBeamPhase.Beam||BeamPhase==ChargedBeamPhase.Recovery;
  public double BeamPhaseElapsed=>Math.Max(0,Time-beamPhaseAt);
  public double ChargeProgress=>BeamPhase==ChargedBeamPhase.Charging?Math.Min(1,(Time-chargeBegan)/Math.Max(.1,Dragon.skill.chargeDuration)):0;
  bool ChargeCooldownReady=>!Dragon.skill.chargedBeam||beamPhase!=ChargedBeamPhase.Failed||Time>=chargedCooldownUntil;
  bool BeginChargedBeam(){
   CancelChargedBeam();CancelCompletionDefense();skillCastTime=chargeBegan=beamPhaseAt=Time;resolvedCastHits=0;
   chargedCooldownUntil=SkillReady;chargeDue=Time+Math.Max(.1,Dragon.skill.chargeDuration);beamDue=chargeDue+Math.Max(0,Dragon.skill.beamIgnitionDelay);beamEnds=beamDue+Math.Max(.1,Dragon.skill.beamDuration);
   beamPhase=ChargedBeamPhase.Charging;pendingSkillHits=0;pendingFirstSkillHit=false;ScheduledSkillHits=Math.Max(2,Dragon.skill.hitCount);
   Cue?.Invoke(CombatCue.SkillCast,0);Feedback?.Invoke(Dragon.skill.displayName+" · 무피격 충전");return true;
  }
  // Split simulation steps at state boundaries. At the exact charge deadline,
  // incoming damage resolves before success, independent of render frame rate.
  double ChargedBeamStep(double step){double due=beamPhase==ChargedBeamPhase.Charging?chargeDue:beamPhase==ChargedBeamPhase.Priming?beamDue:beamPhase==ChargedBeamPhase.Beam?beamEnds:double.MaxValue;double left=due-Time;return left>0?Math.Min(step,left):step;}
  void NotifyChargedBeamDamage(int actualHPDamage){
   if(actualHPDamage<=0||beamPhase!=ChargedBeamPhase.Charging)return;
   beamPhase=ChargedBeamPhase.Failed;beamPhaseAt=Time;pendingSkillHits=0;pendingFirstSkillHit=false;SkillReady=Math.Max(SkillReady,chargedCooldownUntil);
   Cue?.Invoke(CombatCue.ChargeFailed,0);Feedback?.Invoke(Dragon.skill.displayName+" · 충전 실패");
  }
  void TickChargedBeam(){
   if(beamPhase==ChargedBeamPhase.Idle)return;if(Result!=BattleResult.Fighting){CancelChargedBeam();return;}
   if(beamPhase==ChargedBeamPhase.Failed){SkillReady=Math.Max(SkillReady,chargedCooldownUntil);if(Time>=chargedCooldownUntil)beamPhase=ChargedBeamPhase.Idle;return;}
   if(beamPhase==ChargedBeamPhase.Charging&&Time>=chargeDue){beamPhase=ChargedBeamPhase.Priming;beamPhaseAt=chargeDue;Cue?.Invoke(CombatCue.BeamPrimed,0);}
   if(beamPhase==ChargedBeamPhase.Priming&&Time>=beamDue){
    beamPhase=ChargedBeamPhase.Beam;beamPhaseAt=beamDue;protectedSkillFrom=beamDue;protectedSkillUntil=beamEnds;
    skillCastTime=beamDue;resolvedCastHits=0;BeginSlowSignature();BeginCompletionHeal();
    pendingFirstSkillHit=false;pendingSkillHits=ScheduledSkillHits-1;nextSkillHit=beamDue+Math.Max(.03f,Dragon.skill.hitInterval);
    Cue?.Invoke(CombatCue.BeamStarted,0);PerformSkillHit(true);
   }
   if(beamPhase==ChargedBeamPhase.Beam&&Time>=beamEnds){beamPhase=ChargedBeamPhase.Recovery;beamPhaseAt=beamEnds;protectedSkillUntil=protectedSkillFrom=0;pendingSkillHits=0;Cue?.Invoke(CombatCue.BeamEnded,0);}
   if(beamPhase==ChargedBeamPhase.Recovery&&Time>=beamEnds+.25)beamPhase=ChargedBeamPhase.Idle;
  }
  public void CancelChargedBeam(){if(beamPhase==ChargedBeamPhase.Beam||beamPhase==ChargedBeamPhase.Priming||beamPhase==ChargedBeamPhase.Charging){pendingSkillHits=0;pendingFirstSkillHit=false;protectedSkillUntil=protectedSkillFrom=0;}beamPhase=ChargedBeamPhase.Idle;}
 }
}
