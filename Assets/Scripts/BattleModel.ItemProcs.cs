using System;
using System.Collections.Generic;
using System.Linq;
namespace DragonTower {
 public sealed partial class BattleModel {
  sealed class ItemProcState {public string id;public int slot;public ItemAutoTrigger rule;public double ready;public long lastEvent=-1;public readonly HashSet<long> casts=new HashSet<long>();}
  struct OriginalItemEvent {public ItemTrigger trigger;public CombatEventSource source;public long serial,cast;}
  sealed class PendingItemHit {public string id;public double due;public int index;public long serial;public ItemAutoTrigger rule;}
  readonly Dictionary<string,ItemProcState> itemProcStates=new Dictionary<string,ItemProcState>();
  readonly Queue<OriginalItemEvent> itemEvents=new Queue<OriginalItemEvent>();
  readonly List<PendingItemHit> itemLaunches=new List<PendingItemHit>();
  readonly List<PendingItemHit> itemHits=new List<PendingItemHit>();
  readonly HashSet<string> activeItemIds=new HashSet<string>();readonly List<string> removedItemIds=new List<string>();
  long itemEventSerial,itemCastSerial,itemHitSerial;bool processingItems;
  public int ItemProcCount {get;private set;}public int ItemProcHitCount {get;private set;}
  public event Action<ItemProcVisual> ItemProcVisual;
  public double ItemCooldownRemaining(string id)=>id!=null&&itemProcStates.TryGetValue(id,out var s)?Math.Max(0,s.ready-Time):0;
  void SyncItemProcs(){
   var active=activeItemIds;active.Clear();int slot=0;
   foreach(var item in Dragon.items??Array.Empty<BattleItem>()){
    int at=slot++;if(at>=3)break;if(item==null||string.IsNullOrEmpty(item.id)||item.autoTrigger==null||!item.autoTrigger.enabled||!active.Add(item.id))continue;
    if(!itemProcStates.TryGetValue(item.id,out var s)){s=new ItemProcState{id=item.id,rule=item.autoTrigger.Copy()};itemProcStates.Add(item.id,s);}s.slot=at;
   }
   removedItemIds.Clear();foreach(var id in itemProcStates.Keys)if(!active.Contains(id))removedItemIds.Add(id);foreach(var id in removedItemIds)itemProcStates.Remove(id);
   itemLaunches.RemoveAll(x=>!active.Contains(x.id));
   itemHits.RemoveAll(x=>!active.Contains(x.id));
  }
  public ItemCooldownSave[] CaptureItemCooldowns(){SyncItemProcs();return itemProcStates.Values.OrderBy(x=>x.slot).Select(x=>new ItemCooldownSave{itemId=x.id,slot=x.slot,remaining=Math.Max(0,x.ready-Time)}).ToArray();}
  public void RestoreItemCooldowns(ItemCooldownSave[] saved){SyncItemProcs();foreach(var value in saved??Array.Empty<ItemCooldownSave>())if(value!=null&&value.itemId!=null&&itemProcStates.TryGetValue(value.itemId,out var s)&&!double.IsNaN(value.remaining)&&!double.IsInfinity(value.remaining))s.ready=Time+Math.Max(0,value.remaining);}
  void QueueItemEvent(ItemTrigger trigger,CombatEventSource source,long cast=0){
   if(processingItems||source==CombatEventSource.ItemProc||source==CombatEventSource.StatusDamage)return;
   itemEvents.Enqueue(new OriginalItemEvent{trigger=trigger,source=source,serial=++itemEventSerial,cast=cast});
  }
  void RecordOriginalItemHit(ItemTrigger trigger,CombatEventSource source,int before,long cast=0){
   if(before<=0)return;
   if(EnemyHP+enemyShieldHP<before)QueueItemEvent(trigger,source,cast);
   if(EnemyHP==0&&before>0)QueueItemEvent(ItemTrigger.EnemyKilled,source,cast);
  }
  void ProcessItemProcs(){
   if(processingItems)return;processingItems=true;
   try{
    SyncItemProcs();
    while(itemEvents.Count>0){var e=itemEvents.Dequeue();
     foreach(var s in itemProcStates.Values.OrderBy(x=>x.slot)){
      if(s.rule.effect==ItemProcEffect.IncomingReduction||s.rule.trigger!=e.trigger||s.lastEvent==e.serial||Time+1e-8<s.ready||PlayerHP<=0)continue;
      if(Result!=BattleResult.Fighting&&!(Result==BattleResult.Victory&&e.trigger==ItemTrigger.EnemyKilled))continue;
      if(e.trigger==ItemTrigger.SkillHit&&!s.rule.eachSkillHit&&e.cast>0&&s.casts.Contains(e.cast))continue;
      s.lastEvent=e.serial;if(e.trigger==ItemTrigger.SkillHit&&!s.rule.eachSkillHit)s.casts.Add(e.cast);
      if(s.rule.belowHealthPercent>0&&PlayerHP*100.0/Dragon.maxHP>=s.rule.belowHealthPercent)continue;
      if(!Roll(s.rule.chancePercent))continue;
      s.ready=Time+Math.Max(0,s.rule.cooldown);ItemProcCount++;
      if(s.rule.effect!=ItemProcEffect.Attack){
       ItemProcVisual?.Invoke(new ItemProcVisual{itemId=s.id,slot=s.slot,started=true,effect=s.rule});
       if(s.rule.effect==ItemProcEffect.Heal||s.rule.effect==ItemProcEffect.HealAndWard)HealPlayer(Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*s.rule.healthPercent/100)));
       if(s.rule.effect==ItemProcEffect.Ward||s.rule.effect==ItemProcEffect.HealAndWard)AddWard("proc:"+s.id,Math.Max(1,(int)Math.Ceiling(Dragon.maxHP*s.rule.wardPercent/100)),s.rule.buffDuration);
       if(s.rule.effect==ItemProcEffect.SkillRecharge)SkillReady=Math.Max(Time,SkillReady-s.rule.rechargeSeconds);
       continue;
      }
      // A kill event can start an effect/cooldown, but never damages an already dead target.
      if(Result!=BattleResult.Fighting)continue;
      itemLaunches.Add(new PendingItemHit{id=s.id,rule=s.rule.Copy(),serial=++itemHitSerial,due=Time+Math.Max(0,s.rule.activationDelay)});
     }
    }
    itemLaunches.Sort((a,b)=>{int time=a.due.CompareTo(b.due);return time!=0?time:a.serial.CompareTo(b.serial);});
    while(itemLaunches.Count>0&&itemLaunches[0].due<=Time+1e-8&&Result==BattleResult.Fighting){
     var launch=itemLaunches[0];itemLaunches.RemoveAt(0);if(!itemProcStates.TryGetValue(launch.id,out var state))continue;
     ItemProcVisual?.Invoke(new ItemProcVisual{itemId=launch.id,slot=state.slot,started=true,effect=launch.rule});
     for(int i=0;i<Math.Max(1,launch.rule.hitCount);i++)itemHits.Add(new PendingItemHit{id=launch.id,rule=launch.rule,index=i,serial=++itemHitSerial,due=launch.due+Math.Max(0,launch.rule.initialDelay)+i*Math.Max(.03,launch.rule.hitInterval)});
    }
    itemHits.Sort((a,b)=>{int time=a.due.CompareTo(b.due);return time!=0?time:a.serial.CompareTo(b.serial);});
    while(itemHits.Count>0&&itemHits[0].due<=Time+1e-8&&Result==BattleResult.Fighting){
     var hit=itemHits[0];itemHits.RemoveAt(0);if(!itemProcStates.TryGetValue(hit.id,out var state))continue;
     int basis=hit.rule.damageBasis==ItemDamageBasis.BasicAttack?Dragon.attackDamage:hit.rule.damageBasis==ItemDamageBasis.Skill?Dragon.skill.damage:hit.rule.damage;
     // basis is a pre-matchup stat, not the original hit's already scaled damage.
     // Extra attacks retain the existing no-critical/no-basic-or-skill-augment path.
     int damage=basis>0&&hit.rule.damagePercent>0?ElementalExtra(basis,hit.rule.damagePercent,hit.rule.DamageElement):0;ItemProcHitCount++;
     if(Result==BattleResult.Fighting&&hit.rule.status!=CombatStatusEffect.None&&Roll(hit.rule.statusChancePercent))ApplyItemProcStatus(hit.rule);
     ItemProcVisual?.Invoke(new ItemProcVisual{itemId=hit.id,slot=state.slot,damage=damage,hitIndex=hit.index,effect=hit.rule});
    }
    if(Result!=BattleResult.Fighting){itemHits.Clear();itemLaunches.Clear();}
   }finally{processingItems=false;}
  }
  // Applies only to real, positive incoming damage after existing shields/reductions, before HP loss.
  int ApplyItemDefense(int damage){
   if(damage<=0||PlayerHP<=0||Result!=BattleResult.Fighting||processingItems)return damage;
   SyncItemProcs();
   foreach(var s in itemProcStates.Values.OrderBy(x=>x.slot)){
    if(s.rule.effect!=ItemProcEffect.IncomingReduction||s.rule.trigger!=ItemTrigger.DamageTaken||Time+1e-8<s.ready||!Roll(s.rule.chancePercent))continue;
    int next=(int)Math.Round(damage*(1-Math.Max(0,Math.Min(100,s.rule.reductionPercent))/100),MidpointRounding.AwayFromZero);
    s.ready=Time+Math.Max(0,s.rule.cooldown);ItemProcCount++;
    ItemProcVisual?.Invoke(new ItemProcVisual{itemId=s.id,slot=s.slot,started=true,damage=damage-next,effect=s.rule});damage=next;
   }
   return damage;
  }
  void ApplyItemProcStatus(ItemAutoTrigger rule){switch(rule.status){
   case CombatStatusEffect.Burn:ApplyBurn(rule.statusDuration,Math.Max(1,(int)Math.Round(rule.statusPower)));break;
   case CombatStatusEffect.Slow:ApplySlow(rule.statusDuration,rule.statusPower);break;
   case CombatStatusEffect.Paralyze:ApplyParalyze(rule.statusDuration);break;
   case CombatStatusEffect.Stun:ApplyStun(rule.statusDuration);break;
   case CombatStatusEffect.Poison:ApplyPoison(Math.Max(1,(int)Math.Round(rule.statusPower)));break;
  }}
 }
}
