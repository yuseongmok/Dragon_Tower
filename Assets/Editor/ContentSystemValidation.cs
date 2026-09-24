using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class ContentSystemValidation
    {
        static int checks;
        static void Check(bool value,string message){if(!value)throw new Exception(message);checks++;Debug.Log("CONTENT_CHECK "+message);}
        [MenuItem("Dragon Tower/검증/콘텐츠 시스템 검사")]
        public static void Run()
        {
            checks=0;var database=ContentDataSetup.Install(false);
            Check(database!=null,"Content database exists");
            Check(database.dragons.Length==8,"All eight illustrated dragons are registered");
            Check(database.monsters.Length==6,"First-area monsters and boss are registered");
            Check(database.items.Length==3,"Starter items are registered");
            Check(database.augments.Length==43,"Twenty-nine original and fourteen new augments are registered");
            Check(database.augments.Count(a=>a.grade==AugmentGrade.Common)==9,"Nine common augments are registered");
            Check(database.augments.Count(a=>a.grade==AugmentGrade.Rare)==14,"Fourteen rare augments are registered");
            Check(database.augments.Count(a=>a.grade==AugmentGrade.Epic)==14,"Fourteen epic augments are registered");
            Check(database.augments.Count(a=>a.grade==AugmentGrade.Unique)==6,"Six unique augments are registered");
            Check(ContentDatabase.AugmentGradeWeight(AugmentGrade.Common)==60&&ContentDatabase.AugmentGradeWeight(AugmentGrade.Rare)==28&&ContentDatabase.AugmentGradeWeight(AugmentGrade.Epic)==10&&ContentDatabase.AugmentGradeWeight(AugmentGrade.Unique)==2,"Augment rarity weights are 60/28/10/2");
            Check(database.skills.Length==32&&Array.TrueForAll(database.skills,s=>s!=null&&s.battleVfx!=null),"All thirty-two skills have Built-in VFX prefabs");
            foreach(var element in new[]{ElementType.Fire,ElementType.Ice,ElementType.Wind,ElementType.Earth,ElementType.Lightning,ElementType.Water,ElementType.Dark,ElementType.Light})
                Check(database.skills.Count(s=>s.elementType==element)==4,"Each dragon element has four skills: "+element);
            var regular=database.PickMonster(1,false,0);Check(regular!=null&&!regular.boss,"Floor one selects a regular monster");
            var boss=database.PickMonster(10,true,0);Check(boss!=null&&boss.boss&&boss.elementType==ElementType.Earth,"Floor ten selects the earth boss");
            var floor20=boss.CreateBattleStats(20);Check(floor20.maxHP==420&&floor20.damage==26,"Boss progression preserves existing floor scaling");
            var run=new TowerRun(100,0,50);run.AddItem(database.items[0]);
            Check(run.Items.Count==1,"Data item is recorded in the current run");
            var maxItem=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_iron_scale.asset");run.AddItem(maxItem);Check(run.MaxHP==115&&run.CurrentHP==115,"Item effects are applied from data");
            var attackItem=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_sharp_claw.asset");run.AddItem(attackItem);
            var stats=run.BuildBattleStats(database.dragons[0].Snapshot());Check(stats.attackDamage==database.dragons[0].attackDamage+2,"Data attack bonus reaches battle stats");
            Check(ContentValidation.Report()=="문제 없음","Content identifiers and required fields are valid");
            Debug.Log("CONTENT_SYSTEM_VALIDATION_OK_NO_BUILD: "+checks+" checks");
        }
    }
}
