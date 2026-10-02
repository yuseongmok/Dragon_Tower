using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class DragonProductionValidation
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception("PRODUCTION: "+message);Debug.Log("PRODUCTION_CHECK "+message);}
        static object Field(object obj,string name)=>obj.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(obj);
        public static void Validate(BattleView view,DragonData dragon)
        {
            Check(DragonProductionChecks.Validate(dragon).Count==0,string.Join(";",DragonProductionChecks.Validate(dragon)) + " Zephyr template valid");
            var set=dragon.LoadAnimationSet(0);
            foreach(DragonAnimationState state in Enum.GetValues(typeof(DragonAnimationState)))Check(set.Get(state)==dragon.LegacyAnimation(state,0),state+" uses identical approved frames");
            for(int stage=1;stage<=2;stage++)Check((dragon.LoadAnimationSet(stage)!=null)==!string.IsNullOrWhiteSpace(dragon.AnimationPath(stage)),"Evolution set resolves only when configured: "+stage);
            var db=ContentDatabase.Load();
            foreach(var skill in db.skills.Where(s=>s!=null))
                Check(dragon.CanOfferSkill(skill)==(skill.CanEquip(dragon.StableId)&&dragon.CanLearnSkill(skill.elementType)),"Existing reward eligibility preserved: "+skill.StableId);
            string before=EditorJsonUtility.ToJson(dragon);DragonProductionSetup.Install();Check(before==EditorJsonUtility.ToJson(dragon),"Migration idempotent");
            var clone=UnityEngine.Object.Instantiate(dragon);var fake=ScriptableObject.CreateInstance<SkillData>();
            try
            {
                clone.speciesId="production_validation_only";clone.signatureSkill=null;
                view.SetDragonArt(clone);var motion=view.GetComponent<BattleAnimation>();
                Check((bool)Field(motion,"zephyrWind")&&Field(motion,"skillFrames")==set.skill,"Shared presentation works without Zephyr ID");
                Check(!clone.CanOfferSkill(dragon.signatureSkill),"Another Wind dragon cannot learn Dantian");
                foreach(var skill in dragon.elementSkillPool.skills)Check(clone.CanOfferSkill(skill),"Wind pool reusable: "+skill.StableId);
                var run=new TowerRun(clone.maxHP,0,1,clone.elementType,clone.alternateSkillElement,clone.StableId,clone);
                bool denied=false;try{run.ReplaceSkill(dragon.signatureSkill,false);}catch(InvalidOperationException){denied=true;}Check(denied,"Signature replacement owner guard");
                fake.elementType=ElementType.Wind;fake.rarity=ContentRarity.Rare;Check(!clone.CanOfferSkill(fake),"Unlisted skill rejected");
                clone.evolutionForms=new[]{new DragonEvolutionForm{stage=1,displayName="Validation form",battleSprite=dragon.battleSprite,animationSetPath=dragon.animationSetPath,defaultSkill=dragon.elementSkillPool.skills.First(s=>s!=dragon.skill),healthMultiplier=1.7f,attackMultiplier=1.6f}};
                view.SetDragonArt(clone,10);Check(Field(motion,"skillFrames")==set.skill&&clone.NameAtLevel(10)=="Validation form"&&clone.SkillAtLevel(10)==clone.evolutionForms[0].defaultSkill,"Evolution swaps set/name/default skill through shared view");
                Check(clone.HealthMultiplier(1)==1.7f&&clone.AttackMultiplier(1)==1.6f&&clone.HealthMultiplier(2)==1.45f,"Evolution overrides and legacy fallback");
                run.ReplaceSkill(dragon.elementSkillPool.skills[0],false);Check(run.CurrentSkill(clone.SkillAtLevel(10))==dragon.elementSkillPool.skills[0],"Acquired replacement takes priority over evolution default");
                clone.animationSetPath="MissingValidationOnly";clone.battleSprite=null;Check(DragonProductionChecks.Validate(clone).Count>=3,"Missing production settings reported before runtime");
            }
            finally{UnityEngine.Object.DestroyImmediate(clone);UnityEngine.Object.DestroyImmediate(fake);view.SetDragonArt(dragon);}
            foreach(var skill in dragon.elementSkillPool.skills.Concat(new[]{dragon.signatureSkill}))
            {
                var oldRun=new TowerRun(dragon.maxHP,0,1,dragon.elementType,dragon.alternateSkillElement,dragon.StableId);
                var newRun=new TowerRun(dragon.maxHP,0,1,dragon.elementType,dragon.alternateSkillElement,dragon.StableId,dragon);
                oldRun.ReplaceSkill(skill,false);newRun.ReplaceSkill(skill,false);
                var aStats=oldRun.BuildBattleStats(dragon.Snapshot());var bStats=newRun.BuildBattleStats(dragon.Snapshot());
                Check(JsonUtility.ToJson(aStats)==JsonUtility.ToJson(bStats),"Identical combat snapshot: "+skill.StableId);
                var enemy=BattleEnemyStats.Normal();enemy.maxHP=100000;
                var a=new BattleModel(aStats,enemy,aStats.maxHP,()=>.9);var b=new BattleModel(bStats,enemy,bStats.maxHP,()=>.9);
                string traceA="",traceB="";a.Cue+=(cue,damage)=>traceA+=a.Time+":"+cue+":"+damage+";";b.Cue+=(cue,damage)=>traceB+=b.Time+":"+cue+":"+damage+";";
                for(int frame=0;frame<1800;frame++)
                {
                    if(frame%30==0){a.Attack();b.Attack();}if(frame%120==0){a.Skill();b.Skill();}if(frame%90==0){a.Dodge();b.Dodge();}
                    a.Tick(1f/60);b.Tick(1f/60);
                    if(a.PlayerHP!=b.PlayerHP||a.EnemyHP!=b.EnemyHP||a.SkillReady!=b.SkillReady||a.AttackReady!=b.AttackReady||a.DodgeReady!=b.DodgeReady)throw new Exception("Combat mismatch: "+skill.StableId+" frame "+frame);
                }
                Check(traceA==traceB,"30-second damage/event/cooldown trace unchanged: "+skill.StableId);
            }
            Debug.Log("DRAGON_PRODUCTION_VALIDATION_OK");
        }
    }
}
