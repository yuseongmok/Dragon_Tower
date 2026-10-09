using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonTower {
 public enum AchievementMetric { TotalKills, NormalKills, BossKills, Floor }
 [Serializable] public sealed class AchievementDefinition { public string id,title; public AchievementMetric metric; public int target,reward; }
 [CreateAssetMenu(menuName="Dragon Tower/Progression Rewards")]
 public sealed class ProgressionRewards:ScriptableObject {
  [Min(0)] public int normalKill=10,bossKill=200,newFloor=50;
  public AchievementDefinition[] achievements={
   new AchievementDefinition{id="first_kill",title="첫 몬스터 처치",metric=AchievementMetric.TotalKills,target=1,reward=100},
   new AchievementDefinition{id="normal_50",title="일반 몬스터 50마리 처치",metric=AchievementMetric.NormalKills,target=50,reward=300},
   new AchievementDefinition{id="first_boss",title="첫 보스 처치",metric=AchievementMetric.BossKills,target=1,reward=200},
   new AchievementDefinition{id="floor_10",title="10층 도달",metric=AchievementMetric.Floor,target=10,reward=200},
   new AchievementDefinition{id="floor_20",title="20층 도달",metric=AchievementMetric.Floor,target=20,reward=500}};
  public static ProgressionRewards Load()=>Resources.Load<ProgressionRewards>("ProgressionRewards")??CreateInstance<ProgressionRewards>();
 }
 [Serializable] public sealed class AchievementProgress {
  public int normalKills,bossKills,maxFloor;
  public List<string> unlocked=new List<string>();
  public int Value(AchievementMetric metric)=>metric==AchievementMetric.TotalKills?normalKills+bossKills:metric==AchievementMetric.NormalKills?normalKills:metric==AchievementMetric.BossKills?bossKills:maxFloor;
 }
 [Serializable] public sealed class RunProgressRecord {
  public string runId,playerName,dragonId,customDragonName,originalDragonName;
  public int score,normalKills,bossKills,maxFloor,maxBond;
  public bool finalized;
  public List<string> achievements=new List<string>();
 }
 public sealed partial class TowerRun {
  public int BossesDefeated {get;private set;}
  public RunProgressRecord Progress {get;internal set;}=new RunProgressRecord{runId=Guid.NewGuid().ToString("N")};
 }
 public sealed partial class CollectionSession {
  public event Action<AchievementDefinition> AchievementUnlocked;
  ProgressionRewards rewards;
  public ProgressionRewards Rewards=>rewards??(rewards=ProgressionRewards.Load());
  // Work on the candidate snapshot only: profile unlocks and run score commit together.
  List<AchievementDefinition> UpdateProgress(PlayerProfile next,TowerRunSave snapshot){
   var earned=new List<AchievementDefinition>();var p=snapshot.progress;
   if(p==null||string.IsNullOrEmpty(p.runId))throw new InvalidOperationException("Missing run identity");
   if(p.finalized)return earned;
   var career=next.achievementProgress??(next.achievementProgress=new AchievementProgress());
   if(career.unlocked==null)career.unlocked=new List<string>();if(p.achievements==null)p.achievements=new List<string>();
   int normals=Math.Max(0,snapshot.MonstersDefeated-snapshot.BossesDefeated);
   int dn=Math.Max(0,normals-p.normalKills),db=Math.Max(0,snapshot.BossesDefeated-p.bossKills),df=Math.Max(0,snapshot.Floor-p.maxFloor);
   p.score+=dn*Math.Max(0,Rewards.normalKill)+db*Math.Max(0,Rewards.bossKill)+df*Math.Max(0,Rewards.newFloor);
   p.normalKills=Math.Max(p.normalKills,normals);p.bossKills=Math.Max(p.bossKills,snapshot.BossesDefeated);p.maxFloor=Math.Max(p.maxFloor,snapshot.Floor);p.maxBond=Math.Max(p.maxBond,snapshot.Level);
   career.normalKills+=dn;career.bossKills+=db;career.maxFloor=Math.Max(career.maxFloor,snapshot.Floor);
   p.playerName=next.playerName??"";p.dragonId=snapshot.speciesId;
   var owned=next.dragons.Find(d=>d.instanceId==snapshot.InstanceId);p.customDragonName=owned?.customName??"";p.originalDragonName=Find(snapshot.speciesId)?.displayName??snapshot.speciesId;
   foreach(var a in Rewards.achievements??Array.Empty<AchievementDefinition>())if(a!=null&&!string.IsNullOrEmpty(a.id)&&!career.unlocked.Contains(a.id)&&career.Value(a.metric)>=a.target){career.unlocked.Add(a.id);p.achievements.Add(a.id);p.score+=Math.Max(0,a.reward);earned.Add(a);}
   return earned;
  }
  void CommitProgress(PlayerProfile next,TowerRun run,TowerRunSave snapshot,List<AchievementDefinition> earned){
   store.Save(next);Profile=next;run.Progress=JsonUtility.FromJson<RunProgressRecord>(JsonUtility.ToJson(snapshot.progress));
   foreach(var a in earned)try{AchievementUnlocked?.Invoke(a);}catch(Exception e){Debug.LogException(e);}
  }
 }
}
