using System;
using UnityEngine;
namespace DragonTower {
 public enum BossAttackKind { Normal, Skill, Gimmick }
 public enum BossSkillState { Idle, Selection, Warning, Preparation, Travel, Recovery, Cooldown, Barrier, Interval }
 public enum BossBladeDirection { Front, Left, Right }
 public enum BossGimmickOutcome { None, Broken, Failed }
 [Serializable] public sealed class BossSkillDefinition {
  public string id,displayName,ownerBossId,warningVfx,attackVfx,interruptKey;
  public BossAttackKind kind=BossAttackKind.Skill;
  public float firstDelay=6,cooldown=12,warningTime=.35f,preparationTime=.85f,damageTime=1,recovery=1;
  public int floor=10,minimumNormalAttacks=2,damage;
  public float maximumInitialAttackWait=4;
  // Zero defaults preserve existing encounter snapshots and the gatekeeper timeline.
  public int minimumRulesVersion;
  public bool requiresUnstunnedStart,damageOnLaunch;
  public float lingerTime;
  // Optional common scheduler extensions. Zero preserves all pre-existing snapshots.
  public int directionalShots;
  public float interShotDelay,stormDuration,dodgeCooldownPenalty;
  public int barrierDurability;
  public float barrierWindow,breakRecovery;
  public string warningHint;
  public float damageMultiplier=1,minHPPercent=0,maxHPPercent=100;
  public ElementType element=ElementType.Earth;
  public bool dodgeable=true,guardable=true,interruptible=true;
  public CombatStatusEffect status;public float statusDuration;
  public bool Valid=>!string.IsNullOrEmpty(id)&&(kind==BossAttackKind.Skill||kind==BossAttackKind.Gimmick&&barrierDurability>0&&barrierWindow>0&&breakRecovery>=0)&&cooldown>0&&warningTime>=0&&preparationTime>=0&&damageTime>0&&recovery>=0&&lingerTime>=0;
 }
 // These values are copied into the encounter snapshot, never reloaded during replay.
 [Serializable] public sealed class BossSkillCatalog {
  public BossSkillDefinition[] skills;
  public static BossSkillDefinition[] ForBoss(string id,int floor){
   var asset=Resources.Load<TextAsset>("BossSkills");
   if(asset==null){Debug.LogError("BossSkills data missing; retaining legacy boss pattern");return null;}
   var catalog=JsonUtility.FromJson<BossSkillCatalog>(asset.text);
   var found=Array.FindAll(catalog.skills??Array.Empty<BossSkillDefinition>(),s=>s!=null&&s.Valid&&s.ownerBossId==id&&s.floor==floor);
   return found.Length==0?null:found;
  }
 }
}
