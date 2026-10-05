using UnityEngine;
namespace DragonTower {
 public enum LightningSkillKind { Shock, Overheat, Plasma, Thunder }
 [CreateAssetMenu(menuName="Dragon Tower/VFX/Lightning Skill Library")]
 public sealed class LightningSkillLibrary:ScriptableObject {
  [System.Serializable]public struct Entry {public SkillData skill;public LightningSkillKind kind;}
  public LightningVfxStyle style;public Entry[] entries;
  public bool Find(SkillData skill,out LightningSkillKind kind){if(skill!=null&&entries!=null)foreach(var e in entries)if(e.skill==skill){kind=e.kind;return true;}kind=default;return false;}
 }
}
