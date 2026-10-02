using UnityEngine;
namespace DragonTower
{
    public enum IceSkillKind { Shot, Ball, Breath, Wall }
    [CreateAssetMenu(menuName="Dragon Tower/VFX/Ice Skill Library")]
    public sealed class IceSkillLibrary : ScriptableObject
    {
        public IceVfxStyle style;
        public Sprite[] sprites;
        public Sprite Get(int index)=>sprites!=null&&index<sprites.Length?sprites[index]:null;
    }
}
