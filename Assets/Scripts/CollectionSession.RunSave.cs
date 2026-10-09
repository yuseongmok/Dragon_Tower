using System;
using UnityEngine;
namespace DragonTower {
 public sealed partial class CollectionSession {
  public bool HasSavedRun=>Profile.hasActiveRun&&Profile.activeRun!=null;
  public void SaveRun(TowerRun run){
   if(run==null)return;
   var next=ProfileStore.Clone(Profile);var snapshot=run.Capture();var earned=UpdateProgress(next,snapshot);
   next.hasActiveRun=run.Active&&!snapshot.progress.finalized;next.activeRun=next.hasActiveRun?snapshot:null;
   CommitProgress(next,run,snapshot,earned);
  }
  public void FinishRun(TowerRun run,bool cleared=false){
   if(run.Progress.finalized)return;
   var next=ProfileStore.Clone(Profile);var snapshot=run.Capture();var earned=UpdateProgress(next,snapshot);snapshot.progress.finalized=true;
   next.lastRun=new RunResultRecord{instanceId=run.InstanceId,floor=run.Floor,level=run.Level,monsters=run.MonstersDefeated,cleared=cleared,progress=snapshot.progress};
   next.hasActiveRun=false;next.activeRun=null;CommitProgress(next,run,snapshot,earned);
  }
  public void SaveNestReward(TowerRun run,DragonData dragon){
   var next=ProfileStore.Clone(Profile);next.dragons.Add(new OwnedDragon{instanceId=Guid.NewGuid().ToString("N"),speciesId=dragon.StableId});
   var snapshot=run.Capture();var earned=UpdateProgress(next,snapshot);next.hasActiveRun=true;next.activeRun=snapshot;CommitProgress(next,run,snapshot,earned);
  }
 }
}
