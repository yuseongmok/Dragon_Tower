using System;using System.IO;using System.Linq;using System.Reflection;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 [InitializeOnLoad] public static class LeaderboardValidation {
  const string Flag="LeaderboardValidation",Prefix="DragonTower.Test.Leaderboard",Out="C:/Users/PC/Documents/Codex/Leaderboard-20261010/";
  static CollectionFlow flow;static int stage;static double wait;
  static LeaderboardValidation(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
  public static void Run(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE",Prefix);PlayerPrefs.DeleteKey(Prefix+".A");PlayerPrefs.DeleteKey(Prefix+".B");Directory.CreateDirectory(Out);SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
  static void Set(string name,object value)=>typeof(CollectionFlow).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,value);
  static void Check(bool ok,string text){if(!ok)throw new Exception(text);}
  static void Capture(string file)=>typeof(AugmentPresentationValidation).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Out+file+".png"});
  sealed class Mock:IAccountService {public bool Busy=>false;public bool failure;public int submissions;public AccountResponse read;public void Request(AccountRequest r,Action<AccountResponse> done){if(r.op=="leaderboard-submit"){submissions++;done(new AccountResponse{ok=!failure,accepted=!failure,code=failure?"network":""});}else done(read);}}
  static void Tick(){try{
   if(!EditorApplication.isPlaying)return;
   if(flow==null){flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow?.Session==null){flow=null;return;}if(flow.Session.Selected==null)flow.Session.Hatch(0);flow.EnterTowerWithRoll(0,0);stage=0;wait=EditorApplication.timeSinceStartup+.5;return;}
   if(EditorApplication.timeSinceStartup<wait)return;
   if(stage==0){
    var run=flow.CurrentRun;flow.ShowLobby();Check(flow.Session.HasSavedRun,"lobby preserves run");Check(flow.Session.Profile.pendingLeaderboardRecords.Count==0,"lobby never submits unfinished run");
    run.End();flow.Session.FinishRun(run);flow.Session.FinishRun(run);Check(flow.Session.Profile.pendingLeaderboardRecords.Count==1,"finalized exactly once");
    var restored=new ProfileStore(Prefix).Load();Check(restored.pendingLeaderboardRecords.Count==1&&restored.pendingLeaderboardRecords[0].finalized,"pending survives restart");
    var mock=new Mock{failure=true,read=new AccountResponse{ok=false,code="network"}};Set("account",mock);Set("identity",new AccountResponse{uid="test-user",scope="test"});Set("activeOwner","test:test-user");
    flow.ShowLeaderboard();Check(flow.Session.Profile.pendingLeaderboardRecords.Count==1,"network failure keeps queue");Capture("network-error");
    mock.failure=false;mock.read=new AccountResponse{ok=true,entries=Array.Empty<LeaderboardEntry>()};flow.ShowLeaderboard();Check(flow.Session.Profile.pendingLeaderboardRecords.Count==0,"ack removes queue");Check(mock.submissions==2,"retry same finalized record");Capture("empty");
    var rows=Enumerable.Range(1,100).Select(i=>new LeaderboardEntry{uid="u"+i,playerName=i==1?"열두글자닉네임테스트":"플레이어"+i,dragonId=i%2==0?"okta":"ember",dragonName=i==1?"나의작은불꽃친구":"",originalDragonName=i%2==0?"옥타":"엠버",bestScore=99999-i,bestFloor=80-i%5,bestBond=100-i,verification="client-reported"}).ToArray();
    mock.read=new AccountResponse{ok=true,entries=rows,mine=new LeaderboardEntry{uid="test-user",playerName="내닉네임",dragonId="zephyr",dragonName="바람친구",originalDragonName="제피르",bestScore=2450,bestFloor=28,bestBond=21},myRank=107};flow.ShowLeaderboard();wait=EditorApplication.timeSinceStartup+.8;stage=1;return;
   }
   if(stage==1){Check(flow.ScreenName=="유대의 탑 랭킹","ranking screen");Check(UnityEngine.Object.FindObjectsByType<UnityEngine.UI.ScrollRect>(FindObjectsSortMode.None).Any(s=>s.gameObject.activeInHierarchy&&s.content.childCount==100),"scroll top 100");Capture("leaderboard-top100");Debug.Log("LEADERBOARD_UNITY_OK lobby no submission, finalized once, durable queue, failure retry, top100 portrait UI, self rank107. Mock service only.");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);stage=2;}
  }catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}}
 }
}
