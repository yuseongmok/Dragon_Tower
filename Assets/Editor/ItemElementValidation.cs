using System;using System.Linq;using UnityEngine;using UnityEditor;
namespace DragonTower.Editor {
 public static class ItemElementValidation {
  static int checks;static void Check(bool ok,string name){if(!ok)throw new Exception("ITEM_ELEMENT_FAIL "+name);checks++;}
  static BattleModel Model(ElementType item,ElementType enemy,CombatStatusEffect status=CombatStatusEffect.None,params BattleAugment[] augments){
   var s=new BattleStats{speciesId="fixture",elementType=ElementType.Fire,maxHP=10000,attackDamage=100,attackCooldown=.1f,criticalChance=1,criticalDamage=3,skill=new SkillStats{damage=80,elementType=ElementType.Ice,cooldown=1},augments=augments,items=new[]{new BattleItem{id="fixture-item",autoTrigger=new ItemAutoTrigger{enabled=true,trigger=ItemTrigger.BasicHit,damage=100,element=item,cooldown=10,status=status,statusChancePercent=100,statusDuration=3,statusPower=30}}}};
   return new BattleModel(s,new BattleEnemyStats{maxHP=100000,damage=1,interval=100,elementType=enemy},s.maxHP,()=>.5);
  }
  public static void Run(){try{Verify();ItemProcValidation.Verify();InventorySplitValidation.Verify();Debug.Log("ITEM_ELEMENT_OK checks="+checks+" plus proc/inventory regression");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  static void Verify(){
   foreach(ElementType element in Enum.GetValues(typeof(ElementType)))foreach(ElementType target in Enum.GetValues(typeof(ElementType))){
    var b=Model(element,target);int proc=0;CombatEventSource source=CombatEventSource.BasicAttack;b.ItemProcVisual+=x=>{if(!x.started){proc=x.damage;source=x.Source;}};b.Attack();
    Check(proc==ElementRules.Damage(100,element,target)&&source==CombatEventSource.ItemProc,"single matchup "+element+" -> "+target);
    Check(b.Dragon.elementType==ElementType.Fire&&b.Dragon.skill.elementType==ElementType.Ice,"original elements intact");
    Check(!b.EnemyBurning&&!b.EnemySlowed&&!b.EnemyParalyzed&&!b.EnemyPoisoned,"no implied status");
   }
   Check(ElementRules.Damage(100,ElementType.Lightning,ElementType.Water)==150,"Electric uses existing Lightning advantage");
   Check(ElementRules.Damage(100,ElementType.Water,ElementType.Lightning)==100,"reverse direction unchanged: game has no penalty");
   foreach(var pair in new[]{(ElementType.Fire,CombatStatusEffect.Burn),(ElementType.Ice,CombatStatusEffect.Slow),(ElementType.Lightning,CombatStatusEffect.Paralyze),(ElementType.Dark,CombatStatusEffect.Poison)}){var b=Model(pair.Item1,ElementType.Neutral,pair.Item2);b.Attack();Check(pair.Item2==CombatStatusEffect.Burn?b.EnemyBurning:pair.Item2==CombatStatusEffect.Slow?b.EnemySlowed:pair.Item2==CombatStatusEffect.Paralyze?b.EnemyParalyzed:b.EnemyPoisoned,"explicit existing status "+pair.Item2);b.Tick(2);Check(b.ItemProcCount==1,"status cannot recurse");}
   var boosted=Model(ElementType.Fire,ElementType.Ice,CombatStatusEffect.None,new BattleAugment{mechanic=AugmentMechanic.ExploitWeakness,primaryValue=100});int result=0;boosted.ItemProcVisual+=x=>{if(!x.started)result=x.damage;};boosted.Attack();Check(result==150,"basic/skill advantage augment and crit not reapplied to item");boosted.Tick(.2f);boosted.Attack();Check(boosted.ItemProcCount==1,"cooldown preserved");
   var db=ContentDatabase.Load();var guard=db.items.First(x=>x.StableId=="item_guardian_brooch");Check(guard.DamageElement==ElementType.Neutral&&guard.autoTrigger.element==ElementType.Neutral,"guard neutral");Check(db.items.Where(x=>x.kind==ItemKind.Consumable).All(x=>x.DamageElement==ElementType.Neutral),"non-proc consumables no offensive element");
   var ids=new[]{"item_infighting_glove","item_mana_prism","item_yata_mirror","item_meteor_fragment"};var expected=new[]{ElementType.Fire,ElementType.Ice,ElementType.Wind,ElementType.Fire};for(int i=0;i<ids.Length;i++)Check(db.items.First(x=>x.StableId==ids[i]).DamageElement==expected[i],"representative element "+ids[i]);
   var d=db.dragons.First(x=>x.elementType==ElementType.Fire);var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);foreach(var id in ids.Take(3))r.AddItem(db.items.First(x=>x.StableId==id));
   var tape=new BattleResumeData{initialRunJson=JsonUtility.ToJson(r.Capture(false)),seed=12,enemy=new BattleEnemyStats{maxHP=100000,damage=1,interval=100,elementType=ElementType.Ice}};var rng=new System.Random(12);var live=new BattleModel(r.BuildBattleStats(d.Snapshot()),tape.enemy,r.CurrentHP,()=>rng.NextDouble());live.Attack();live.Tick(.1f);tape.inputs.Add(new BattleInputRecord{action=1});tape.inputs.Add(new BattleInputRecord{action=0,delta=.1f});r.ItemCooldowns=live.CaptureItemCooldowns();r.BattleResume=tape;
   var loaded=TowerRun.Restore(JsonUtility.FromJson<TowerRunSave>(JsonUtility.ToJson(r.Capture())),db);Check(loaded.ItemAt(0).DamageElement==ElementType.Fire&&loaded.ItemAt(1).DamageElement==ElementType.Ice,"save resolves item elements by stable ID");var replay=BattleReplay.Restore(loaded.BattleResume,db,"test");live.Tick(.5f);replay.Tick(.5f);Check(live.EnemyHP==replay.EnemyHP&&live.ItemProcHitCount==replay.ItemProcHitCount,"inflight elemental damage restored");
   Check(Math.Abs(live.ItemCooldownRemaining(ids[0])-replay.ItemCooldownRemaining(ids[0]))<.001,"cooldown restored");
   var old=ScriptableObject.CreateInstance<ItemData>();old.kind=ItemKind.Equipment;old.autoTrigger=null;Check(old.DamageElement==ElementType.Neutral,"legacy missing config neutral");UnityEngine.Object.DestroyImmediate(old);
  }
 }
}
