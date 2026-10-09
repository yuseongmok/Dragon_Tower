using System;using System.IO;using System.Linq;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
 [InitializeOnLoad] public static class CharcoalSweepValidation {
  const string Flag="CharcoalSweepValidation";const string Out="C:/Users/PC/Documents/Codex/CharcoalSweep-20261009/";static CollectionFlow flow;static double started;
  static CharcoalSweepValidation(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
  public static void Run(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Test.CharcoalSweep");Directory.CreateDirectory(Out);SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
  static object Call(string name,params object[] args)=>typeof(CollectionFlow).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(flow,args);
  static void Capture(string name){
   var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas).ToArray();var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();
   var roots=canvases.SelectMany(c=>c.GetComponentsInChildren<RectTransform>()).Where(r=>r.name=="Portrait game area"||r.name=="Portrait Battle · 480 x 850").ToArray();var scales=roots.Select(r=>r.localScale).ToArray();foreach(var r in roots)r.localScale=Vector3.one;
   var go=new GameObject("Review camera");var cam=go.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=425;cam.transform.position=new Vector3(0,0,-100);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;var rt=new RenderTexture(480,850,24);cam.targetTexture=rt;
   foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=cam;c.planeDistance=10;}Canvas.ForceUpdateCanvases();cam.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(480,850,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,480,850),0,0);texture.Apply();File.WriteAllBytes(Out+name+".png",texture.EncodeToPNG());RenderTexture.active=previous;
   for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];}for(int i=0;i<roots.Length;i++)roots[i].localScale=scales[i];UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);
  }
  static void Check(bool ok,string text){if(!ok)throw new Exception(text);}
  static void Tick(){try{if(!EditorApplication.isPlaying)return;if(flow==null){flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow?.Session==null){flow=null;return;}if(flow.Session.Selected==null)flow.Session.Hatch(0);var d=flow.Session.Selected;var run=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);run.AttachInstance(flow.Session.Profile.selectedInstanceId);var save=run.Capture();save.Phase=FloorPhase.BattleRewards;save.Experience=75;save.LastExperienceGain=75;save.LastBattleGold=10;run=TowerRun.Restore(save,ContentDatabase.Load());typeof(CollectionFlow).GetField("towerRun",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,run);Call("ShowExperienceReward",1,0);started=EditorApplication.timeSinceStartup;return;}
   if(EditorApplication.timeSinceStartup-started<3||flow.ContinueButton==null)return;
   Capture("experience");var gauge=GameObject.Find("Experience fill").GetComponent<RectTransform>();Check(Mathf.Abs(gauge.rect.width-292.5f)<1,"experience animation amount unchanged");Check(flow.ContinueButton.GetComponent<AncientButtonFeedback>()!=null,"continue styled and interactive");
   foreach(string screen in new[]{"ShowRecoveryRoom","ShowGoldRoom","ShowShopRoom","ShowNestRoom","ShowDragonSelect"}){Call(screen);Capture(screen);}
   Call("ShowRoomResult","보상을 획득했습니다.");Capture("reward-complete");
   var settings=UnityEngine.Object.FindFirstObjectByType<DragonTowerSettings>();typeof(DragonTowerSettings).GetMethod("Open",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(settings,null);Capture("settings");
   typeof(DragonTowerSettings).GetMethod("Close",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(settings,null);
   var combat=flow.CurrentRun.Capture();combat.Phase=FloorPhase.CombatPending;combat.Room=TowerRoomKind.Monster;combat.RoomChosen=true;typeof(CollectionFlow).GetField("towerRun",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,TowerRun.Restore(combat,ContentDatabase.Load()));Call("StartCurrentBattle");Capture("battle-hud");
   foreach(var art in UnityEngine.Object.FindObjectsByType<AncientStoneSurface>(FindObjectsSortMode.None))Check(!art.raycastTarget,"decoration must not block controls");
   Debug.Log("CHARCOAL_SWEEP_OK experience gauge continue recovery gold shop nest selection reward settings");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}}
 }
}
