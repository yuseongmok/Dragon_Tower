using UnityEngine;
namespace DragonTower
{
    public enum EarthSkillKind { Slam, Claw, Quake, Screw }
    [CreateAssetMenu(menuName="Dragon Tower/VFX/Earth Skill Library")]
    public sealed class EarthSkillLibrary : ScriptableObject
    {
        [System.Serializable] public struct Entry { public SkillData skill; public EarthSkillKind kind; }
        public EarthVfxStyle style;
        public Sprite[] shapes;
        public Entry[] entries;
        public bool Find(SkillData skill,out EarthSkillKind kind)
        {
            if(entries!=null) foreach(var entry in entries)
                if(skill!=null&&entry.skill==skill){kind=entry.kind;return true;}
            kind=default;return false;
        }
        public Sprite Shape(int row,float phase)=>shapes[row*8+Mathf.Clamp((int)(phase*8),0,7)];
    }
}
