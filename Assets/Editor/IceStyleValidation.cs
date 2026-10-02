using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class IceStyleValidation
    {
        static int wait;
        [InitializeOnLoadMethod]static void Listen(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("IceStyleValidation",false)){wait=0;EditorApplication.update+=RunAfterStart;}};}
        public static void Run(){IceStyleSetup.Install();Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.IceStyle.Validation");SessionState.SetBool("IceStyleValidation",true);EditorApplication.isPlaying=true;}
        static void Check(bool value,string text){if(!value)throw new Exception("ICE_STYLE: "+text);Debug.Log("ICE_STYLE_CHECK "+text);}
        static void RunAfterStart()
        {
            if(++wait<12)return;EditorApplication.update-=RunAfterStart;SessionState.SetBool("IceStyleValidation",false);
            try{Validate();if(Application.isBatchMode)EditorApplication.Exit(0);else EditorApplication.isPlaying=false;}
            catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);else EditorApplication.isPlaying=false;}
        }
        static void Validate()
        {
            var preview=UnityEngine.Object.FindFirstObjectByType<IceStylePreview>();preview.autoplay=false;preview.Initialize();
            var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();var view=controller.view;
            var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();var rt=new RenderTexture(480,850,24);rt.Create();camera.targetTexture=rt;camera.orthographic=true;camera.orthographicSize=425;camera.transform.position=new Vector3(0,0,-10);camera.transform.rotation=Quaternion.identity;camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.cullingMask=-1;
            var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;var rect=(RectTransform)canvas.transform;rect.position=Vector3.zero;rect.localScale=Vector3.one;rect.sizeDelta=new Vector2(480,850);ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory("Validation/IceStyle");int peak=0;
            for(int d=0;d<2;d++)for(int stage=0;stage<3;stage++)
            {
                preview.dragonIndex=d;preview.evolution=stage;preview.Restart();var b=controller.CurrentBattle;int hp=b.EnemyHP,playerHP=b.PlayerHP;var time=b.Time;var skill=b.SkillReady;var position=view.playerArt.rectTransform.anchoredPosition;
                string prefix=(d==0?"aurora":"luna")+"-"+stage;
                for(int f=0;f<90;f++)
                {
                    preview.Step(1f/30);peak=Mathf.Max(peak,preview.Effect.ActiveCount);
                    if(stage==2||new[]{0,11,21,23,27,40,80,89}.Contains(f))Capture(view,camera,rt,prefix+"-"+f.ToString("000"));
                }
                Check(b.EnemyHP==hp&&b.PlayerHP==playerHP&&b.Time==time&&b.SkillReady==skill,"Visual-only sample preserves HP/time/cooldown "+prefix);
                Check(view.playerArt.rectTransform.anchoredPosition==position,"Character placement unchanged "+prefix);
                Check(!preview.Effect.Active&&preview.Effect.ActiveCount==0,"All sample objects returned "+prefix);
                // Validate future casting anchor against the actual authored skill key pose,
                // without sending a skill cue or invoking any Legacy VFX.
                var set=preview.dragons[d].LoadAnimationSet(stage);var player=view.playerArt.transform.Find("Pixel sprite").GetComponent<Image>();player.sprite=set.skill.frames[2];player.rectTransform.localScale=Vector3.one*set.skill.displayScale;player.rectTransform.anchoredPosition=set.skill.offsets[2]*256;
                Check(view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out var origin),"Casting anchor resolves "+prefix);
                preview.Effect.Play(origin,new Vector2(116,-58),new Vector2(30,86));preview.Effect.Step(.2f);Capture(view,camera,rt,prefix+"-casting");preview.Effect.Clear();
            }
            Check(preview.Effect.Capacity==28&&peak<=28,"Fixed pool, peak active "+peak);
            var images=view.frame.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("Ice module ")).ToArray();
            Check(images.Length==28&&images.All(i=>!i.raycastTarget&&i.material==Graphic.defaultGraphicMaterial),"Shared UI material and no input interception");
            Check(preview.style.modules.All(s=>s.texture.width==256&&!s.texture.isReadable&&s.texture.filterMode==FilterMode.Point),"One small Point atlas, no CPU texture copy");
            preview.Restart();int count=view.frame.GetComponentsInChildren<Transform>(true).Length;
            for(int repeat=0;repeat<20;repeat++){preview.Effect.Play(Vector2.zero,Vector2.zero,Vector2.zero);preview.Effect.Step(.9f);preview.Effect.Clear();}
            Check(count==view.frame.GetComponentsInChildren<Transform>(true).Length,"No pool growth after repeat/cancel");preview.Effect.Play(Vector2.zero,Vector2.zero,Vector2.zero);preview.Effect.enabled=false;Check(preview.Effect.ActiveCount==0,"Disable clears sample");preview.Effect.enabled=true;
            // Allocation/CPU sample isolates presentation stepping; not a mobile GPU claim.
            for(int i=0;i<100;i++){preview.Effect.Play(Vector2.zero,Vector2.zero,Vector2.zero);preview.Effect.Step(.9f);}
            long before=GC.GetAllocatedBytesForCurrentThread();var watch=System.Diagnostics.Stopwatch.StartNew();
            for(int i=0;i<1000;i++){preview.Effect.Play(Vector2.zero,Vector2.zero,Vector2.zero);preview.Effect.Step(.9f);preview.Effect.Clear();}
            watch.Stop();long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
            File.WriteAllText("Validation/IceStyle/result.txt","ICE_STYLE_VALIDATION_OK\nPool=28 Peak="+peak+"\n1000 Play/Step/Clear cycles ms="+watch.Elapsed.TotalMilliseconds+" Allocated bytes="+allocated+"\nEditor CPU-only measurement; device frame time/draw calls not certified.");
            camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);Debug.Log("ICE_STYLE_VALIDATION_OK");
        }
        static void Capture(BattleView view,Camera camera,RenderTexture target,string name)
        {
            Canvas.ForceUpdateCanvases();view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;camera.Render();var old=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(480,850,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,480,850),0,0);texture.Apply();File.WriteAllBytes("Validation/IceStyle/"+name+".png",texture.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(texture);
        }
        public static void BuildPreviewWeb()
        {
            var r=BuildPipeline.BuildPlayer(new[]{IceStyleSetup.ScenePath},"../IceStyle-WebGL",BuildTarget.WebGL,BuildOptions.None);
            if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Ice preview WebGL failed");Debug.Log("ICE_STYLE_WEBGL_OK");
        }
    }
}
