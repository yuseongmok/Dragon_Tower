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
            Check(database.items.Length==30,"Thirty balanced items are registered");
            Check(database.items.Count(i=>i.grade==ItemGrade.Common)==3,"Three common items are registered");
            Check(database.items.Count(i=>i.grade==ItemGrade.Rare)==13,"Thirteen rare items are registered");
            Check(database.items.Count(i=>i.grade==ItemGrade.Epic)==10,"Ten epic items are registered");
            Check(database.items.Count(i=>i.grade==ItemGrade.Unique)==4,"Four unique items are registered");
            Check(ContentDatabase.ItemGradeWeight(ItemGrade.Common)==55&&ContentDatabase.ItemGradeWeight(ItemGrade.Rare)==30&&ContentDatabase.ItemGradeWeight(ItemGrade.Epic)==12&&ContentDatabase.ItemGradeWeight(ItemGrade.Unique)==3,"Item rarity weights are 55/30/12/3");
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
            var run=new TowerRun(100,0,50);
            var maxItem=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_hardened_armor.asset");run.AddItem(maxItem);Check(run.MaxHP==120&&run.CurrentHP==120,"Equipped maximum HP item updates the run");
            var attackItem=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_hardened_claw.asset");run.AddItem(attackItem);
            var stats=run.BuildBattleStats(database.dragons[0].Snapshot());Check(stats.attackDamage==(int)Math.Round(database.dragons[0].attackDamage*1.1f),"Equipped attack bonus reaches battle stats");
            var potion=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_healing_potion.asset");Check(!run.CanAddItem(potion),"Two different item types fill both inventory slots");
            run.AddItem(potion,0);Check(run.Items.Count==2&&run.ItemAt(0)==potion,"A new item can replace either occupied slot");
            run.AddItem(potion);Check(run.ItemCountAt(0)==2,"Duplicate items stack in one of the two slots");
            var itemBattle=new BattleModel(run.BuildBattleStats(database.dragons[0].Snapshot()));itemBattle.Tick(2.41f);int wounded=itemBattle.PlayerHP;
            Check(itemBattle.UseItem(potion)&&itemBattle.PlayerHP>wounded,"A healing consumable can be used during battle");run.ConsumeItem(0);Check(run.ItemCountAt(0)==1,"Using a consumable removes one stack");
            var shard=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_time_shard.asset");var invulnerableBattle=new BattleModel(database.dragons[0].Snapshot());invulnerableBattle.Tick(2f);
            Check(invulnerableBattle.UseItem(shard),"A timed combat consumable activates");invulnerableBattle.Tick(.5f);Check(invulnerableBattle.PlayerHP==database.dragons[0].maxHP,"Time shard blocks an enemy strike during its one-second window");
            Check(ContentValidation.Report()=="문제 없음","Content identifiers and required fields are valid");
            Debug.Log("CONTENT_SYSTEM_VALIDATION_OK_NO_BUILD: "+checks+" checks");
        }
    }
}
