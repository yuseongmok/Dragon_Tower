using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class IceBatchValidation
    {
        static int wait;
        [InitializeOnLoadMethod]static void Listen(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("IceBatch",false)){wait=0;EditorApplication.update+=After;}};}
        public static void Shot()=>Run(0);
        public static void Ball()=>Run(1);
        public static void Breath()=>Run(2);
        public static void Wall()=>Run(3);
        static void Run(int through){IceBatchSetup.Install(through);Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.IceBatch.Validation");EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetInt("IceThrough",through);SessionState.SetBool("IceBatch",true);EditorApplication.isPlaying=true;}
        static void After(){if(++wait<12)return;EditorApplication.update-=After;SessionState.SetBool("IceBatch",false);try{Validate();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        static void Check(bool ok,string label){if(!ok)throw new Exception("ICE_BATCH "+label);Debug.Log("ICE_CHECK "+label);}
        static void Validate()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<BattleController>();c.enabled=false;var v=c.view;var screens=v.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);
            var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();var rt=new RenderTexture(480,850,24);rt.Create();cam.targetTexture=rt;cam.orthographic=true;cam.orthographicSize=425;cam.transform.position=new Vector3(0,0,-10);cam.transform.rotation=Quaternion.identity;cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=-1;
            var canvas=v.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;var r=(RectTransform)canvas.transform;r.position=Vector3.zero;r.localScale=Vector3.one;r.sizeDelta=new Vector2(480,850);ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory("Validation/IceBatch");
            int through=SessionState.GetInt("IceThrough",0);
            for(int d=0;d<2;d++)for(int stage=0;stage<3;stage++)for(int s=0;s<=through;s++)
            {
                var dragon=AssetDatabase.LoadAssetAtPath<DragonData>(d==0?"Assets/Data/Dragon9.asset":"Assets/Data/Dragon1.asset");var data=AssetDatabase.LoadAssetAtPath<SkillData>(IceBatchSetup.Paths[s]);
                var stats=dragon.Snapshot();stats.skill=data.Snapshot();stats.criticalChance=0;stats.passiveMechanic=DragonPassiveMechanic.None;var enemy=BattleEnemyStats.Normal();enemy.maxHP=9999;enemy.interval=100;
                c.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,stage==0?1:stage*10,data);c.SendMessage("OnApplicationFocus",true);var b=c.CurrentBattle;var fx=v.GetComponent<IceSkillVfx>();int objects=v.frame.GetComponentsInChildren<Transform>(true).Length;var times=new List<double>();int total=0;
                b.Cue+=(cue,damage)=>{if(cue==CombatCue.Skill||cue==CombatCue.SkillHit){times.Add(b.Time);total+=damage;Check(fx.HitsShown==times.Count,"cue/impact same frame");}};
                Check(dragon.CanOfferSkill(data),"shared skill eligible");
                var run=new TowerRun(dragon.maxHP,0,1,ElementType.Ice);run.ReplaceSkill(data,false);Check(run.BuildBattleStats(dragon.Snapshot()).skill.statusOnHit,"TowerRun preserves hit-based Slow");
                double nextStrike=b.NextEnemyStrike;
                Check(c.RequestSkill(),"cast accepted");Check(!c.RequestSkill(),"cooldown rejects recast");Check(!b.EnemySlowed,"no Slow before impact");
                string prefix=(d==0?"aurora":"luna")+"-"+stage+"-"+s;
                for(int f=0;f<90;f++){b.Tick(1f/30);v.StepAnimation(b,1f/30);v.Show(b);if(stage==2||f==10||f==20)Capture(v,cam,rt,prefix+"-"+f.ToString("000"));}
                Check(times.Count==data.hitCount&&fx.HitsShown==data.hitCount,"hit count "+prefix);for(int i=0;i<times.Count;i++)Check(Math.Abs(times[i]-data.initialHitDelay-i*data.hitInterval)<.034,"data timing "+prefix);
                Check(9999-b.EnemyHP==total,"damage matches cues");Check(Math.Abs(b.EnemySlowPower-data.statusPower)<.01,"Slow data power");Check(b.NextEnemyStrike>nextStrike,"Slow delays enemy attack");Check(!fx.Active,"skill visuals expire");Check(fx.ActiveImages>0,"Slow visual outlives skill");
                Check(v.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text==data.displayName,"HUD data name");Check(Array.Exists(v.skillButton.GetComponentsInChildren<Image>(),im=>im.sprite==data.icon&&im.enabled),"HUD data icon");
                b.Tick(Math.Max(5,data.cooldown));v.StepAnimation(b,Math.Max(5,data.cooldown));v.Show(b);Check(!b.EnemySlowed&&fx.ActiveImages==0,"Slow expires and returns pool");Check(objects==v.frame.GetComponentsInChildren<Transform>(true).Length,"fixed pool");
                int measuredPeak=fx.PeakImages;
                for(int repeat=0;repeat<3;repeat++){times.Clear();Check(c.RequestSkill(),"repeat cast");for(int j=0;j<90;j++){b.Tick(.05f);v.StepAnimation(b,.05f);}b.Tick(data.cooldown);v.StepAnimation(b,data.cooldown);times.Clear();fx.Configure(data);}
                Debug.Log("ICE_FORM_OK "+prefix+" Peak="+measuredPeak);
                for(int warm=0;warm<20;warm++){fx.Cast(new Vector2(30,-264));fx.Step(b,.15f);fx.Clear();}
                var watch=new System.Diagnostics.Stopwatch();long allocated=GC.GetAllocatedBytesForCurrentThread();watch.Start();for(int bench=0;bench<100;bench++){fx.Cast(new Vector2(30,-264));fx.Step(b,.15f);fx.Clear();}watch.Stop();allocated=GC.GetAllocatedBytesForCurrentThread()-allocated;Debug.Log("ICE_PERF "+prefix+" 100 cycles ms="+watch.Elapsed.TotalMilliseconds+" managed bytes="+allocated);
                Check(allocated==0,"warmed VFX stepping allocation free");
                var weak=BattleEnemyStats.Normal();weak.maxHP=1;weak.interval=100;c.BeginBattle(dragon,stats,weak,1,false,stats.maxHP,null,1,data);c.RequestSkill();c.CurrentBattle.Tick(data.initialHitDelay+.01f);v.StepAnimation(c.CurrentBattle,.01f);Check(!fx.Active&&fx.ActiveImages==0,"death clears effects");
                c.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,1,data);c.RequestSkill();c.EndBattle();Check(!fx.Active&&fx.ActiveImages==0,"end battle clears effects");
            }
            if(through==3){var zephyr=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");WindBladeValidation.Validate(v,zephyr,_=>{});GaleSlashValidation.Validate(v,zephyr,_=>{});WindClawValidation.Validate(v,zephyr,_=>{});DantianValidation.Validate(v,zephyr,_=>{});WindRemasterValidation.Validate(v,zephyr);Check(!v.GetComponent<IceSkillVfx>().Configured,"Wind routing preserved");}
            cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);File.WriteAllText("Validation/IceBatch/result-"+through+".txt","ICE_BATCH_OK through="+through);Debug.Log("ICE_BATCH_OK through="+through);
        }
        static void Capture(BattleView v,Camera cam,RenderTexture rt,string name){Canvas.ForceUpdateCanvases();v.frame.localScale=Vector3.one;v.frame.anchoredPosition=Vector2.zero;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(480,850,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,480,850),0,0);t.Apply();File.WriteAllBytes("Validation/IceBatch/"+name+".png",t.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(t);}
    }
}
