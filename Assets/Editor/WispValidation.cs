using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class WispValidation
    {
        static int wait;
        public static void Regression(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Wisp.Regression");EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetInt("IceThrough",3);SessionState.SetBool("WispRegression",true);SessionState.SetBool("WispValidate",true);EditorApplication.isPlaying=true;}
        public static void Run(){WispSetup.Install();Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Wisp.Validation");EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetBool("WispValidate",true);EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Listen(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("WispValidate",false)){wait=0;EditorApplication.update+=After;}};}
        static void After(){if(++wait<12)return;EditorApplication.update-=After;SessionState.SetBool("WispValidate",false);try{if(SessionState.GetBool("WispRegression",false)){SessionState.SetBool("WispRegression",false);Performance();typeof(IceBatchValidation).GetMethod("Validate",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,null);}else Test();Debug.Log("WISP_VALIDATION_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        static void Performance()
        {
            var d=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset");var s=AssetDatabase.LoadAssetAtPath<SkillData>(WispSetup.SkillPath);var c=UnityEngine.Object.FindFirstObjectByType<BattleController>();c.enabled=false;c.BeginBattle(d,Stats(d,s),Enemy(),1,false,d.maxHP,null,20,s);var b=c.CurrentBattle;var fx=c.view.GetComponent<WispVfx>();b.Skill();
            ProfileCycle(fx,b,s);ProfileCycle(fx,b,s);long bytes=GC.GetAllocatedBytesForCurrentThread();var timer=System.Diagnostics.Stopwatch.StartNew();for(int repeat=0;repeat<10;repeat++)ProfileCycle(fx,b,s);timer.Stop();bytes=GC.GetAllocatedBytesForCurrentThread()-bytes;Debug.Log("WISP_PERF 1500 presentation steps ms="+timer.Elapsed.TotalMilliseconds+" managed bytes="+bytes+" peak="+fx.PeakImages);fx.Cancel();c.EndBattle();
        }
        static void ProfileCycle(WispVfx fx,BattleModel b,SkillData s){fx.Cast(new Vector2(30,-264));int hit=0;for(int f=0;f<150;f++){if(hit<s.hitCount&&f/60f>=s.initialHitDelay+hit*s.hitInterval){fx.Hit();hit++;}fx.Step(b,1f/60);}fx.Cancel();}
        static void Check(bool ok,string message){if(!ok)throw new Exception("WISP_CHECK_FAIL "+message);Debug.Log("WISP_CHECK "+message);}
        static BattleStats Stats(DragonData d,SkillData skill){var stats=d.Snapshot();stats.skill=skill.Snapshot();stats.criticalChance=0;stats.passiveMechanic=DragonPassiveMechanic.None;return stats;}
        static BattleEnemyStats Enemy(){var e=BattleEnemyStats.Normal();e.maxHP=99999;e.interval=100;return e;}
        static void Test()
        {
            var aurora=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset");var luna=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon1.asset");var skill=AssetDatabase.LoadAssetAtPath<SkillData>(WispSetup.SkillPath);
            Check(aurora.signatureSkill==skill&&aurora.CanOfferSkill(skill)&&!luna.CanOfferSkill(skill),"Aurora only, excluded from Luna");Check(aurora.elementSkillPool==null||!aurora.elementSkillPool.Contains(skill),"not shared Ice pool");
            var forbidden=new BattleModel(Stats(luna,skill),Enemy());Check(!forbidden.Skill(),"model rejects foreign signature");
            var run=new TowerRun(aurora.maxHP,0,1,ElementType.Ice,ElementType.Neutral,aurora.StableId,aurora);run.ReplaceSkill(skill,false);var snapshot=run.BuildBattleStats(aurora.Snapshot()).skill;Check(snapshot.dodgeFreeAfterDuration==3&&snapshot.finalHitDamageMultiplier==5,"TowerRun forwards new data");
            var m=new BattleModel(Stats(aurora,skill),Enemy());Check(m.Dodge()&&!m.Dodge(),"normal cooldown before cast");Check(m.Skill(),"cast accepted on existing dodge cooldown");Check(m.Dodge()&&m.Dodge(),"immediate release bypasses only cooldown");Check(Math.Abs(m.DodgeUntil-m.Time-.42)<.00001,"normal dodge invulnerability duration");
            m.Tick(2.5f);Check(m.DodgeCooldownReleased&&Math.Abs(m.DodgeReleaseRemaining-3)<.0001,"exactly 3 seconds after visual completion");m.Tick(2.99f);Check(m.Dodge(),"release at end minus 10ms");m.Tick(.02f);Check(!m.DodgeCooldownReleased&&!m.Dodge(),"normal cooldown restored after expiry");m.Tick(1.4f);Check(m.Dodge(),"normal dodge ready again");
            var doubleStats=Stats(aurora,skill);doubleStats.augments=new[]{new BattleAugment{mechanic=AugmentMechanic.DoubleCasting}};var doubleModel=new BattleModel(doubleStats,Enemy());doubleModel.Skill();doubleModel.Tick(4.19f);Check(doubleModel.ScheduledSkillHits==26&&doubleModel.EnemyHP==99999-408&&Math.Abs(doubleModel.DodgeReleaseRemaining-3)<.0002,"existing DoubleCasting retains both final hits and 3s recovery");
            var c=UnityEngine.Object.FindFirstObjectByType<BattleController>();c.enabled=false;var v=c.view;var screens=v.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);
            var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();var rt=new RenderTexture(480,850,24);rt.Create();cam.targetTexture=rt;cam.orthographic=true;cam.orthographicSize=425;cam.transform.position=new Vector3(0,0,-10);cam.transform.rotation=Quaternion.identity;cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=-1;
            var canvas=v.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;var rect=(RectTransform)canvas.transform;rect.position=Vector3.zero;rect.localScale=Vector3.one;rect.sizeDelta=new Vector2(480,850);ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory("Validation/Wisp");
            for(int stage=0;stage<3;stage++){
                c.BeginBattle(aurora,Stats(aurora,skill),Enemy(),1,false,aurora.maxHP,null,stage==0?1:stage*10,skill);c.SendMessage("OnApplicationFocus",true);var b=c.CurrentBattle;var fx=v.GetComponent<WispVfx>();int count=0,total=0,lastDamage=0;int objects=v.frame.GetComponentsInChildren<Transform>(true).Length;
                b.Cue+=(cue,damage)=>{if((cue==CombatCue.Skill||cue==CombatCue.SkillHit)&&b.ResolvingSkillHit){Check(Math.Abs(b.Time-skill.initialHitDelay-count*skill.hitInterval)<.035,"hit time "+count);count++;total+=damage;lastDamage=damage;Check(fx.HitsShown==count,"VFX contact same frame "+count);}};
                Check(c.RequestSkill()&&!c.RequestSkill(),"cooldown blocks repeated skill");
                for(int frame=0;frame<210;frame++){b.Tick(1f/30);if(frame%9==0&&frame>65&&frame<160)Check(c.RequestDodge(frame%18==0?1:-1),"swipe route free dodge");v.StepAnimation(b,1f/30);v.Show(b);if(stage==2||frame==15||frame==58||frame==100)Capture(v,cam,rt,"aurora-"+stage+"-"+frame.ToString("000"));}
                Check(count==13&&total==204&&lastDamage==60&&b.EnemyHP==99999-total,"12 contacts plus boosted final, exact damage");Check(!b.DodgeCooldownReleased&&fx.Clean,"buff and visuals expire");Check(fx.PeakImages<192,"pool headroom "+fx.PeakImages);Check(objects==v.frame.GetComponentsInChildren<Transform>(true).Length,"no per-cast objects");Check(v.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text=="위습","HUD name from data");
                Debug.Log("WISP_FORM_OK stage="+stage+" peak="+fx.PeakImages);
                c.EndBattle();Check(fx.Clean,"exit cleanup");
            }
            for(int repeat=0;repeat<3;repeat++){c.BeginBattle(aurora,Stats(aurora,skill),Enemy(),1,false,aurora.maxHP,null,20,skill);var b=c.CurrentBattle;Check(c.RequestSkill(),"repeat cast");for(int f=0;f<180;f++){b.Tick(1f/30);v.StepAnimation(b,1f/30);}Check(!b.DodgeCooldownReleased&&v.GetComponent<WispVfx>().Clean,"repeat release cleanup");}
            var weak=Enemy();weak.maxHP=1;c.BeginBattle(aurora,Stats(aurora,skill),weak,1,false,aurora.maxHP,null,20,skill);c.RequestSkill();c.CurrentBattle.Tick(.4f);v.StepAnimation(c.CurrentBattle,.4f);Check(!c.CurrentBattle.DodgeCooldownReleased&&v.GetComponent<WispVfx>().Clean,"target death cleanup");
            c.BeginBattle(aurora,Stats(aurora,skill),Enemy(),1,false,aurora.maxHP,null,20,skill);c.RequestSkill();var fx2=v.GetComponent<WispVfx>();c.CurrentBattle.Tick(2);v.StepAnimation(c.CurrentBattle,2);fx2.enabled=false;Check(!c.CurrentBattle.DodgeCooldownReleased&&fx2.Clean,"component disable clears state and offsets");fx2.enabled=true;c.EndBattle();
            cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
        static void Capture(BattleView v,Camera cam,RenderTexture rt,string name){Canvas.ForceUpdateCanvases();v.frame.localScale=Vector3.one;v.frame.anchoredPosition=Vector2.zero;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(480,850,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,480,850),0,0);tex.Apply();File.WriteAllBytes("Validation/Wisp/"+name+".png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=old;}
    }
}


