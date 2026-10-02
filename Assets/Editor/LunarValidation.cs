using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class LunarValidation
    {
        static int wait;
        public static void Regression(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Lunar.Regression");EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetInt("IceThrough",3);SessionState.SetBool("LunarRegression",true);SessionState.SetBool("LunarValidate",true);EditorApplication.isPlaying=true;}
        public static void Run(){LunarSetup.Install();Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Lunar.Validation");EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetBool("LunarValidate",true);EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Listen(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("LunarValidate",false)){wait=0;EditorApplication.update+=After;}};}
        static void After(){if(++wait<12)return;EditorApplication.update-=After;SessionState.SetBool("LunarValidate",false);try{if(SessionState.GetBool("LunarRegression",false)){SessionState.SetBool("LunarRegression",false);typeof(WispValidation).GetMethod("Test",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);typeof(IceBatchValidation).GetMethod("Validate",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);}else Test();Debug.Log("LUNAR_VALIDATION_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        static void Check(bool ok,string message){if(!ok)throw new Exception("LUNAR_FAIL "+message);Debug.Log("LUNAR_CHECK "+message);}
        static BattleStats Stats(DragonData d,SkillData s){var b=d.Snapshot();b.skill=s.Snapshot();b.criticalChance=0;b.passiveMechanic=DragonPassiveMechanic.None;return b;}
        static BattleEnemyStats Enemy(int hp=99999){var e=BattleEnemyStats.Normal();e.maxHP=hp;e.interval=100;return e;}
        public static void Slow(BattleModel b,float duration)=>typeof(BattleModel).GetMethod("ApplySlow",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(b,new object[]{duration,60f});
        static void Test()
        {
            var luna=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon1.asset");var aurora=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset");var skill=AssetDatabase.LoadAssetAtPath<SkillData>(LunarSetup.SkillPath);
            Check(luna.signatureSkill==skill&&luna.CanOfferSkill(skill)&&!aurora.CanOfferSkill(skill),"Luna signature only");Check(luna.elementSkillPool==null||!luna.elementSkillPool.Contains(skill),"not common Ice pool");Check(!new BattleModel(Stats(aurora,skill),Enemy()).Skill(),"model exclusive guard");
            var run=new TowerRun(luna.maxHP,0,1,ElementType.Ice,ElementType.Neutral,luna.StableId,luna);run.ReplaceSkill(skill,false);Check(run.BuildBattleStats(luna.Snapshot()).skill.slowBonusDamagePercent==50,"TowerRun bonus data");
            for(int mode=0;mode<3;mode++){var b=new BattleModel(Stats(luna,skill),Enemy());if(mode==1)Slow(b,.2f);b.Skill();if(mode==2)Slow(b,5);b.Tick(2);Check(b.EnemyHP==99999-(mode==1?330:220),"cast-time Slow snapshot "+mode);Check(!b.ProtectedSkillActive&&!b.DodgeCooldownReleased,"no unrequested immunity or dodge buff");}
            var ds=Stats(luna,skill);ds.augments=new[]{new BattleAugment{mechanic=AugmentMechanic.DoubleCasting}};var dm=new BattleModel(ds,Enemy());Slow(dm,3);dm.Skill();dm.Tick(2);Check(dm.EnemyHP==99999-660,"DoubleCasting keeps two main and two bonus hits");
            var c=UnityEngine.Object.FindFirstObjectByType<BattleController>();c.enabled=false;var v=c.view;var screens=v.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);var arena=v.frame.Find("Arena").GetComponent<Image>();var original=arena.material;
            var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();var rt=new RenderTexture(480,850,24);rt.Create();cam.targetTexture=rt;cam.orthographic=true;cam.orthographicSize=425;cam.transform.position=new Vector3(0,0,-10);cam.transform.rotation=Quaternion.identity;cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=-1;var canvas=v.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;var r=(RectTransform)canvas.transform;r.position=Vector3.zero;r.localScale=Vector3.one;r.sizeDelta=new Vector2(480,850);ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory("Validation/Lunar");
            for(int stage=0;stage<3;stage++)for(int slow=0;slow<2;slow++){
                c.BeginBattle(luna,Stats(luna,skill),Enemy(),1,false,luna.maxHP,null,stage==0?1:stage*10,skill);c.SendMessage("OnApplicationFocus",true);var b=c.CurrentBattle;var fx=v.GetComponent<LunarVfx>();if(slow==1)Slow(b,3);int hits=0,bonuses=0;double mainTime=0;
                b.Cue+=(cue,damage)=>{if((cue==CombatCue.Skill||cue==CombatCue.SkillHit)&&b.ResolvingSkillHit){hits++;mainTime=b.Time;Check(fx.BaseHits==hits,"main VFX same frame");}if(cue==CombatCue.SkillBonusHit){bonuses++;Check(Math.Abs(b.Time-mainTime-skill.slowBonusDelay)<.035&&fx.BonusHits==bonuses,"bonus VFX and damage synchronized");}};
                Check(c.RequestSkill()&&!c.RequestSkill(),"normal cooldown and accepted cast");Check(fx.Synergy==(slow==1),"visual synergy matches snapshot");int objects=v.frame.GetComponentsInChildren<Transform>(true).Length;
                for(int f=0;f<105;f++){b.Tick(1f/30);v.StepAnimation(b,1f/30);v.Show(b);if(stage==2||f==20||f==34)Capture(v,cam,rt,"luna-"+stage+"-"+slow+"-"+f.ToString("000"));}
                Check(hits==1&&bonuses==slow&&b.EnemyHP==99999-(slow==1?330:220),"damage/count "+stage+" "+slow);Check(fx.Clean&&arena.material==original&&Time.timeScale==1,"end restores domain/offsets/time");Check(fx.PeakImages<192&&objects==v.frame.GetComponentsInChildren<Transform>(true).Length,"fixed pool "+fx.PeakImages);Check(v.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text=="초월","HUD reads SkillData");Debug.Log("LUNAR_FORM_OK stage="+stage+" slow="+slow+" peak="+fx.PeakImages);c.EndBattle();
            }
            foreach(float stop in new[]{.1f,.6f,.97f,1.13f}){c.BeginBattle(luna,Stats(luna,skill),Enemy(),1,false,luna.maxHP,null,20,skill);Slow(c.CurrentBattle,3);c.RequestSkill();c.CurrentBattle.Tick(stop);v.StepAnimation(c.CurrentBattle,stop);var b=c.CurrentBattle;c.EndBattle();Check(v.GetComponent<LunarVfx>().Clean&&arena.material==original&&!b.SlowBonusPending,"cancel phase "+stop);}
            foreach(bool playerDies in new[]{false,true}){var e=Enemy(playerDies?99999:1);if(playerDies){e.interval=.3f;e.damage=99999;}c.BeginBattle(luna,Stats(luna,skill),e,1,false,luna.maxHP,null,20,skill);Slow(c.CurrentBattle,3);c.RequestSkill();c.CurrentBattle.Tick(1);v.StepAnimation(c.CurrentBattle,1);Check(v.GetComponent<LunarVfx>().Clean&&arena.material==original,"death restores domain "+playerDies);}
            c.BeginBattle(luna,Stats(luna,skill),Enemy(),1,false,luna.maxHP,null,20,skill);Slow(c.CurrentBattle,3);c.RequestSkill();c.CurrentBattle.Tick(.97f);v.StepAnimation(c.CurrentBattle,.97f);var component=v.GetComponent<LunarVfx>();component.enabled=false;Check(component.Clean&&arena.material==original&&!c.CurrentBattle.SlowBonusPending,"disable during impact restores state");component.enabled=true;c.EndBattle();
            cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
        static void Capture(BattleView v,Camera cam,RenderTexture rt,string name){Canvas.ForceUpdateCanvases();v.frame.localScale=Vector3.one;v.frame.anchoredPosition=Vector2.zero;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(480,850,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,480,850),0,0);t.Apply();File.WriteAllBytes("Validation/Lunar/"+name+".png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);RenderTexture.active=old;}
    }
}
