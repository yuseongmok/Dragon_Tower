using UnityEngine;
namespace DragonTower {
 public enum WaterSkillKind { Aqua,Surge,Snipe,Splash }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Water Skill Library")]
 public sealed class WaterSkillLibrary:ScriptableObject {
  [System.Serializable]public struct Entry {public SkillData skill;public WaterSkillKind kind;}
  public WaterVfxStyle style;public Entry[] entries;
  public bool Find(SkillData skill,out WaterSkillKind kind){if(skill!=null&&entries!=null)foreach(var e in entries)if(e.skill==skill){kind=e.kind;return true;}kind=default;return false;}
 }
}
