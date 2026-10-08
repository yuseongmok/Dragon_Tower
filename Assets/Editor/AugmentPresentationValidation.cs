using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor {
[InitializeOnLoad] public static class AugmentPresentationValidation {
 const string Flag="AugmentPresentationTest";
 static double start;static int stage;static int frame;static double nextCapture;static CollectionFlow flow;
 static string Out=>"C:/Users/PC/Documents/Codex/AugmentImplementation-20261008";
 static AugmentPresentationValidation(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
 public static void Run(){AugmentReworkValidation.Verify();Directory.CreateDirectory(Out+"/frames");SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
 static object Call(string name,params object[] args)=>typeof(CollectionFlow).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(flow,args);
 static void Capture(string path){
 var view=UnityEngine.Object.FindFirstObjectByType<BattleView>();var canvas=view.GetComponent<Canvas>();
 var go=new GameObject("Capture camera");var cam=go.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=425;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;cam.transform.position=new Vector3(0,0,-100);
 var rt=new RenderTexture(480,850,24);cam.targetTexture=rt;
 canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=10;view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;
 Canvas.ForceUpdateCanvases();cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(480,850,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,480,850),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());RenderTexture.active=old;
 canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;cam.targetTexture=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);
 }
 static void Tick(){try{
 if(!EditorApplication.isPlaying)return;
 if(flow==null){flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow==null||flow.Session==null)return;Screen.SetResolution(480,850,false);start=EditorApplication.timeSinceStartup;stage=0;return;}
 double t=EditorApplication.timeSinceStartup-start;
 if(stage>0&&stage<6&&t>=nextCapture){Capture(Out+"/frames/"+(frame++).ToString("D3")+".png");nextCapture=t+.08;}
 if(stage==0&&t>2){var db=ContentDatabase.Load();var d=db.dragons[0];if(flow.Session.Selected==null)flow.Session.Hatch(0);var run=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);typeof(CollectionFlow).GetField("towerRun",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,run);Call("ShowAugmentChoices",false);start=EditorApplication.timeSinceStartup;stage=1;}
 else if(stage==1&&t>.12){if(flow.RewardsReady)throw new Exception("premature input");Capture(Out+"/01-before.png");stage=2;}
 else if(stage==2&&t>.55){Capture(Out+"/02-reveal.png");stage=3;}
 else if(stage==3&&t>1.4){if(!flow.RewardsReady)throw new Exception("reveal never unlocks");Capture(Out+"/03-choices.png");stage=4;}
 else if(stage==4&&t>2.1){flow.ChoiceButtons[0].onClick.Invoke();stage=5;}
 else if(stage==5&&t>2.6){Capture(Out+"/04-details.png");stage=6;}
 else if(stage==6&&t>3.4){var confirm=GameObject.Find("이 선택으로 성장");if(confirm==null)throw new Exception("missing confirmation");confirm.GetComponent<Button>().onClick.Invoke();if(flow.ScreenName=="증강방")throw new Exception("selection did not complete");Debug.Log("AUGMENT_PRESENTATION_OK delay, stagger, unlock, detail, confirmation tested");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);}
 }catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}}
}}


