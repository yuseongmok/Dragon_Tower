using UnityEngine;
namespace DragonTower
{
    public enum DragonAnimationState { Idle,Attack,Skill,Dodge,Hit,Death }
    public enum DragonAnimationArchetype { SmallQuadruped,LargeQuadruped,Biped,Flying,Serpentine }
    public enum DragonBasicPresentation { Standard,PixelWind }
    [CreateAssetMenu(menuName="Dragon Tower/Production/Animation Set")]
    public sealed class DragonAnimationSet : ScriptableObject
    {
        public DragonAnimationArchetype archetype;
        public DragonIdleFrames idle,attack,skill,dodge,hit,death;
        public DragonIdleFrames Get(DragonAnimationState state)
        {
            switch(state){case DragonAnimationState.Attack:return attack;case DragonAnimationState.Skill:return skill;case DragonAnimationState.Dodge:return dodge;case DragonAnimationState.Hit:return hit;case DragonAnimationState.Death:return death;default:return idle;}
        }
    }
    [System.Serializable]
    public sealed class DragonEvolutionForm
    {
        [Range(1,2)] public int stage=1;
        public string displayName;
        public Sprite battleSprite;
        [Tooltip("Resources-relative Animation Set path, without extension. No eager animation reference.")]
        public string animationSetPath;
        [Tooltip("Optional default skill after evolution. An acquired replacement still takes priority.")]
        public SkillData defaultSkill;
        [Tooltip("Zero preserves existing shared evolution multipliers.")]
        [Min(0)] public float healthMultiplier,attackMultiplier;
    }
}
