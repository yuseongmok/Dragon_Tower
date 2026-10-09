using System;
namespace DragonTower {
 public sealed partial class BattleModel {
  PersistentAttackStats persistentPlan;double persistentStart;int persistentNext,persistentDamage;ElementType persistentElement;bool persistentActive;
  public bool PersistentAttackActive=>persistentActive&&Result==BattleResult.Fighting;
  public double PersistentAttackElapsed=>Math.Max(0,Time-persistentStart);
  public PersistentAttackStats PersistentPlan=>persistentPlan;
  public string PersistentSourceId{get;private set;}
  public int PersistentSerial{get;private set;}public int PersistentHitIndex{get;private set;}public int PersistentHits{get;private set;}
  public void CancelPersistentAttack(){persistentActive=false;}
  bool BeginPersistentAttack(){CancelPersistentAttack();persistentPlan=Dragon.skill.persistentAttack;persistentStart=Time;persistentNext=0;persistentDamage=Dragon.skill.damage;persistentElement=EffectiveSkillElement;PersistentSourceId=Dragon.skill.skillId;PersistentHitIndex=-1;PersistentHits=0;PersistentSerial++;persistentActive=true;Cue?.Invoke(CombatCue.PersistentStarted,0);return true;}
  double PersistentStep(double step){if(!PersistentAttackActive)return step;double due=persistentStart+(persistentNext<persistentPlan.attacks.Length?persistentPlan.attacks[persistentNext].time:persistentPlan.duration);return due>Time+1e-8?Math.Min(step,due-Time):step;}
  void TickPersistentAttack(){if(!PersistentAttackActive){CancelPersistentAttack();return;}while(persistentNext<persistentPlan.attacks.Length&&persistentStart+persistentPlan.attacks[persistentNext].time<=Time+1e-8&&Result==BattleResult.Fighting){var beat=persistentPlan.attacks[persistentNext];PersistentHitIndex=persistentNext++;var savedCast=activeAugmentCast;activeAugmentCast=persistentAugmentCast;int itemEnemyBefore=EnemyHP+enemyShieldHP;bool critical;int damage=Hit(Math.Max(0,(int)Math.Round(persistentDamage*beat.damageMultiplier)),persistentElement,true,out critical);RecordOriginalItemHit(ItemTrigger.SkillHit,CombatEventSource.Legendary,itemEnemyBefore,itemCastSerial);NotifyAugmentSkillHit(damage,critical);activeAugmentCast=savedCast;PersistentHits++;Cue?.Invoke(CombatCue.PersistentHit,damage);if(Result!=BattleResult.Fighting){CancelPersistentAttack();return;}}if(Time>=persistentStart+persistentPlan.duration-1e-8){persistentActive=false;Cue?.Invoke(CombatCue.PersistentEnded,0);}}
 }
}
