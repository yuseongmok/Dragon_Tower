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
        [Header("Optional celestial signature")]
        public bool celestialSignature;[Min(0)] public int celestialExtraDamage=42;
        public bool timeDomain; public float timeDomainDelay=.24f,timeDomainDuration=5;
        [Tooltip("Optional secondary element: best existing matchup, never multiply both.")]
        public bool useSecondaryElement;
        public ElementType secondaryElement;
        public SkillEffectKind effectKind;
        public Sprite icon;
        public ContentRarity rarity=ContentRarity.Common;
        [Header("Signature skill (optional)")]
        public string exclusiveDragonId;
        [Header("Optional signature presentation / dodge release")]
        public bool wispPresentation;
        public bool lunarPresentation;
        public bool solarPresentation;
        public bool volcanicPresentation;
        public bool genesisPresentation;
        [Header("Optional damage-interrupted charged channel")]
        [Header("Optional basic-attack empowerment")]
        [Min(0)] public float attackEmpowerDuration,attackEmpowerDelay;
        [Min(0)] public int attackEmpowerDamage;
        public bool chargedBeam;
        [Min(.1f)] public float chargeDuration=5;
        [Min(0)] public float beamIgnitionDelay=.18f;
        [Min(.1f)] public float beamDuration=5;
        [Header("Optional burrow protection / completion defense")]
        [Min(0)] public float protectedCastDelay;
        [Min(0)] public float completionDefenseDuration;
        [Range(0,100)] public float completionDefensePercent;
        [Header("Optional final-impact status and target modifier")]
        public bool statusOnFinalHit;
        [Range(0,100)] public float targetCriticalBonus;
        [Min(0)] public float targetCriticalDuration;
        [Tooltip("Optional sorted seconds from cast; empty preserves uniform timing.")] public float[] hitTimeOffsets;
        [Range(0,100)] public float completionHealPercent;
        [Min(0)] public float completionHealDelay=.22f;
        [Min(0)] public float slowBonusDamagePercent;
        [Min(.03f)] public float slowBonusDelay=.16f;
        [Min(0)] public float dodgeFreeCastDuration;
        [Min(0)] public float dodgeFreeAfterDuration;
        [Min(1)] public float finalHitDamageMultiplier=1;
        [Range(0,100)] public float rewardEligibilityPercent=100;
        [Tooltip("Successful casts only: protects the full cinematic; zero preserves ordinary skills.")]
        [Min(0)] public float protectedCastDuration;
        public bool CanEquip(string dragonId)=>string.IsNullOrEmpty(exclusiveDragonId)||exclusiveDragonId==dragonId;
        [Min(0)] public float initialHitDelay;
        [Header("Ice presentation (optional)")]
        public bool icePresentation;
        public IceSkillKind iceKind;
        public bool statusOnHit;
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
        public SkillStats Snapshot()=>new SkillStats{celestialSignature=celestialSignature,celestialExtraDamage=celestialExtraDamage,timeDomain=timeDomain,timeDomainDelay=timeDomainDelay,timeDomainDuration=timeDomainDuration,useSecondaryElement=useSecondaryElement,secondaryElement=secondaryElement,attackEmpowerDuration=attackEmpowerDuration,attackEmpowerDelay=attackEmpowerDelay,attackEmpowerDamage=attackEmpowerDamage,chargedBeam=chargedBeam,chargeDuration=chargeDuration,beamIgnitionDelay=beamIgnitionDelay,beamDuration=beamDuration,protectedCastDelay=protectedCastDelay,completionDefenseDuration=completionDefenseDuration,completionDefensePercent=completionDefensePercent,statusOnFinalHit=statusOnFinalHit,targetCriticalBonus=targetCriticalBonus,targetCriticalDuration=targetCriticalDuration,hitTimeOffsets=hitTimeOffsets==null?null:(float[])hitTimeOffsets.Clone(),completionHealPercent=completionHealPercent,completionHealDelay=completionHealDelay,exclusiveDragonId=exclusiveDragonId,slowBonusDamagePercent=slowBonusDamagePercent,slowBonusDelay=slowBonusDelay,dodgeFreeCastDuration=dodgeFreeCastDuration,dodgeFreeAfterDuration=dodgeFreeAfterDuration,finalHitDamageMultiplier=Mathf.Max(1,finalHitDamageMultiplier),protectedCastDuration=Mathf.Max(0,protectedCastDuration),displayName=displayName,elementType=elementType,damage=damage,cooldown=cooldown,initialHitDelay=Mathf.Max(0,initialHitDelay),hitCount=Mathf.Max(1,hitCount),hitInterval=Mathf.Max(.03f,hitInterval),statusOnHit=statusOnHit,statusEffect=statusEffect,statusChancePercent=statusChancePercent,statusDuration=statusDuration,statusPower=statusPower};
    }
}


