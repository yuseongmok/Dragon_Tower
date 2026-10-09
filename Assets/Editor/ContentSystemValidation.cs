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
            Check(database.dragons.Length==16,"All sixteen illustrated dragons are registered");
            Check(database.dragons.Select(d=>d.passiveMechanic).Distinct().Count()==16,"Every dragon has a distinct passive mechanic");
            var storm=database.dragons.First(d=>d.StableId=="storm");var sharkid=database.dragons.First(d=>d.StableId=="sharkid");
            Check(storm.CanLearnSkill(ElementType.Wind)&&storm.CanLearnSkill(ElementType.Lightning),"Storm can learn lightning and wind skills");
            Check(sharkid.CanLearnSkill(ElementType.Water)&&sharkid.CanLearnSkill(ElementType.Ice),"Sharkid can learn water and ice skills");
            Check(database.monsters.Length==16,"Floor one through thirty monsters and bosses are registered");
            Check(database.items.Length==50,"Fifty items registered");
            Check(database.items.Count(i=>i.kind==ItemKind.Consumable)==10,"Ten consumables");
            foreach(var pair in new[]{(ItemGrade.Common,15),(ItemGrade.Rare,10),(ItemGrade.Unique,10),(ItemGrade.Legendary,5)})Check(database.items.Count(i=>i.kind==ItemKind.Equipment&&i.grade==pair.Item1)==pair.Item2,"Equipment grade "+pair.Item1);
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
            var floor10=boss.CreateBattleStats(10);Check(floor10.maxHP==360&&floor10.damage==24&&floor10.bossPattern==EnemyBossPattern.EarthShatter,"Floor ten boss uses the charged earth-shatter pattern");
            var secondArea=database.PickMonster(11,false,0);Check(secondArea!=null&&secondArea.minimumFloor==11&&secondArea.timingVariancePercent>0,"Floor eleven selects a stronger irregular attacker");
            var kraken=database.PickMonster(20,true,0);var floor20=kraken.CreateBattleStats(20);
            Check(kraken.contentId=="boss_kraken_guardian"&&floor20.maxHP==650&&floor20.damage==32&&floor20.bossPattern==EnemyBossPattern.AbyssalRush,"Floor twenty selects the Kraken Guardian and its tentacle pattern");
            var vulcan=database.PickMonster(30,true,0);var floor30=vulcan.CreateBattleStats(30);
            Check(vulcan.contentId=="boss_forge_warden_vulcan"&&floor30.maxHP==1100&&floor30.bossPattern==EnemyBossPattern.ForgeBarrier&&floor30.barrierHP==220,"Floor thirty selects Vulcan and its forge barrier pattern");
            var legacyBoss=ScriptableObject.CreateInstance<MonsterData>();legacyBoss.contentId="legacy";legacyBoss.boss=true;legacyBoss.minimumFloor=10;legacyBoss.maximumFloor=999;legacyBoss.spawnWeight=999;
            var exactBoss=ScriptableObject.CreateInstance<MonsterData>();exactBoss.contentId="exact";exactBoss.boss=true;exactBoss.minimumFloor=20;exactBoss.maximumFloor=20;exactBoss.spawnWeight=1;
            var bossSelector=ScriptableObject.CreateInstance<ContentDatabase>();bossSelector.monsters=new[]{legacyBoss,exactBoss};Check(bossSelector.PickMonster(20,true,0)==exactBoss,"An exact-floor boss overrides legacy broad boss ranges");
            UnityEngine.Object.DestroyImmediate(legacyBoss);UnityEngine.Object.DestroyImmediate(exactBoss);UnityEngine.Object.DestroyImmediate(bossSelector);
            var barrierStats=new BattleEnemyStats{displayName="barrier test",maxHP=999,damage=0,interval=.1f,bossPattern=EnemyBossPattern.ForgeBarrier,patternEveryAttacks=2,barrierHP=100,barrierDuration=1,barrierFailureDamagePercent=50};
            var barrierBattle=new BattleModel(database.dragons[0].Snapshot(),barrierStats,100,()=>.9);barrierBattle.Tick(.11f);
            Check(barrierBattle.EnemyShieldHP==100,"Vulcan creates a separate breakable shield");barrierBattle.Tick(1.01f);Check(barrierBattle.PlayerHP==50,"An unbroken forge barrier deals guard-ignoring maximum-HP damage");
            var quickStats=new BattleEnemyStats{displayName="timing test",maxHP=999,damage=1,interval=2,quickAttackChancePercent=100,quickAttackIntervalMultiplier=.5f};
            var quickBattle=new BattleModel(database.dragons[0].Snapshot(),quickStats,database.dragons[0].maxHP,()=>0);
            quickBattle.Tick(2.01f);Check(quickBattle.EnemyIntent=="기습 공격"&&quickBattle.NextEnemyStrike<3.1,"Irregular enemies can suddenly schedule a faster attack");
            var run=new TowerRun(100,0,50);
            var maxItem=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_hardened_armor.asset");run.AddItem(maxItem);Check(run.MaxHP==120&&run.CurrentHP==120,"Equipped maximum HP item updates the run");
            var attackItem=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_hardened_claw.asset");run.AddItem(attackItem);
            var stats=run.BuildBattleStats(database.dragons[0].Snapshot());Check(stats.attackDamage==(int)Math.Round(database.dragons[0].attackDamage*1.1f),"Equipped attack bonus reaches battle stats");
            var potion=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_healing_potion.asset");Check(run.CanAddItem(potion),"Consumable fits its independent slot");
            run.AddItem(potion);Check(run.Items.Count==3&&run.ItemAt(3)==potion,"Two equipment and one dedicated consumable are stored");
            var shardForCapacity=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_time_shard.asset");Check(!run.CanAddItem(shardForCapacity),"Different consumable requires replacement");
            run.AddItem(potion);Check(run.ItemCountAt(3)==2,"Consumables stack in the dedicated slot");
            var itemBattle=new BattleModel(run.BuildBattleStats(database.dragons[0].Snapshot()));itemBattle.Tick(2.41f);int wounded=itemBattle.PlayerHP;
            Check(itemBattle.UseItem(potion)&&itemBattle.PlayerHP>wounded,"A healing consumable can be used during battle");run.ConsumeItem(3);Check(run.ItemCountAt(3)==1,"Using the dedicated consumable removes one stack");
            var shard=AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/item_time_shard.asset");var invulnerableBattle=new BattleModel(database.dragons[0].Snapshot());invulnerableBattle.Tick(2f);
            Check(invulnerableBattle.UseItem(shard),"A timed combat consumable activates");invulnerableBattle.Tick(.5f);Check(invulnerableBattle.PlayerHP==database.dragons[0].maxHP,"Time shard blocks an enemy strike during its one-second window");
            Check(ContentValidation.Report()=="문제 없음","Content identifiers and required fields are valid");
            Debug.Log("CONTENT_SYSTEM_VALIDATION_OK_NO_BUILD: "+checks+" checks");
        }
    }
}
