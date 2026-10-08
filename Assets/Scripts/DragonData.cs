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
        [Tooltip("Optional base-form idle. Evolution art remains independent.")]
        public DragonIdleFrames idleFrames;
        [Tooltip("Optional base-form attack frames. Presentation only; combat timing is unchanged.")]
        public DragonIdleFrames attackFrames;
        [Tooltip("Optional base-form skill casting frames; does not change skill mechanics.")]
        public DragonIdleFrames skillFrames;
        public DragonIdleFrames dodgeFrames;
        public DragonIdleFrames hitFrames;
        public DragonIdleFrames deathFrames;
        [Header("Production template (legacy fields remain supported)")]
        [Tooltip("Resources-relative Animation Set path. Only the active form is resolved by BattleView.")]
        public string animationSetPath;
        public DragonBasicPresentation basicPresentation;
        public ElementSkillPool elementSkillPool;
        public ElementSkillPool[] additionalSkillPools=Array.Empty<ElementSkillPool>();
        public SkillData signatureSkill;
        public DragonEvolutionForm[] evolutionForms=Array.Empty<DragonEvolutionForm>();
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
        public DragonEvolutionForm Form(int stage){if(evolutionForms!=null)foreach(var form in evolutionForms)if(form!=null&&form.stage==stage)return form;return null;}
        public string AnimationPath(int stage)=>stage==0?animationSetPath:Form(stage)?.animationSetPath;
        public DragonAnimationSet LoadAnimationSet(int stage)
        {
            var path=AnimationPath(stage);return string.IsNullOrWhiteSpace(path)?null:Resources.Load<DragonAnimationSet>(path);
        }
        public DragonIdleFrames LegacyAnimation(DragonAnimationState state,int stage)
        {
            if(stage!=0)return null;
            switch(state){case DragonAnimationState.Attack:return attackFrames;case DragonAnimationState.Skill:return skillFrames;case DragonAnimationState.Dodge:return dodgeFrames;case DragonAnimationState.Hit:return hitFrames;case DragonAnimationState.Death:return deathFrames;default:return idleFrames;}
        }
        public SkillData SkillAtLevel(int level)=>Form(EvolutionStage(level))?.defaultSkill??skill;
        public float HealthMultiplier(int stage)=>Form(stage)!=null&&Form(stage).healthMultiplier>0?Form(stage).healthMultiplier:TowerRun.HealthEvolutionMultiplier(stage);
        public float AttackMultiplier(int stage)=>Form(stage)!=null&&Form(stage).attackMultiplier>0?Form(stage).attackMultiplier:TowerRun.AttackEvolutionMultiplier(stage);
        public bool CanOfferSkill(SkillData candidate)
        {
            if(candidate==null||!candidate.CanEquip(StableId)||!CanLearnSkill(candidate.elementType))return false;
            if(candidate.IsSignatureSkill)return candidate==signatureSkill;
            // Existing unmigrated dragons retain their original element-based candidate list.
            if(elementSkillPool==null)return true;
            if(elementSkillPool.Contains(candidate))return true;
            if(additionalSkillPools!=null)foreach(var pool in additionalSkillPools)if(pool!=null&&pool.Contains(candidate))return true;
            return false;
        }
        public string NameAtLevel(int level) => NameForStage(EvolutionStage(level));
        public string NameForStage(int stage)
        {
            var form=Form(stage);if(form!=null&&!string.IsNullOrWhiteSpace(form.displayName))return form.displayName;
            if(stage>=2&&!string.IsNullOrWhiteSpace(finalName))return finalName;
            if(stage>=1&&!string.IsNullOrWhiteSpace(intermediateName))return intermediateName;
            return displayName;
        }
        public Sprite BattleSpriteAtLevel(int level) => SpriteForStage(EvolutionStage(level));
        public Sprite SpriteForStage(int stage)
        {
            var form=Form(stage);if(form!=null&&form.battleSprite!=null)return form.battleSprite;
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
            speciesId=StableId,
            displayName=NameAtLevel(level), element=element, elementType=elementType, maxHP=maxHP, attackDamage=attackDamage, skillEffect=SkillAtLevel(level)==null?skillEffect:SkillAtLevel(level).effectKind,
            attackCooldown=.3f,dodgeCooldown=1.4f,dodgeDuration=.42f,
            skill=SkillAtLevel(level).Snapshot(),passiveName=passiveName,passiveDescription=passiveDescription,
            passiveMechanic=passiveMechanic,alternateSkillElement=alternateSkillElement,passiveStage=EvolutionStage(level)
        };
        public bool CanLearnSkill(ElementType candidate) => candidate==elementType||candidate==alternateSkillElement&&alternateSkillElement!=ElementType.Neutral;
    }
}
