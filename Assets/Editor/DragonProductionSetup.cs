using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class DragonProductionSetup
    {
        public const string AnimationPath="Assets/Resources/DragonAnimations/ZephyrBase.asset";
        public const string PoolPath="Assets/Data/SkillPools/WindShared.asset";
        [MenuItem("Dragon Tower/Production/Connect Zephyr Reference")]
        public static void Install()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");
            if(dragon==null||dragon.StableId!="zephyr")throw new InvalidOperationException("Expected existing Zephyr data.");
            Directory.CreateDirectory("Assets/Resources/DragonAnimations");Directory.CreateDirectory("Assets/Data/SkillPools");AssetDatabase.Refresh();
            var set=AssetDatabase.LoadAssetAtPath<DragonAnimationSet>(AnimationPath);
            if(set==null)
            {
                set=ScriptableObject.CreateInstance<DragonAnimationSet>();set.archetype=DragonAnimationArchetype.SmallQuadruped;
                set.idle=dragon.idleFrames;set.attack=dragon.attackFrames;set.skill=dragon.skillFrames;set.dodge=dragon.dodgeFrames;set.hit=dragon.hitFrames;set.death=dragon.deathFrames;
                AssetDatabase.CreateAsset(set,AnimationPath);
            }
            var pool=AssetDatabase.LoadAssetAtPath<ElementSkillPool>(PoolPath);
            if(pool==null)
            {
                pool=ScriptableObject.CreateInstance<ElementSkillPool>();pool.element=ElementType.Wind;
                var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");
                pool.skills=(db.skills??Array.Empty<SkillData>()).Where(s=>s!=null&&s.elementType==ElementType.Wind&&s.rarity!=ContentRarity.Legendary&&string.IsNullOrEmpty(s.exclusiveDragonId)).Distinct().ToArray();
                AssetDatabase.CreateAsset(pool,PoolPath);
            }
            // Connect once; rerunning the utility never resets later author edits.
            if(string.IsNullOrWhiteSpace(dragon.animationSetPath))
            {dragon.animationSetPath="DragonAnimations/ZephyrBase";dragon.basicPresentation=DragonBasicPresentation.PixelWind;}
            if(dragon.elementSkillPool==null)dragon.elementSkillPool=pool;
            if(dragon.signatureSkill==null)dragon.signatureSkill=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/skill_dantian.asset");
            EditorUtility.SetDirty(dragon);AssetDatabase.SaveAssets();
        }
    }
}
