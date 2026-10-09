using System;using System.Linq;using UnityEngine;using UnityEditor;
namespace DragonTower.Editor {
 public static class ItemProcValidation {
 static int checks;static void Check(bool ok,string label){if(!ok)throw new Exception("ITEM_PROC_FAIL "+label);checks++;}
 static BattleItem Item(string id,ItemTrigger trigger,float cooldown=10,bool perHit=false,float chance=100)=>new BattleItem{id=id,displayName=id,autoTrigger=new ItemAutoTrigger{enabled=true,trigger=trigger,cooldown=cooldown,damage=7,hitCount=1,chancePercent=chance,eachSkillHit=perHit}};
 static BattleModel Model(params BattleItem[] items){var s=ContentDatabase.Load().dragons[0].Snapshot();s.passiveMechanic=DragonPassiveMechanic.None;s.augments=Array.Empty<BattleAugment>();s.items=items;s.criticalChance=0;s.attackDamage=10;s.attackCooldown=.1f;s.baseAttackCooldown=.1f;s.skill=new SkillStats{damage=10,hitCount=3,hitInterval=.1f,cooldown=1,elementType=s.elementType};return new BattleModel(s,new BattleEnemyStats{maxHP=100000,damage=1,interval=100},s.maxHP,()=>.5);}
 public static void Run(){try{Verify();InventorySplitValidation.Verify();MonsterRewardValidation.Verify();ProgressionRewardsValidation.Verify();FloorProgressionValidation.Verify();Debug.Log("ITEM_PROC_OK checks="+checks+" plus inventory floor score shop regression");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 public static void Verify(){
 var b=Model(Item("a",ItemTrigger.BasicHit,10),Item("b",ItemTrigger.BasicHit,12),Item("c",ItemTrigger.BasicHit,15));b.Attack();Check(b.ItemProcCount==3&&b.ItemProcHitCount==3,"three independent procs no recursion");Check(Math.Abs(b.ItemCooldownRemaining("a")-10)<.001&&Math.Abs(b.ItemCooldownRemaining("b")-12)<.001,"independent cooldown");b.Tick(.2f);b.Attack();Check(b.ItemProcCount==3,"cooldown blocks");b.Tick(9.8f);b.Attack();Check(b.ItemProcCount==4,"only ten second ready");
 var saved=b.CaptureItemCooldowns();var fresh=Model(Item("a",ItemTrigger.BasicHit,10),Item("b",ItemTrigger.BasicHit,12),Item("c",ItemTrigger.BasicHit,15));fresh.RestoreItemCooldowns(saved);fresh.Attack();Check(fresh.ItemProcCount==0,"saved cooldown not reset");
 var multi=Model(Item("once",ItemTrigger.SkillHit,0),Item("each",ItemTrigger.SkillHit,0,true),Item("cast",ItemTrigger.SkillUsed,0));multi.Skill();multi.Tick(.4f);Check(multi.ItemProcCount==5,"three hits once plus each plus cast");
 var duplicates=Model(Item("same",ItemTrigger.BasicHit,0),Item("same",ItemTrigger.BasicHit,0));duplicates.Attack();Check(duplicates.ItemProcCount==1,"duplicate IDs defense");
 var none=Model(Item("never",ItemTrigger.BasicHit,0,false,0));none.Attack();Check(none.ItemProcCount==0,"zero chance");
 var delayed=Item("delayed",ItemTrigger.BasicHit,10);delayed.autoTrigger.hitCount=3;delayed.autoTrigger.initialDelay=.2f;var unmount=Model(delayed);unmount.Attack();Check(unmount.ItemProcHitCount==0,"delayed hit");unmount.Dragon.items=Array.Empty<BattleItem>();unmount.Tick(1);Check(unmount.ItemProcHitCount==0,"unequip cancels pending");
 var dodge=Model(Item("dodge",ItemTrigger.DodgeSucceeded,0),Item("hit",ItemTrigger.DamageTaken,0));dodge.Tick(99.9f);dodge.Dodge();dodge.Tick(.2f);Check(dodge.ItemProcCount==1,"successful dodge");dodge.Tick(100);Check(dodge.ItemProcCount==2,"actual incoming hit");
 var kill=Model(Item("kill",ItemTrigger.EnemyKilled,0));kill.Dragon.attackDamage=200000;kill.Attack();Check(kill.ItemProcCount==1&&kill.ItemProcHitCount==0,"kill event once no dead target hit");kill.Tick(1);kill.Attack();Check(kill.ItemProcCount==1,"no repeat kill");
 var status=Model(Item("status",ItemTrigger.SkillHit,0,true));status.Dragon.skill.statusEffect=CombatStatusEffect.Burn;status.Dragon.skill.statusChancePercent=100;status.Dragon.skill.statusDuration=5;status.Dragon.skill.statusPower=5;status.Skill();status.Tick(4);Check(status.ItemProcCount==3,"status damage excluded");
 var procKill=Model(Item("attack",ItemTrigger.BasicHit,0),Item("kill",ItemTrigger.EnemyKilled,0));procKill.Dragon.items[0].autoTrigger.damage=200000; // snapshots are immutable; build a new model with the configured value.
 procKill=Model(procKill.Dragon.items);procKill.Attack();Check(procKill.ItemProcCount==1,"item kill cannot chain another item");
 var poison=Item("poison",ItemTrigger.BasicHit);poison.autoTrigger.status=CombatStatusEffect.Poison;poison.autoTrigger.statusPower=5;var p=Model(poison);p.Attack();Check(p.EnemyPoisoned,"configured status applied");
 var sourceDb=ContentDatabase.Load();var db=ScriptableObject.CreateInstance<ContentDatabase>();db.dragons=sourceDb.dragons;db.items=sourceDb.items;db.skills=sourceDb.skills;db.augments=sourceDb.augments;
 var fixture=ScriptableObject.CreateInstance<ItemData>();fixture.contentId="proc-test-fixture";fixture.kind=ItemKind.Equipment;fixture.autoTrigger=new ItemAutoTrigger{enabled=true,trigger=ItemTrigger.BasicHit,damage=7,cooldown=10,hitCount=3,initialDelay=.2f,hitInterval=.15f};db.items=db.items.Concat(new[]{fixture}).ToArray();
 var d=db.dragons[0];var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);r.ItemCooldowns=saved;var rr=TowerRun.Restore(JsonUtility.FromJson<TowerRunSave>(JsonUtility.ToJson(r.Capture())),db);Check(rr.ItemCooldowns.Length==3&&rr.ItemCooldowns[0].itemId==saved[0].itemId,"run saves IDs slots remaining");
 foreach(float stop in new[]{.05f,.25f,.6f,11f}){
  var replayRun=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);replayRun.AddItem(fixture);
  var tape=new BattleResumeData{initialRunJson=JsonUtility.ToJson(replayRun.Capture(false)),seed=34,enemy=new BattleEnemyStats{maxHP=1000000,damage=1,interval=100}};
  var rng=new System.Random(34);var live=new BattleModel(replayRun.BuildBattleStats(d.Snapshot()),tape.enemy,replayRun.CurrentHP,()=>rng.NextDouble());live.Attack();tape.inputs.Add(new BattleInputRecord{action=1});live.Tick(stop);tape.inputs.Add(new BattleInputRecord{action=0,delta=stop});replayRun.ItemCooldowns=live.CaptureItemCooldowns();replayRun.BattleResume=tape;
  var loaded=TowerRun.Restore(JsonUtility.FromJson<TowerRunSave>(JsonUtility.ToJson(replayRun.Capture())),db);var replay=BattleReplay.Restore(loaded.BattleResume,db,"test");Check(replay.EnemyHP==live.EnemyHP&&replay.ItemProcCount==live.ItemProcCount&&Math.Abs(replay.ItemCooldownRemaining(fixture.StableId)-live.ItemCooldownRemaining(fixture.StableId))<.00001,"replay state "+stop);
  live.Tick(1);replay.Tick(1);Check(replay.EnemyHP==live.EnemyHP&&replay.ItemProcHitCount==live.ItemProcHitCount,"pending hits restored "+stop);
 }
 UnityEngine.Object.DestroyImmediate(fixture);UnityEngine.Object.DestroyImmediate(db);
 }
 }
}

