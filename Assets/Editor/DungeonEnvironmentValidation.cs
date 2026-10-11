using System;using System.Linq;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
[InitializeOnLoad]public static class DungeonEnvironmentValidation {
 const string Flag="DungeonEnvironmentQA",Out="C:/Users/PC/Documents/Codex/DungeonEnvironment-20261011/";static CollectionFlow flow;static ContentDatabase db;static BattleSkin skin;static int stage;static double next;
 static DungeonEnvironmentValidation(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
 public static void Run(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Test.DungeonEnvironment");SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
 static void Check(bool b,string m){if(!b)throw new Exception(m);}
 static object Call(string n,params object[] a)=>typeof(CollectionFlow).GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(flow,a);
 static void Capture(string n)=>typeof(AugmentPresentationValidation).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Out+n+".png"});
 static void Set(int floor,TowerRoomKind kind,bool chosen){flow.ShowLobby();var d=flow.Session.Selected;var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);r.AttachInstance(flow.Session.Profile.selectedInstanceId);var s=r.Capture();s.Floor=floor;s.choices=kind==TowerRoomKind.Boss?new[]{kind}:new[]{kind,TowerRoomKind.Gold};r=TowerRun.Restore(s,db);if(chosen)r.BeginFloorRoom(0,18421);typeof(CollectionFlow).GetField("towerRun",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,r);}
 static void Scenery(EnvironmentScreen screen){var im=UnityEngine.Object.FindObjectsByType<Image>(FindObjectsSortMode.None).First(x=>x.name=="Room scenery"&&x.gameObject.activeInHierarchy);Check(im.sprite==skin.BackgroundForScreen(flow.CurrentRun.Floor,screen),"room routing "+screen);}
 static void Tick(){try{if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
 if(flow==null){flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow?.Session==null){flow=null;return;}if(flow.Session.Selected==null)flow.Session.Hatch(0);db=ContentDatabase.Load();skin=Resources.Load<BattleSkin>("PixelBattleSkin");Screen.SetResolution(480,850,false);
 foreach(EnvironmentScreen v in Enum.GetValues(typeof(EnvironmentScreen)))foreach(int f in new[]{1,10})Check(skin.BackgroundForScreen(f,v)!=null,"missing "+v);
 Check(skin.BackgroundForScreen(10,EnvironmentScreen.BossBattle)!=skin.BackgroundForFloor(10),"boss distinct");
 for(int f=11;f<=80;f++){var expected=f<=20?skin.floodedSewerBackground:f<=30?skin.forgeDepthsBackground:skin.upperTowerBackgrounds[(f-31)/10];Check(skin.BackgroundForFloor(f)==expected&&skin.BackgroundForScreen(f,EnvironmentScreen.BossBattle)==expected,"upper battle preserved");Check(skin.BackgroundForScreen(f,EnvironmentScreen.Item)==Resources.Load<Sprite>("UI/treasure-chamber"),"upper item preserved");Check(skin.BackgroundForScreen(f,EnvironmentScreen.Stairs)==Resources.Load<Sprite>("UI/tower-stairways"),"upper stairs preserved");Check(skin.BackgroundForScreen(f,EnvironmentScreen.BossEntrance)==Resources.Load<Sprite>("UI/boss-stairway"),"upper entrance preserved");}
 Set(1,TowerRoomKind.Monster,true);Call("StartCurrentBattle");next=EditorApplication.timeSinceStartup+.3;return;}
 if(stage==0){Capture("battle");Set(6,TowerRoomKind.Item,true);Call("ShowTreasureChamber");Scenery(EnvironmentScreen.Item);}
 if(stage==1){Capture("item");Call("SaveProgress");flow.ShowLobby();flow.ResumeRun();Scenery(EnvironmentScreen.Item);Check(flow.CurrentRun.Floor==6,"resume floor");Capture("item-resumed");Set(9,TowerRoomKind.Item,false);Call("ShowTowerChoices");Scenery(EnvironmentScreen.Stairs);}
 if(stage==2){Capture("stairs");Check(flow.RoomButton.GetComponent<RectTransform>().anchoredPosition==new Vector2(-103,-225),"door position");Set(10,TowerRoomKind.Boss,false);Call("ShowTowerChoices");Scenery(EnvironmentScreen.BossEntrance);}
 if(stage==3){Capture("boss-entrance");Set(10,TowerRoomKind.Boss,true);Call("StartCurrentBattle");}
 if(stage==4){Capture("boss-battle");Debug.Log("DUNGEON_ENVIRONMENT_OK five routes distinct boss floors11to80 preserved resume and fixed door geometry");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);return;}
 stage++;next=EditorApplication.timeSinceStartup+.3;
 }catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}}
}}
