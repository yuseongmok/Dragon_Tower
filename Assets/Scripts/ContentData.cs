using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonTower
{
    public enum ContentRarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum ContentEffectType
    {
        Heal, MaxHP, AttackDamage, SkillDamagePercent, SkillCooldownPercent,
        AttackCooldownPercent, DodgeCooldownPercent, DodgeDurationPercent, Gold,
        MaxHPPercent, AttackDamagePercent, CriticalChancePercent, CriticalDamagePercent,
        SkillDisabled, SkillCooldownSetZero
    }
    [Serializable]
    public sealed class ContentEffect
    {
        public ContentEffectType type;
        public float value;
        [TextArea] public string note;
    }
    public abstract class IdentifiedContent : ScriptableObject
    {
        [Tooltip("Stable identifier used by saves and CSV. Do not change after release.")]
        public string contentId;
        public string displayName;
        [TextArea] public string description;
        public ContentRarity rarity;
        public Sprite icon;
        public string StableId=>string.IsNullOrWhiteSpace(contentId)?name:contentId.Trim();
    }
}
