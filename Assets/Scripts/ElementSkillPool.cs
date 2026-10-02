using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName="Dragon Tower/Production/Element Skill Pool")]
    public sealed class ElementSkillPool : ScriptableObject
    {
        public ElementType element;
        [Tooltip("Shared Common through Unique skills only. Signatures belong to DragonData.")]
        public SkillData[] skills=System.Array.Empty<SkillData>();
        public bool Contains(SkillData skill)
        {
            if(skills==null||skill==null||skill.rarity==ContentRarity.Legendary||!string.IsNullOrEmpty(skill.exclusiveDragonId)||skill.elementType!=element)return false;
            foreach(var entry in skills)if(entry==skill)return true;return false;
        }
    }
}
