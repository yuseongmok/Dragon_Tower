using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class FireStyleValidation
    {
        static int wait;
        public static void Run(){FireStyleSetup.Install();Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.FireStyle.Validation");SessionState.SetBool("FireStyleCheck",true);EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Listen(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("FireStyleCheck",false)){wait=0;EditorApplication.update+=After;}};}
        static void After(){if(++wait<12)return;EditorApplication.update-=After;SessionState.SetBool("FireStyleCheck",false);try{Test();Debug.Log("FIRE_STYLE_VALIDATION_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        static void Check(bool ok,string label){if(!ok)throw new Exception("FIRE_STYLE_FAIL "+label);Debug.Log("FIRE_STYLE_CHECK "+label);}
        static void Test()
        {
            var p=UnityEngine.Object.FindFirstObjectByType<FireStylePreview>();p.autoplay=false;p.Initialize();var c=UnityEngine.Object.FindFirstObjectByType<BattleController>();var v=c.view;var fx=p.Effect;
            var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();var rt=new RenderTexture(480,850,24);rt.Create();cam.targetTexture=rt;cam.orthographic=true;cam.orthographicSize=425;cam.transform.position=new Vector3(0,0,-10);cam.transform.rotation=Quaternion.identity;cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=-1;
            var canvas=v.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;var r=(RectTransform)canvas.transform;r.position=Vector3.zero;r.localScale=Vector3.one;r.sizeDelta=new Vector2(480,850);ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory("Validation/FireStyle");
            for(int d=0;d<2;d++){
                p.dragonIndex=d;p.evolution=2;p.Restart();var b=p.Model;int hp=b.EnemyHP,playerHP=b.PlayerHP;var position=v.playerArt.rectTransform.anchoredPosition;var hud=v.frame.anchoredPosition;int count=v.frame.GetComponentsInChildren<Transform>(true).Length;bool status=false;
                for(int f=0;f<240;f++){p.Step(1f/30);if(f==90)Check(b.EnemyHP==hp,"no attack damage from A/B sample");if(f>109&&f<185){Check(fx.BurnShown==b.EnemyBurning,"Burn reads model");status|=fx.BurnShown;}Capture(v,cam,rt,(d==0?"sol":"ember")+"-"+f.ToString("000"));}
                Check(status&&b.EnemyHP==hp-3&&b.PlayerHP==playerHP,"existing Burn tick only");Check(b.SkillReady==0&&Time.timeScale==1,"no skill cooldown or time scale change");Check(fx.ActiveCount==0&&!fx.BurnShown,"expired fire and status returned");Check(v.playerArt.rectTransform.anchoredPosition==position&&v.frame.anchoredPosition==hud,"actor and HUD unchanged");Check(count==v.frame.GetComponentsInChildren<Transform>(true).Length,"pool stable");Debug.Log("FIRE_STYLE_FORM_OK "+d+" peak="+fx.PeakImages);
            }
            Check(p.style.frames.Length==80&&p.style.frames.All(s=>s.texture.filterMode==FilterMode.Point&&!s.texture.isReadable),"Point atlas and no CPU copy");Check(v.frame.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("Fire pooled ")).All(i=>!i.raycastTarget),"no touch interception");
            p.Restart();for(int i=0;i<5;i++)Cycle(fx,p.Model);var watch=new System.Diagnostics.Stopwatch();long start=GC.GetAllocatedBytesForCurrentThread();watch.Start();for(int i=0;i<10;i++)Cycle(fx,p.Model);watch.Stop();long allocated=GC.GetAllocatedBytesForCurrentThread()-start;Debug.Log("FIRE_STYLE_PERF 2400 steps ms="+watch.Elapsed.TotalMilliseconds+" bytes="+allocated);Check(allocated==0,"warmed visual update allocation-free");
            p.Restart();p.Step(.7f);fx.enabled=false;Check(fx.ActiveCount==0,"disable clears");fx.enabled=true;p.Restart();p.Step(2);c.EndBattle();fx.Step(null,0);Check(fx.ActiveCount==0,"scene/battle exit clears");
            var dying=p.dragons[0].Snapshot();dying.maxHP=1;dying.passiveMechanic=DragonPassiveMechanic.None;var e=BattleEnemyStats.Normal();e.damage=999;e.interval=.01f;var dead=new BattleModel(dying,e,1);dead.Tick(2);fx.Play(Vector2.zero,Vector2.zero,Vector2.zero);fx.Step(dead,.2f);Check(fx.ActiveCount==0,"death clears");
            cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
        static void Cycle(FireStylePrototype fx,BattleModel model){fx.Play(new Vector2(60,-80),new Vector2(30,161),new Vector2(30,86));for(int f=0;f<240;f++)fx.Step(model,1f/60);fx.Clear();}
        static void Capture(BattleView v,Camera cam,RenderTexture rt,string name){Canvas.ForceUpdateCanvases();v.frame.localScale=Vector3.one;v.frame.anchoredPosition=Vector2.zero;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(480,850,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,480,850),0,0);t.Apply();File.WriteAllBytes("Validation/FireStyle/"+name+".png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);RenderTexture.active=old;}
        public static void BuildWeb(){var report=BuildPipeline.BuildPlayer(new[]{FireStyleSetup.ScenePath},"../FireStyle-WebGL",BuildTarget.WebGL,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Fire WebGL build failed");Debug.Log("FIRE_STYLE_WEBGL_OK");}
    }
}
