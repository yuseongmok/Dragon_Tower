using System;using UnityEngine;
namespace DragonTower {
 public static class OnlineFeatures {public static bool Enabled=>false;}
 public static class OfflineRecords {
  public static bool Exists(string prefix)=>PlayerPrefs.HasKey(prefix+".A")||PlayerPrefs.HasKey(prefix+".B");
  public static bool Meaningful(PlayerProfile p)=>p.dragons.Count>0||p.hasActiveRun||p.lastRun!=null||p.bestScore>0||!string.IsNullOrEmpty(p.playerName);
  public static string CopyAccount(string prefix,string owner){
   string source=prefix+".Account."+LocalTrainerSave.Key(owner),destination=prefix+".Offline."+LocalTrainerSave.Key(owner);
   if(!Exists(source))return null;var profile=new ProfileStore(source).Load();
   if(Exists(destination)){new ProfileStore(destination).Load();return destination;}
   var copy=ProfileStore.Clone(profile);
   if(string.IsNullOrEmpty(copy.playerName)){var cache=new LocalTrainerSave(prefix).Load(owner);if(cache?.profile!=null)copy.playerName=cache.profile.displayName;}
   new ProfileStore(destination).Save(copy);return destination;
  }
  public static void ApplyFinished(PlayerProfile p,RunProgressRecord r){
   if(r==null||!r.finalized)return;
   if(r.score>p.bestScore||(r.score==p.bestScore&&(p.bestScoreRun==null||string.IsNullOrEmpty(p.bestScoreRun.runId))))p.bestScoreRun=JsonUtility.FromJson<RunProgressRecord>(JsonUtility.ToJson(r));
   p.bestScore=Math.Max(p.bestScore,r.score);p.bestFloor=Math.Max(p.bestFloor,r.maxFloor);p.bestBond=Math.Max(p.bestBond,r.maxBond);
  }
 }
 public sealed partial class CollectionSession {
  public void PrepareOfflineRecords(){
   if(Profile.localRecordsVersion>=1)return;var next=ProfileStore.Clone(Profile);
   // Keep legacy maxima; never invent a missing matching partner record.
   next.bestFloor=Math.Max(next.bestFloor,next.achievementProgress?.maxFloor??0);
   OfflineRecords.ApplyFinished(next,next.lastRun?.progress);
   foreach(var r in next.pendingLeaderboardRecords??new System.Collections.Generic.List<RunProgressRecord>())OfflineRecords.ApplyFinished(next,r);
   next.localRecordsVersion=1;store.Save(next);Profile=next;
  }
 }
}
