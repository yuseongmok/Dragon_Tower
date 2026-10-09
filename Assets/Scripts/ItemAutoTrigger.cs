using System;
using UnityEngine;
namespace DragonTower {
 public enum ItemTrigger { BasicHit, SkillUsed, SkillHit, DodgeSucceeded, DamageTaken, EnemyKilled }
 public enum CombatEventSource { BasicAttack, ActiveSkill, Legendary, ItemProc, StatusDamage, EnemyAttack }
 public enum ItemProcEffect { Attack, IncomingReduction, Heal, Ward, SkillRecharge, HealAndWard }
 public enum ItemProcArt { None, Fireball, FrostSpear, Tornado, Meteor, Guard, Earth, Lightning, Light, Water, Dark }
 public enum ItemDamageBasis { Flat, BasicAttack, Skill }
 public enum ItemProcVfxOrigin { Enemy, Character, SkillOrigin }
 [Serializable] public sealed class ItemAutoTrigger {
  public bool enabled;
  [Range(0,100)] public float belowHealthPercent;
  [Min(0)] public float healthPercent,wardPercent,rechargeSeconds;
  [Min(.1f)] public float buffDuration=5;
  [Min(.1f)] public float visualScale=1;
  public ItemProcEffect effect;
  public ItemProcArt art;
  public ItemDamageBasis damageBasis;
  [Range(0,100)] public float reductionPercent;
  [Min(0)] public float damagePercent=100;
  public ItemTrigger trigger;
  [Min(0)] public float cooldown=10;
  [Range(0,100)] public float chancePercent=100;
  public bool eachSkillHit;
  [Min(0)] public int damage;
  [Tooltip("Independent item attack element. Uses existing ElementRules once; never changes dragon/skill element.")]
  public ElementType element;
  public ElementType DamageElement=>effect==ItemProcEffect.Attack?element:ElementType.Neutral;
  [Min(1)] public int hitCount=1;
  [Tooltip("Wait before launching item VFX and attack; cooldown is reserved on the original trigger.")]
  [Min(0)] public float activationDelay;
  [Min(0)] public float initialDelay;
  [Min(.03f)] public float hitInterval=.1f;
  public CombatStatusEffect status;
  [Range(0,100)] public float statusChancePercent=100;
  [Min(0)] public float statusDuration=3,statusPower;
  public GameObject vfxPrefab;
  public ItemProcVfxOrigin vfxOrigin;
  [Min(.05f)] public float vfxLifetime=1;
  public ItemAutoTrigger Copy()=>(ItemAutoTrigger)MemberwiseClone();
 }
 [Serializable] public sealed class ItemCooldownSave {public string itemId;public int slot;public double remaining;}
 public struct ItemProcVisual {public CombatEventSource Source=>CombatEventSource.ItemProc;public string itemId;public int slot,damage,hitIndex;public bool started;public ItemAutoTrigger effect;}
}
