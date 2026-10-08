using System;using UnityEngine;
namespace DragonTower {
 public enum PersistentAttackMotion { Sweep,DiagonalSlam,VerticalCrush,Wrap,Final }
 [Serializable] public struct PersistentAttackBeat {public float time;[Min(0)]public float damageMultiplier;public int limb;public PersistentAttackMotion motion;}
 [CreateAssetMenu(menuName="Dragon Tower/Skill/Persistent Attack")]
 public sealed class PersistentAttackData:ScriptableObject {
  [Min(.1f)]public float duration=10;public PersistentAttackBeat[] attacks;
  public PersistentAttackStats Snapshot()=>new PersistentAttackStats{duration=duration,attacks=attacks==null?Array.Empty<PersistentAttackBeat>():(PersistentAttackBeat[])attacks.Clone()};
 }
 public sealed class PersistentAttackStats {
  public float duration;public PersistentAttackBeat[] attacks;
  public bool Valid {get{if(float.IsNaN(duration)||float.IsInfinity(duration)||duration<=0||attacks==null||attacks.Length==0)return false;float previous=-1;foreach(var b in attacks){if(float.IsNaN(b.time)||float.IsInfinity(b.time)||b.time<=previous||b.time<0||b.time>duration||float.IsNaN(b.damageMultiplier)||float.IsInfinity(b.damageMultiplier)||b.damageMultiplier<0)return false;previous=b.time;}return true;}}
 }
}
