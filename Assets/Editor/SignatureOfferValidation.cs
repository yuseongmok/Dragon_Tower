using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class SignatureOfferValidation
    {
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
        static TowerRun RunFor(DragonData d,int level,bool finish=true)
        {
            var run=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);
            while(run.Level<level){run.RecordBattleVictory(run.CurrentHP);if(run.PendingEvolutionStage>0&&(finish||run.Level<level))run.ConsumeEvolution();}
            return run;
        }
        public static void Run()
        {
            try{Test();Debug.Log("SIGNATURE_OFFERS_OK");EditorApplication.Exit(0);}
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        static void Test()
        {
            var db=ContentDatabase.Load();Check(db!=null,"database");
            var dragons=Enumerable.Range(0,16).Select(i=>AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+i+".asset")).ToArray();
            var go=new GameObject("Signature reward test");go.SetActive(false);var flow=go.AddComponent<CollectionFlow>();
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            typeof(CollectionFlow).GetField("database",flags).SetValue(flow,db);
            var session=new CollectionSession(new ProfileStore("DragonTower.Test.Signature."+Guid.NewGuid().ToString("N")),dragons);
            typeof(CollectionFlow).GetProperty("Session").SetValue(flow,session);
            var pick=typeof(CollectionFlow).GetMethod("PickRandomRewards",flags);
            foreach(var d in dragons)
            {
                var signature=d.signatureSkill;Check(signature!=null&&db.skills.Contains(signature),d.StableId+" signature/database link");
                Check(signature.CanEquip(d.StableId)&&signature.IsSignatureSkill,d.StableId+" ownership");
                var basic=db.skills.First(s=>s!=null&&!s.IsSignatureSkill&&d.CanOfferSkill(s));
                session.Profile.dragons.Clear();session.Profile.dragons.Add(new OwnedDragon{instanceId="test",speciesId=d.StableId});session.Profile.selectedInstanceId="test";
                foreach(int level in new[]{1,10,19})
                {
                    var run=RunFor(d,level);Check(!run.CanOfferSkill(signature)&&run.CanOfferSkill(basic),d.StableId+" early gate "+level);
                    bool blocked=false;try{run.ReplaceSkill(signature,false);}catch(InvalidOperationException){blocked=true;}Check(blocked,"selection bypass blocked");
                    typeof(CollectionFlow).GetField("towerRun",flags).SetValue(flow,run);
                    for(int seed=0;seed<200;seed++)foreach(var reward in (Array)pick.Invoke(flow,new object[]{seed,3}))
                    {var skill=(SkillData)reward.GetType().GetField("skill").GetValue(reward);Check(skill==null||!skill.IsSignatureSkill,"early signature shown");}
                }
                var pending=RunFor(d,20,false);Check(!pending.CanOfferSkill(signature),"pending evolution gate");pending.ConsumeEvolution();Check(pending.CanOfferSkill(signature),"level20 completed unlock");
                foreach(var other in dragons.Where(x=>x!=d))Check(!pending.CanOfferSkill(other.signatureSkill),"foreign signature");
                typeof(CollectionFlow).GetField("towerRun",flags).SetValue(flow,pending);
                int sig=0,ordinary=0;const int samples=20000;
                for(int seed=0;seed<samples;seed++)
                {
                    var seen=new System.Collections.Generic.HashSet<SkillData>();
                    foreach(var reward in (Array)pick.Invoke(flow,new object[]{seed,3}))
                    {
                        var skill=(SkillData)reward.GetType().GetField("skill").GetValue(reward);if(skill==null)continue;
                        Check(seen.Add(skill),"duplicate skill card");Check(skill.StableId!=pending.CurrentSkillId(d.SkillAtLevel(20)),"current skill excluded");
                        if(skill.IsSignatureSkill){Check(skill==signature,"wrong signature");sig++;}else ordinary++;
                    }
                }
                int ordinaryCount=db.skills.Count(s=>s!=null&&!s.IsSignatureSkill&&pending.CanOfferSkill(s)&&s.StableId!=pending.CurrentSkillId(d.SkillAtLevel(20)));
                Check(sig>0&&sig<ordinary/(float)ordinaryCount,d.StableId+" lower observed offer rate");
                pending.ReplaceSkill(signature,false);Check(pending.CurrentSkill(null)==signature,"final selection");
                for(int seed=0;seed<200;seed++)foreach(var reward in (Array)pick.Invoke(flow,new object[]{seed,3}))Check((SkillData)reward.GetType().GetField("skill").GetValue(reward)!=signature,"equipped signature excluded");
                Debug.Log($"SIGNATURE_OFFER {d.StableId}: {sig}/{samples} panels, ordinary per skill={ordinary/(float)ordinaryCount/samples:P2}, eligibility={signature.EffectiveRewardEligibilityPercent}");
            }
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
