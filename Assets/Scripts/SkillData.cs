using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName = "Dragon Tower/Skill")]
    public class SkillData : ScriptableObject
    {
        [Tooltip("Stable identifier used by content tools and CSV.")]
        public string skillId;
        public string displayName = "Flame burst";
        [TextArea] public string description;
        public ElementType elementType;
        public SkillEffectKind effectKind;
        public Sprite icon;
        [Header("Battle VFX")]
        [Tooltip("Use a prefab from Eric VFX Studio/Prefabs/Built-In for this project.")]
        public GameObject battleVfx;
        [Min(0.01f)] public float vfxScale = 0.65f;
        [Tooltip("Offset from the enemy in the 480 x 850 battle frame.")]
        public Vector2 vfxOffset;
        public Vector3 vfxEuler;
        [Min(0.1f)] public float vfxDuration = 1.6f;
        [Tooltip("Maximum live particles per child system. Composite VFX keep every child system enabled.")]
        [Range(3, 24)] public int vfxSystemLimit = 14;
        [Min(1)] public int damage = 32;
        [Min(0.1f)] public float cooldown = 4f;
        [Header("Multi hit")]
        [Min(1)] public int hitCount=1;
        [Min(0.03f)] public float hitInterval=.14f;
        [Header("Status effect")]
        public CombatStatusEffect statusEffect;
        [Range(0,100)] public float statusChancePercent;
        [Min(0)] public float statusDuration;
        [Tooltip("Burn: damage per second. Slow: attack speed reduction percent.")][Min(0)] public float statusPower;
        public string StableId=>string.IsNullOrWhiteSpace(skillId)?name:skillId.Trim();
        public SkillStats Snapshot()=>new SkillStats{displayName=displayName,elementType=elementType,damage=damage,cooldown=cooldown,hitCount=Mathf.Max(1,hitCount),hitInterval=Mathf.Max(.03f,hitInterval),statusEffect=statusEffect,statusChancePercent=statusChancePercent,statusDuration=statusDuration,statusPower=statusPower};
    }
}
