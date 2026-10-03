using UnityEngine;
namespace DragonTower {
 public enum FireSkillKind { Burst,Shot,Meteor,Purgatory }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Fire Skill Library")]
 public sealed class FireSkillLibrary:ScriptableObject {
  [System.Serializable] public struct Entry { public SkillData skill; public FireSkillKind kind; }
  public Sprite[] infernoColumn; public Sprite meteor; public FireVfxStyle style; public Entry[] entries;
  public bool Find(SkillData skill,out FireSkillKind kind){if(entries!=null)foreach(var e in entries)if(e.skill==skill&&skill!=null){kind=e.kind;return true;}kind=default;return false;}
 }
}


