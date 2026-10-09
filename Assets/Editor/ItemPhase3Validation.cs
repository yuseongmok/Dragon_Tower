using System;using System.Linq;using UnityEngine;using UnityEditor;
namespace DragonTower.Editor {
 public static class ItemPhase3Validation {
  static int checks;static void Check(bool b,string s){if(!b)throw new Exception("ITEM_PHASE3_FAIL "+s);checks++;}
  static BattleModel Model(ItemData i,int hp=5000){var d=new BattleStats{speciesId="item-test",maxHP=10000,attackDamage=100,attackCooldown=.1f,skill=new SkillStats{damage=100,cooldown=20},items=new[]{i.Snapshot()},augments=Array.Empty<BattleAugment>()};return new BattleModel(d,new BattleEnemyStats{maxHP=1000000,damage=100,interval=1},hp,()=>.5);}
  public static void Run(){try{var db=ContentDatabase.Load();ItemDelayValidation.Verify();Check(db.items.All(x=>x!=null),"resolved database");Check(db.items.Select(x=>x.StableId).Distinct().Count()==db.items.Length,"unique IDs");Check(db.items.Select(x=>x.displayName).Distinct().Count()==db.items.Length,"unique names");
   Check(db.items.First(x=>x.StableId=="item_ember_bead").autoTrigger.enabled,"new rule deserialized");Debug.Log(string.Join(";",db.items.Select(x=>x.StableId+":"+x.kind+":"+x.autoTrigger.enabled)));
   foreach(var i in db.items.Where(x=>x.kind==ItemKind.Equipment&&x.autoTrigger.enabled)){
    var r=i.autoTrigger;var m=Model(i,r.belowHealthPercent>0?2000:5000);int hp=m.PlayerHP;double ready=0;
    if(r.trigger==ItemTrigger.DodgeSucceeded){m.Tick(.8f);m.Dodge();m.Tick(.3f);}else if(r.trigger==ItemTrigger.DamageTaken)m.Tick(1.01f);else if(r.trigger==ItemTrigger.BasicHit)m.Attack();else {m.Skill();m.Tick(.05f);}
    Check(m.ItemProcCount==1,i.StableId+" trigger");ready=m.ItemCooldownRemaining(i.StableId);Check(ready>0&&ready<=r.cooldown,i.StableId+" cooldown");
    if(r.effect==ItemProcEffect.Attack){m.Tick(r.activationDelay+r.initialDelay+r.hitCount*r.hitInterval+.1f);Check(m.ItemProcHitCount==r.hitCount,i.StableId+" hit count");}
    if(r.effect==ItemProcEffect.Heal)Check(m.PlayerHP>hp,i.StableId+" heal");
    if(r.effect==ItemProcEffect.HealAndWard)Check(m.PlayerHP>hp&&m.ShieldHP>0,i.StableId+" heal ward");
    var saved=m.CaptureItemCooldowns();var restored=Model(i);restored.RestoreItemCooldowns(saved);Check(Math.Abs(restored.ItemCooldownRemaining(i.StableId)-m.ItemCooldownRemaining(i.StableId))<.0001,i.StableId+" saved cooldown");
    int count=m.ItemProcCount;m.Attack();m.Skill();Check(m.ItemProcCount==count,i.StableId+" no immediate duplicate");m.Dragon.items=Array.Empty<BattleItem>();m.Tick(30);m.Attack();Check(m.ItemProcCount==count,i.StableId+" removal");
   }
   foreach(var i in db.items.Where(x=>x.hasLegacyConsumable)){var old=i.AsSavedConsumable();Check(old.kind==ItemKind.Consumable&&old.StableId==i.StableId&&!old.autoTrigger.enabled,"legacy charges "+i.StableId);}
   if(db.items.Length==50){Check(db.items.Count(x=>x.kind==ItemKind.Consumable)==10,"10 consumables");foreach(var pair in new[]{(ItemGrade.Common,15),(ItemGrade.Rare,10),(ItemGrade.Unique,10),(ItemGrade.Legendary,5)})Check(db.items.Count(x=>x.kind==ItemKind.Equipment&&x.grade==pair.Item1)==pair.Item2,"tier "+pair.Item1);}
   foreach(var i in db.items.Where(x=>x.kind==ItemKind.Consumable)){
    var m=Model(i,1000);m.Skill();int hp=m.PlayerHP;Check(m.UseItem(i),"consume "+i.StableId);
    switch(i.mechanic){case ItemMechanic.HealingPotion:case ItemMechanic.GreaterHealingPotion:case ItemMechanic.BloodPotion:case ItemMechanic.PurificationRod:Check(m.PlayerHP>hp,"healing "+i.StableId);break;case ItemMechanic.BarrierStone:Check(m.ShieldHP>0,"barrier");break;case ItemMechanic.BurstCore:Check(m.SkillReady<=m.Time,"skill reset");break;case ItemMechanic.FrostWitchTear:Check(m.EnemySlowed,"consumable slow");break;case ItemMechanic.StunGun:Check(m.EnemyParalyzed,"consumable paralysis");break;case ItemMechanic.TimeShard:m.Tick(.5f);Check(m.PlayerHP==hp,"invulnerable");break;case ItemMechanic.WeaknessLens:Check(m.Attack(),"crit buff attack");break;}
    var d=db.dragons[0];var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);r.AddItem(i);r.AddItem(i);r.AddItem(i);Check(!r.CanAddItem(i),"stack cap "+i.StableId);r.ConsumeItem(3);var copy=TowerRun.Restore(r.Capture(),db);Check(copy.ItemCountAt(3)==2&&copy.ItemAt(3).StableId==i.StableId,"consumed quantity saved "+i.StableId);
   }
   foreach(var i in db.items.Where(x=>x.hasLegacyConsumable)){
    var d=db.dragons[0];var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);var save=r.Capture();save.inventoryVersion=1;save.consumable=new SavedItemSlot{id=i.StableId,count=3};var old=TowerRun.Restore(save,db);Check(old.ItemCountAt(3)==3&&old.ItemAt(3).kind==ItemKind.Consumable,"legacy save "+i.StableId);Check(Model(i).UseItem(old.ItemAt(3)),"legacy use "+i.StableId);var twice=TowerRun.Restore(old.Capture(),db);Check(twice.ItemAt(3).kind==ItemKind.Consumable,"legacy repeat save "+i.StableId);
   }
   var accel=Model(db.items.First(x=>x.StableId=="item_emergency_accelerator"));accel.Skill();double skillReady=accel.SkillReady;accel.Tick(.8f);accel.Dodge();accel.Tick(.3f);Check(Math.Abs(accel.SkillReady-(skillReady-2))<.001,"recharge exact two seconds");
   foreach(var i in db.items.Where(x=>x.kind==ItemKind.Equipment&&x.effects.Count>0)){
    var d=db.dragons[0];var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);r.AddItem(i);r.AddItem(db.items.First(x=>x.StableId=="item_infighting_glove"));r.AddItem(db.items.First(x=>x.StableId=="item_mana_prism"));var before=r.BuildBattleStats(d.Snapshot());var copy=TowerRun.Restore(r.Capture(),db);var after=copy.BuildBattleStats(d.Snapshot());Check(JsonUtility.ToJson(before)==JsonUtility.ToJson(after),"stat save not doubled "+i.StableId);copy.AddItem(db.items.First(x=>x.StableId=="item_guardian_brooch"),0);var clean=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);clean.AddItem(db.items.First(x=>x.StableId=="item_guardian_brooch"));clean.AddItem(db.items.First(x=>x.StableId=="item_infighting_glove"));clean.AddItem(db.items.First(x=>x.StableId=="item_mana_prism"));var aa=JsonUtility.ToJson(copy.BuildBattleStats(d.Snapshot()));var bb=JsonUtility.ToJson(clean.BuildBattleStats(d.Snapshot()));Check(aa==bb,"stat removed "+i.StableId);
   }
   foreach(var name in new[]{"제피르","루나","솔","엠버","베놈","옥타"}){
    var d=db.dragons.First(x=>x.displayName==name);var stats=d.Snapshot(20);stats.maxHP=100000;stats.items=new[]{db.items.First(x=>x.StableId=="item_chain_lightning").Snapshot(),db.items.First(x=>x.StableId=="item_poison_fang").Snapshot(),db.items.First(x=>x.StableId=="item_mana_prism").Snapshot()};stats.augments=Array.Empty<BattleAugment>();if(d.signatureSkill!=null)stats.skill=d.signatureSkill.Snapshot();var m=new BattleModel(stats,new BattleEnemyStats{maxHP=10000000,damage=1,interval=1},stats.maxHP,()=>.5);m.Attack();m.Skill();for(int n=0;n<100;n++)m.Tick(.05f);Check(m.ItemProcCount>=1&&m.ItemProcCount<=3,"signature no proc chain "+name);Check(m.Dragon.passiveMechanic==stats.passiveMechanic,"passive intact "+name);
   }
   var feather=db.items.First(x=>x.StableId=="item_phoenix_feather");var st=Model(feather).Dragon;var phoenix=new BattleModel(st,new BattleEnemyStats{maxHP=100000,damage=20000,interval=1},10000,()=>.5);phoenix.Tick(1.01f);Check(phoenix.PlayerHP==2500&&phoenix.Result==BattleResult.Fighting,"feather revive 25 percent");phoenix.Tick(1);Check(phoenix.Result==BattleResult.Defeat,"feather only once");
   typeof(ItemPhase2Validation).GetField("db",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).SetValue(null,db);
   typeof(ItemPhase2Validation).GetMethod("Verify",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,null);
   typeof(ItemElementValidation).GetMethod("Verify",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,null);
   ItemProcValidation.Verify();InventorySplitValidation.Verify();Debug.Log("ITEM_PHASE3_OK checks="+checks+" items="+db.items.Length);EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
