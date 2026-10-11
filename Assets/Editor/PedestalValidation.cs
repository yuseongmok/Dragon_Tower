using System;using System.Reflection;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
[InitializeOnLoad]public static class PedestalValidation {
 const string Flag="PedestalQA",Out="C:/Users/PC/Documents/Codex/DungeonEnvironment-20261011/";static CollectionFlow flow;static TowerRun run;static int stage,owned;static double next;
 static PedestalValidation(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
 public static void Run(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Test.Pedestals");SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
 static object Call(string n,params object[] a)=>typeof(CollectionFlow).GetMethod(n,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(flow,a);
 static void Check(bool b,string m){if(!b)throw new Exception(m);}
 static void Capture(string n)=>typeof(AugmentPresentationValidation).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Out+n+".png"});
 static void Show(TowerRoomKind kind,string method){flow.ShowLobby();var d=flow.Session.Selected;run=new TowerRun(d.maxHP,80,0,d.elementType,d.alternateSkillElement,d.StableId,d);run.AttachInstance(flow.Session.Profile.selectedInstanceId);var s=run.Capture();s.choices=new[]{kind,TowerRoomKind.Monster};run=TowerRun.Restore(s,ContentDatabase.Load());run.BeginFloorRoom(0,3291);run.SyncHealth(1,false);typeof(CollectionFlow).GetField("towerRun",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,run);Call(method);}
 static void Tick(){try{if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
 if(flow==null){flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow?.Session==null){flow=null;return;}if(flow.Session.Selected==null)flow.Session.Hatch(0);Screen.SetResolution(480,850,false);Show(TowerRoomKind.Gold,"ShowGoldRoom");next=EditorApplication.timeSinceStartup+.5;return;}
 if(stage==0){Capture("gold-pedestal");flow.RoomButton.onClick.Invoke();flow.RoomButton.onClick.Invoke();}
 if(stage==1){Check(run.Gold==50&&run.RewardClaimed,"gold exactly once");Call("SaveProgress");Check(flow.Session.Profile.activeRun.Gold==50,"gold saved");Show(TowerRoomKind.Recovery,"ShowRecoveryRoom");}
 if(stage==2){Capture("recovery-pedestal");flow.RoomButton.onClick.Invoke();flow.RoomButton.onClick.Invoke();}
 if(stage==3){Check(run.CurrentHP==run.MaxHP&&run.RewardClaimed,"heal once and complete");Show(TowerRoomKind.Nest,"ShowNestRoom");owned=flow.Session.Profile.dragons.Count;}
 if(stage==4){Capture("nest-pedestal");flow.RoomButton.onClick.Invoke();flow.RoomButton.onClick.Invoke();Check(flow.Session.Profile.dragons.Count==owned+1&&run.RewardClaimed,"nest saved once before animation");}
 if(stage==5){Check(flow.ScreenName=="둥지 부화 성공!","nest reveal");Capture("nest-result");Debug.Log("PEDESTAL_OK gold50 once saved fullheal nest one persisted reveal");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);return;}
 next=EditorApplication.timeSinceStartup+(stage%2==0?1.35:.3);stage++;
 }catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}}
}}
