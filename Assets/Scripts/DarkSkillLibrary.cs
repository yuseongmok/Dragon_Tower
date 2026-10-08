using UnityEngine;
namespace DragonTower {
 public enum DarkSkillKind { Strike, Paranoia, Orb, Apocalypse }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Dark Skill Library")]
 public sealed class DarkSkillLibrary:ScriptableObject {
  [System.Serializable]public struct Entry {public SkillData skill;public DarkSkillKind kind;}
  public DarkVfxStyle style;public Entry[] entries;
  public bool Find(SkillData skill,out DarkSkillKind kind){if(skill!=null&&entries!=null)foreach(var e in entries)if(e.skill==skill){kind=e.kind;return true;}kind=default;return false;}
 }
}
