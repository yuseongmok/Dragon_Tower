using System;using System.Linq;using System.IO;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
[InitializeOnLoad]public static class LobbyPolishValidation {
 const string Flag="LobbyPolishValidation",Out="C:/Users/PC/Documents/Codex/LobbyPolish-20261009/";static CollectionFlow flow;static ContentDatabase db;static int stage,frame;static double started;
 static LobbyPolishValidation(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
 public static void Run(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Test.LobbyPolish");PlayerPrefs.DeleteKey("DragonTower.Test.LobbyPolish.A");PlayerPrefs.DeleteKey("DragonTower.Test.LobbyPolish.B");Directory.CreateDirectory(Out+"frames");SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
 static void Capture(string file)=>typeof(AugmentPresentationValidation).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Out+file+".png"});
 static void Call(string method,params object[] args)=>typeof(CollectionFlow).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(flow,args);
 static void Select(DragonData d){var owned=flow.Session.Profile.dragons.FirstOrDefault(x=>x.speciesId==d.StableId)??flow.Session.RegisterHatchedDragon(d);flow.Session.Select(owned.instanceId);}
 static void Tick(){try{
 if(!EditorApplication.isPlaying)return;
 if(flow==null){flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow?.Session==null){flow=null;return;}db=ContentDatabase.Load();if(flow.Session.Selected==null)flow.Session.Hatch(0);started=EditorApplication.timeSinceStartup;return;}
 if(EditorApplication.timeSinceStartup-started<2)return;
 if(stage==0){
  foreach(var d in db.dragons){Select(d);flow.ShowLobby();var idle=UnityEngine.Object.FindObjectsByType<DragonPortraitIdle>(FindObjectsSortMode.None).First(x=>x.gameObject.activeInHierarchy);if(idle.FitScale<=0)throw new Exception("Invalid scale "+d.displayName);idle.Step(.25f);Capture("lobby-"+d.StableId);flow.ShowStatus();Canvas.ForceUpdateCanvases();foreach(var t in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Where(x=>x.gameObject.activeInHierarchy&&x.name=="Description")){if(t.preferredHeight>t.rectTransform.rect.height+2)Debug.LogWarning("DESCRIPTION_CHECK "+d.displayName+" "+t.text);}
  Call("ShowCodexEntry",d);var portraits=UnityEngine.Object.FindObjectsByType<DragonPortraitIdle>(FindObjectsSortMode.None).Where(x=>x.gameObject.activeInHierarchy).ToArray();if(portraits.Length!=3)throw new Exception("Missing evolution portrait "+d.displayName);foreach(var p in portraits){p.Step(.5f);var art=p.GetComponentInChildren<Image>();if(art==null)throw new Exception("Missing Idle image");}Capture("codex-"+d.StableId);
  }
  var titan=db.dragons.First(x=>x.displayName=="티탄");Select(titan);flow.ShowStatus();Capture("status");flow.ShowLobby();Capture("lobby");stage=1;frame=0;
 }
 if(stage==1){foreach(var p in UnityEngine.Object.FindObjectsByType<DragonPortraitIdle>(FindObjectsSortMode.None).Where(x=>x.gameObject.activeInHierarchy)){p.enabled=false;p.Step(.05f);}Capture("frames/"+(frame++).ToString("D3"));if(frame>=60){flow.ShowDragonSelect();Capture("select");flow.ShowCodex();Capture("codex");Call("ShowProgressRecords");Capture("records");Call("ShowRenameDragon");Capture("rename");Debug.Log("LOBBY_POLISH_OK 16 dragons / 48 evolution portraits; Idle, framing, lobby, status, codex, selection");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);}}
 }catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}}
}}
