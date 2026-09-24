using System;
using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName = "Dragon Tower/Dragon")]
    public class DragonData : ScriptableObject
    {
        [Tooltip("Stable save identifier. Do not change after release.")]
        public string speciesId;
        public string StableId => string.IsNullOrEmpty(speciesId) ? name : speciesId;
        public string displayName = "Ember";
        [TextArea] public string description;
        public ContentRarity rarity;
        public string element = "FIRE";
        [Tooltip("Combat element used for strengths and weaknesses. Independent of the display label.")]
        public ElementType elementType;
        [Min(1)] public int maxHP = 100;
        [Min(1)] public int attackDamage = 9;
        public Color color = new Color(1f, .48f, .27f);
        [Tooltip("Battle artwork is independent of the displayed element name.")]
        public Sprite battleSprite;
        [Header("Tower evolution")]
        [Tooltip("Horizontal transparent sheet: intermediate form on the left, final form on the right.")]
        public Texture2D evolutionSheet;
        public string intermediateName;
        public string finalName;
        [Tooltip("Controls the skill presentation independently of editable names.")]
        public SkillEffectKind skillEffect;
        public SkillData skill;
        [NonSerialized] Sprite intermediateSprite,finalSprite;
        public static int EvolutionStage(int level) => level>=40?2:level>=20?1:0;
        public string NameAtLevel(int level) => NameForStage(EvolutionStage(level));
        public string NameForStage(int stage)
        {
            if(stage>=2&&!string.IsNullOrWhiteSpace(finalName))return finalName;
            if(stage>=1&&!string.IsNullOrWhiteSpace(intermediateName))return intermediateName;
            return displayName;
        }
        public Sprite BattleSpriteAtLevel(int level) => SpriteForStage(EvolutionStage(level));
        public Sprite SpriteForStage(int stage)
        {
            if(stage<=0||evolutionSheet==null)return battleSprite;
            if(stage==1&&intermediateSprite!=null)return intermediateSprite;
            if(stage>=2&&finalSprite!=null)return finalSprite;
            int half=evolutionSheet.width/2;
            var rect=stage==1?new Rect(0,0,half,evolutionSheet.height):new Rect(half,0,evolutionSheet.width-half,evolutionSheet.height);
            var sprite=Sprite.Create(evolutionSheet,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name=StableId+(stage==1?"_intermediate":"_final");
            if(stage==1)intermediateSprite=sprite;else finalSprite=sprite;return sprite;
        }
        public BattleStats Snapshot() => Snapshot(1);
        public BattleStats Snapshot(int level) => new BattleStats
        {
            displayName=NameAtLevel(level), element=element, elementType=elementType, maxHP=maxHP, attackDamage=attackDamage, skillEffect=skillEffect,
            attackCooldown=.3f,dodgeCooldown=1.4f,dodgeDuration=.42f,
            skill=skill.Snapshot()
        };
    }
}
