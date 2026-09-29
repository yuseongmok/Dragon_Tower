using System;
using UnityEngine;
namespace DragonTower
{
    public enum DragonPassiveMechanic
    {
        None,FlameBreath,Phoenix,FreezingGaze,Mirage,JetStream,RuyiOrb,IronArmor,
        HardenedShell,ElectricShock,StormAffinity,Tentacle,FrozenBlade,HolyLight,
        TimeRift,VoidAccelerator,HydraVenom
    }
    [CreateAssetMenu(menuName = "Dragon Tower/Dragon")]
    public class DragonData : ScriptableObject
    {
        [Tooltip("Stable save identifier. Do not change after release.")]
        public string speciesId;
        public string StableId => string.IsNullOrEmpty(speciesId) ? name : speciesId;
        public string displayName = "Ember";
        [Header("Codex")]
        [InspectorName("도감 소개 코멘트")]
        [TextArea(3,6)]
        [Tooltip("드래곤 도감에 표시할 짧은 소개입니다. 스킬 설명과 별도로 자유롭게 작성할 수 있습니다.")]
        public string description;
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
        [Tooltip("Separated artwork used from level 10.")]
        public Sprite intermediateEvolutionSprite;
        [Tooltip("Separated artwork used from level 20.")]
        public Sprite finalEvolutionSprite;
        [Tooltip("Horizontal transparent sheet: intermediate form on the left, final form on the right.")]
        public Texture2D evolutionSheet;
        public string intermediateName;
        public string finalName;
        [Tooltip("Controls the skill presentation independently of editable names.")]
        public SkillEffectKind skillEffect;
        public SkillData skill;
        [Header("Unique passive")]
        public string passiveName;
        [TextArea] public string passiveDescription;
        public DragonPassiveMechanic passiveMechanic;
        [Tooltip("Secondary skill element for affinity passives.")]
        public ElementType alternateSkillElement;
        [Tooltip("New three-form sheets include the baby form in the left third.")]
        public bool evolutionSheetIncludesBase;
        [NonSerialized] Sprite intermediateSprite,finalSprite;
        public static int EvolutionStage(int level) => level>=20?2:level>=10?1:0;
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
            if(stage<=0)return battleSprite;
            if(stage==1&&intermediateEvolutionSprite!=null)return intermediateEvolutionSprite;
            if(stage>=2&&finalEvolutionSprite!=null)return finalEvolutionSprite;
            if(evolutionSheetIncludesBase&&evolutionSheet!=null)
            {
                if(stage==1&&intermediateSprite!=null)return intermediateSprite;if(stage>=2&&finalSprite!=null)return finalSprite;
                int third=evolutionSheet.width/3;int x=stage==1?third:third*2;
                int width=stage>=2?evolutionSheet.width-third*2:third;
                var created=Sprite.Create(evolutionSheet,new Rect(x,0,width,evolutionSheet.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                created.name=StableId+"_stage_"+stage;if(stage==1)intermediateSprite=created;else finalSprite=created;return created;
            }
            if(evolutionSheet==null)return battleSprite;
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
            skill=skill.Snapshot(),passiveName=passiveName,passiveDescription=passiveDescription,
            passiveMechanic=passiveMechanic,alternateSkillElement=alternateSkillElement
        };
        public bool CanLearnSkill(ElementType candidate) => candidate==elementType||candidate==alternateSkillElement&&alternateSkillElement!=ElementType.Neutral;
    }
}
