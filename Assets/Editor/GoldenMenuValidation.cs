using System;using System.IO;using System.Linq;using System.Reflection;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.UI;
namespace DragonTower.Editor {
 [InitializeOnLoad]public static class GoldenMenuValidation {
 const string Flag="GoldenMenuQA",Out="C:/Users/PC/Documents/Codex/GoldenUI-20261011/";static CollectionFlow flow;static int stage;static double next;
 static GoldenMenuValidation(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
 public static void Run(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Test.GoldenUI");SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
 static void Check(bool b,string m){if(!b)throw new Exception(m);}
 static void Capture(string name){typeof(AugmentPresentationValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{Out+name+".png"});}
 static Button Button(string name)=>UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==name&&b.gameObject.activeInHierarchy);
 static void Tick(){try{if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;if(flow==null){flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow?.Session==null){flow=null;return;}if(flow.Session.Selected==null)flow.Session.Hatch(0);flow.ShowLobby();Screen.SetResolution(480,850,false);next=EditorApplication.timeSinceStartup+1;return;}
 if(stage==0){Capture("lobby-preserved");Check(!flow.TowerButton.GetComponentsInChildren<CozyMenuSurface>().Any(x=>x.isActiveAndEnabled),"lobby excluded");flow.ShowStatus();}
 if(stage==1){Capture("dragon-status");Check(UnityEngine.Object.FindObjectsByType<CozyMenuSurface>(FindObjectsSortMode.None).Any(x=>x.Surface?.sprite?.name=="panel"),"shared lobby panel");Button("이름 변경").onClick.Invoke();}
 if(stage==2){Capture("rename");flow.ShowCodex();}
 if(stage==3){Capture("codex");Button("▶").onClick.Invoke();}
 if(stage==4){Capture("codex-page2");flow.ShowLobby();flow.ShowDragonSelect();}
 if(stage==5){Capture("dragon-select");flow.ShowLobby();flow.TowerButton.onClick.Invoke();flow.ShowLobby();Check(flow.Session.HasSavedRun,"resume retained");Check(!OnlineFeatures.Enabled,"offline retained");Debug.Log("GOLDEN_PHASE_AB_OK shared sprites lobby isolation status rename codex paging selection resume offline");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);return;}
 stage++;next=EditorApplication.timeSinceStartup+.6;
 }catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}}
 }
}
