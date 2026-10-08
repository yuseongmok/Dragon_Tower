using System;using System.Collections.Generic;
namespace DragonTower {
 public sealed partial class BattleModel {
  struct CelestialPending {public double due;public int id,pattern;}
  readonly List<CelestialPending> celestialQueue=new List<CelestialPending>(16);
  SkillStats celestialSource;bool celestialArmed;int celestialSerial,celestialPattern;
  public const float CelestialProcChance=10,CelestialCharge=.1f;
  public bool CelestialCooldownActive=>Result==BattleResult.Fighting&&celestialArmed&&ReferenceEquals(celestialSource,Dragon.skill)&&Dragon.skill.celestialSignature&&!string.IsNullOrEmpty(Dragon.skill.exclusiveDragonId)&&Dragon.speciesId==Dragon.skill.exclusiveDragonId&&Time<SkillReady;
  public int CelestialRolls{get;private set;}public int CelestialProcs{get;private set;}public int CelestialExtraHits{get;private set;}
  public int CelestialEventId{get;private set;}public int CelestialEventPattern{get;private set;}
  public bool IsCelestialPending(int id){if(!CelestialCooldownActive)return false;foreach(var q in celestialQueue)if(q.id==id)return true;return false;}
  void ArmCelestial(){celestialSource=Dragon.skill;celestialArmed=true;}
  public void CancelCelestial(){celestialArmed=false;celestialSource=null;celestialQueue.Clear();}
  // Exactly one roll from the original successful Attack entry point. Extras never call this.
  void ApplyCelestialBasicHit(int basicDamage){if(Result!=BattleResult.Fighting){CancelCelestial();return;}if(basicDamage<=0||EnemyHP<=0||!CelestialCooldownActive)return;CelestialRolls++;if(!Roll(CelestialProcChance))return;var q=new CelestialPending{due=Time+CelestialCharge,id=++celestialSerial,pattern=celestialPattern++%5};celestialQueue.Add(q);CelestialProcs++;CelestialEventId=q.id;CelestialEventPattern=q.pattern;Cue?.Invoke(CombatCue.CelestialPrimed,0);}
  double CelestialStep(double step){if(!CelestialCooldownActive||celestialQueue.Count==0)return step;double next=Math.Min(celestialQueue[0].due,SkillReady);return next>Time+1e-8?Math.Min(step,next-Time):step;}
  void TickCelestial(){if(!CelestialCooldownActive){celestialQueue.Clear();return;}while(celestialQueue.Count>0&&celestialQueue[0].due<=Time+1e-8&&Result==BattleResult.Fighting){var q=celestialQueue[0];celestialQueue.RemoveAt(0);bool critical;int damage=ElementalExtra(Percent(Math.Max(0,Dragon.skill.celestialExtraDamage),-Param(AugmentMechanic.Transference,1)),100,EffectiveSkillElement);CelestialEventId=q.id;CelestialEventPattern=q.pattern;CelestialExtraHits++;Cue?.Invoke(CombatCue.CelestialExtraHit,damage);if(Result!=BattleResult.Fighting){CancelCelestial();break;}}}
 }
}
