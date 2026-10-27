using System;
using System.Linq;
using System.Collections.Generic;
namespace DragonTower {
 public enum RewardRerollKind { Room, Level, Chest, Drop }
 [Serializable] public sealed class RewardRerollRecord {
  public string key;public string[] candidates;
  public RewardRerollRecord Copy()=>new RewardRerollRecord{key=key,candidates=(string[])candidates.Clone()};
 }
 public sealed partial class TowerRun {
  public List<RewardRerollRecord> Rerolls {get;private set;}=new List<RewardRerollRecord>();
  public string RewardKey(RewardRerollKind kind)=>Progress.runId+":"+kind+":"+(kind==RewardRerollKind.Level?(Level/5-PendingLevelAugments+1):Floor);
  public RewardRerollRecord RerollFor(RewardRerollKind kind)=>Rerolls.Find(x=>x.key==RewardKey(kind));
  public bool CanRerollScreen(RewardRerollKind kind) {
   if(!Active||Progress.finalized||ExperiencePresentationPending||Phase==FloorPhase.CombatActive||Phase==FloorPhase.Ended)return false;
   switch(kind) {
    case RewardRerollKind.Level:return PendingLevelAugments>0&&PendingEvolutionStage==0&&string.IsNullOrEmpty(PendingDropId);
    case RewardRerollKind.Room:return PendingLevelAugments==0&&Phase==FloorPhase.RoomEvent&&Room==TowerRoomKind.Augment&&!RewardClaimed;
    case RewardRerollKind.Chest:return PendingLevelAugments==0&&Phase==FloorPhase.RoomEvent&&Room==TowerRoomKind.Item&&!RewardClaimed&&ChestOpened;
    case RewardRerollKind.Drop:return Phase==FloorPhase.BattleRewards&&!string.IsNullOrEmpty(PendingDropId);
    default:return false;
   }
  }
  internal void ApplyReroll(RewardRerollKind kind,RewardRerollRecord record) {
   Rerolls.Add(record.Copy());
   if(kind==RewardRerollKind.Chest)ChestRewardIds=(string[])record.candidates.Clone();
   if(kind==RewardRerollKind.Drop)PendingDropId=record.candidates[0];
  }
 }
 public sealed partial class CollectionSession {
  public bool TryRerollReward(TowerRun run,ContentDatabase db,RewardRerollKind kind,string expectedKey,long revision,int seed,out string message) {
   message="현재 보상에서 재추첨할 수 없습니다.";
   if(run==null||db==null||!run.CanRerollScreen(kind)||expectedKey!=run.RewardKey(kind)||revision!=Profile.bondRevision||!HasSavedRun||Profile.activeRun.progress?.runId!=run.Progress.runId)return false;
   // Check the committed receipt too: a stale in-memory screen must not spend twice.
   if(run.RerollFor(kind)!=null||(Profile.activeRun.rerolls?.Exists(x=>x.key==expectedKey)??false)){message="이미 재추첨한 보상입니다.";return false;}
   var saved=Profile.activeRun;
   if(saved.Floor!=run.Floor||saved.Phase!=run.Phase||saved.Level!=run.Level||saved.PendingLevelAugments!=run.PendingLevelAugments||saved.RewardClaimed!=run.RewardClaimed||(saved.PendingDropId??"")!=(run.PendingDropId??""))return false;
   int cost=BondEconomy.Load().rerollCost;
   if(Profile.bondPoints<cost){message="유대 포인트 "+(cost-Profile.bondPoints)+"P 부족";return false;}
   string[] ids;
   if(kind==RewardRerollKind.Chest||kind==RewardRerollKind.Drop)ids=db.PickItems(kind==RewardRerollKind.Drop?1:(run.ChestRewardIds?.Length??0),seed,run).Select(x=>x.StableId).ToArray();
   else ids=RewardGenerator.Mixed(run,db,Selected,seed,3).Select(x=>x.augment!=null?"a:"+x.augment.StableId:"s:"+x.skill.StableId).ToArray();
   if(ids.Length==0){message="재추첨할 수 있는 보상이 없습니다.";return false;}
   var record=new RewardRerollRecord{key=expectedKey,candidates=ids};
   var next=ProfileStore.Clone(Profile);var snapshot=run.Capture();
   snapshot.rerolls.Add(record.Copy());
   if(kind==RewardRerollKind.Chest)snapshot.ChestRewardIds=(string[])ids.Clone();
   if(kind==RewardRerollKind.Drop)snapshot.PendingDropId=ids[0];
   next.bondPoints-=cost;next.bondRevision++;next.activeRun=snapshot;
   // One checksummed profile envelope contains currency, candidates and the receipt.
   // No runtime mutation or animation occurs before the save succeeds.
   store.Save(next);Profile=next;run.ApplyReroll(kind,record);
   message="재추첨 완료";return true;
  }
 }
}
