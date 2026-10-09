using System;using System.Linq;using UnityEngine;
namespace DragonTower.Editor {
 public static class ItemDelayValidation {
 static void Check(bool value,string label){if(!value)throw new Exception("ITEM_DELAY_FAIL "+label);}
 static BattleModel Model(ItemData item,int enemyHP=1000000){return new BattleModel(new BattleStats{maxHP=10000,attackDamage=100,attackCooldown=.1f,skill=new SkillStats{damage=100,cooldown=20},items=new[]{item.Snapshot()},augments=Array.Empty<BattleAugment>()},new BattleEnemyStats{maxHP=enemyHP,damage=1,interval=100},10000,()=>.5);}
 public static void Verify(){var db=ContentDatabase.Load();var items=db.items.Where(i=>i.autoTrigger.enabled&&i.autoTrigger.effect==ItemProcEffect.Attack&&(i.autoTrigger.trigger==ItemTrigger.SkillUsed||i.autoTrigger.trigger==ItemTrigger.SkillHit)).ToArray();Check(items.Length==9,"nine skill attack items");
 foreach(var i in items){var r=i.autoTrigger;Check(r.activationDelay==2,i.StableId+" data delay");var m=Model(i);int launches=0;m.ItemProcVisual+=v=>{if(v.started)launches++;};m.Skill();Check(m.ItemProcCount==1&&launches==0&&m.ItemCooldownRemaining(i.StableId)>0,"reserved without VFX");m.Tick(1.95f);Check(launches==0&&m.ItemProcHitCount==0,"silent wait");m.Tick(.06f);Check(launches==1&&m.ItemProcHitCount==0,"launch after two seconds before contact");m.Tick(r.initialDelay+r.hitCount*r.hitInterval+.1f);Check(m.ItemProcHitCount==r.hitCount&&launches==1,"all original hits once");
 var removed=Model(i);int removedLaunches=0;removed.ItemProcVisual+=v=>{if(v.started)removedLaunches++;};removed.Skill();removed.Tick(1);removed.Dragon.items=Array.Empty<BattleItem>();removed.Tick(5);Check(removedLaunches==0&&removed.ItemProcHitCount==0,"unequip cancels waiting attack");
 var killed=Model(i,150);int deadLaunches=0;killed.ItemProcVisual+=v=>{if(v.started)deadLaunches++;};killed.Skill();killed.Attack();killed.Tick(5);Check(killed.Result==BattleResult.Victory&&deadLaunches==0&&killed.ItemProcHitCount==0,"victory cancels waiting attack");
 }
 Debug.Log("ITEM_DELAY_OK nine items: deferred VFX/contact, reserved cooldown, full hit count, unequip and victory cancellation");}
 }
}
