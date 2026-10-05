using UnityEngine;
namespace DragonTower {
 public enum LightSkillKind { Flash, Holy, Sanctuary, BigBang }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Light Skill Library")]
 public sealed class LightSkillLibrary:ScriptableObject {
  [System.Serializable]public struct Entry {public SkillData skill;public LightSkillKind kind;}
  public LightVfxStyle style;public Entry[] entries;
  public bool Find(SkillData skill,out LightSkillKind kind){if(skill!=null&&entries!=null)foreach(var e in entries)if(e.skill==skill){kind=e.kind;return true;}kind=default;return false;}
 }
}
