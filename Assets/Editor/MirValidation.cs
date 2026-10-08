using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class MirValidation
    {
        static int wait;
        public static void MouthCheck(){SessionState.SetBool("MirMouthOnly",true);Run();}
        public static void Run(){MirSetup.InstallAll();Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Mir.Validation");EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetBool("MirCheck",true);EditorApplication.isPlaying=true;}
        [InitializeOnLoadMethod]static void Listen(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("MirCheck",false)){wait=0;EditorApplication.update+=After;}};}
        static void After(){if(++wait<12)return;EditorApplication.update-=After;SessionState.SetBool("MirCheck",false);try{Test();Debug.Log("MIR_VALIDATION_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
        static void Check(bool ok,string label){if(!ok)throw new Exception("MIR_FAIL "+label);Debug.Log("MIR_CHECK "+label);}
        static void Test()
        {
            var d=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon10.asset");var c=UnityEngine.Object.FindFirstObjectByType<BattleController>();c.enabled=false;var v=c.view;
            var screens=v.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);
            var cam=UnityEngine.Object.FindFirstObjectByType<Camera>();var rt=new RenderTexture(480,850,24);rt.Create();cam.targetTexture=rt;cam.orthographic=true;cam.orthographicSize=425;cam.transform.position=new Vector3(0,0,-10);cam.transform.rotation=Quaternion.identity;cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=-1;
            var canvas=v.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=cam;var rect=(RectTransform)canvas.transform;rect.position=Vector3.zero;rect.localScale=Vector3.one;rect.sizeDelta=new Vector2(480,850);ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory("Validation/Mir");
            Check(d.signatureSkill==null,"Signature intentionally not implemented");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            for(int stage=SessionState.GetBool("MirMouthOnly",false)?1:0;stage<(SessionState.GetBool("MirMouthOnly",false)?2:3);stage++){
                int level=stage==0?1:stage*10;var set=d.LoadAnimationSet(stage);var stats=d.Snapshot(level);stats.criticalChance=0;var enemy=BattleEnemyStats.Normal();enemy.maxHP=99999;enemy.interval=100;
                void Fresh(){c.BeginBattle(d,stats,enemy,1,false,stats.maxHP,null,level,d.skill);c.SendMessage("OnApplicationFocus",true);v.BindCombat(()=>c.RequestAttack(),()=>c.RequestSkill(),dir=>c.RequestDodge(dir),()=>{},()=>c.CanControl);}
                Fresh();var actor=v.playerArt.transform.Find("Pixel sprite").GetComponent<Image>();
                Check((ElementHudTheme)typeof(BattleView).GetField("hudTheme",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(v)==ElementHudThemes.Get(ElementType.Wind),"Wind theme "+stage);
                foreach(DragonAnimationState state in Enum.GetValues(typeof(DragonAnimationState))){
                    var clip=set.Get(state);Check(clip.frames.Length==4&&clip.durations.SequenceEqual(template.Get(state).durations),"timing "+stage+" "+state);Fresh();
                    if(state==DragonAnimationState.Attack)Check(c.RequestAttack(),"attack");else if(state==DragonAnimationState.Skill)Check(c.RequestSkill(),"skill");else if(state==DragonAnimationState.Dodge)Check(c.RequestDodge(-1),"dodge");else if(state==DragonAnimationState.Hit)v.PlayCue(CombatCue.EnemyHit,10);
                    else if(state==DragonAnimationState.Death){var weak=d.Snapshot(level);weak.maxHP=1;var killer=BattleEnemyStats.Normal();killer.damage=9999;killer.interval=.01f;c.BeginBattle(d,weak,killer,1,false,1,null,level,d.skill);for(int n=0;n<500&&c.CurrentBattle.Result==BattleResult.Fighting;n++)c.CurrentBattle.Tick(.02f);Check(c.CurrentBattle.Result==BattleResult.Defeat,"death logic");}
                    for(int f=0;f<4;f++){v.StepAnimation(c.CurrentBattle,f==0?.001f:clip.durations[f-1]);v.Show(c.CurrentBattle);Check(clip.frames.Contains(actor.sprite),"sprite "+state+" "+f);Check(v.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,v.frame,out var point)&&float.IsFinite(point.x),"casting anchor");if(stage==2)foreach(string socket in new[]{"AttackOrigin","CastingCenter","DragonHead"})Check(v.TryGetCharacterAttachment(socket,v.frame,out var at)&&float.IsFinite(at.x)&&float.IsFinite(at.y),"optional anchor "+socket);Capture(v,cam,rt,stage+"-"+state+"-"+f);if(state==DragonAnimationState.Skill)CaptureAnchors(v,cam,rt,stage+"-Anchors-"+f);}
                }
                Fresh();var gesture=v.Gestures;var gr=(RectTransform)gesture.transform;var raycaster=v.GetComponent<GraphicRaycaster>();
                PointerEventData E(int id,Vector2 p)=>new PointerEventData(EventSystem.current){pointerId=id,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(cam,gr.TransformPoint(p)),pointerPressRaycast=new RaycastResult{module=raycaster,gameObject=gesture.gameObject}};
                foreach(int id in new[]{-1,7}){Fresh();typeof(BattleGestureSurface).GetField("lastTouch",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(gesture,-999f);int hp=c.CurrentBattle.EnemyHP;gesture.OnPointerDown(E(id,Vector2.zero));gesture.OnPointerUp(E(id,Vector2.zero));Check(c.CurrentBattle.EnemyHP<hp,"tap "+id);foreach(int dir in new[]{-1,1}){Fresh();gesture.OnPointerDown(E(id,Vector2.zero));gesture.OnDrag(E(id,new Vector2(dir*70,0)));gesture.OnPointerUp(E(id,new Vector2(dir*70,0)));Check(c.CurrentBattle.DodgeReady>0,"swipe "+id+" "+dir);}}
                Check(set.attachments.Length==72,"per pose head / casting anchors");
                Check(d.passiveMechanic==DragonPassiveMechanic.RuyiOrb&&d.maxHP==90&&d.attackDamage==9&&d.elementType==ElementType.Wind,"existing Mir combat data");
                Fresh();int before=c.CurrentBattle.EnemyHP;Check(c.RequestAttack(),"passive attack accepted");
                int boosted=stats.attackDamage+(int)Math.Round(stats.attackDamage*stats.passiveStage*.1,MidpointRounding.AwayFromZero);
                Check(before-c.CurrentBattle.EnemyHP==boosted,"RuyiOrb combined two-hit damage "+stage);
                string[] paths={"Assets/Data/Skill2.asset","Assets/Data/Skills/skill_falling_flower.asset","Assets/Data/Skills/skill_gale_slash.asset","Assets/Data/Skills/skill_wind_claw.asset"};
                for(int sk=0;sk<paths.Length;sk++){
                    var skill=AssetDatabase.LoadAssetAtPath<SkillData>(paths[sk]);Check(d.CanOfferSkill(skill),"shared Wind eligible "+skill.StableId);
                    var testStats=d.Snapshot(level);testStats.skill=skill.Snapshot();testStats.criticalChance=0;
                    c.BeginBattle(d,testStats,enemy,1,false,testStats.maxHP,null,level,skill);c.SendMessage("OnApplicationFocus",true);
                    int hits=0;c.CurrentBattle.Cue+=(cue,damage)=>{if(cue==CombatCue.Skill||cue==CombatCue.SkillHit)hits++;};
                    int hp=c.CurrentBattle.EnemyHP;
                    for(int frame=0;frame<75;frame++){
                        if(frame==8)Check(c.RequestSkill(),"Wind cast "+skill.StableId);
                        c.CurrentBattle.Tick(1f/30);v.StepAnimation(c.CurrentBattle,1f/30);v.Show(c.CurrentBattle);
                        foreach(DragonVisualAnchor kind in Enum.GetValues(typeof(DragonVisualAnchor)))CheckAnchor(v,kind);
                        if(stage==2)Capture(v,cam,rt,"Wind-"+sk+"-"+frame.ToString("D3"));
                    }
                    Check(c.CurrentBattle.EnemyHP<hp&&hits==Math.Max(1,testStats.skill.hitCount),"Wind hits "+stage+" "+skill.StableId+" = "+hits);
                }
                c.EndBattle();Debug.Log("MIR_FORM_OK "+stage);
            }
            cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
        }
        static void CheckAnchor(BattleView v,DragonVisualAnchor kind){if(!v.TryGetCharacterAnchor(kind,v.frame,out var p)||!float.IsFinite(p.x)||!float.IsFinite(p.y))throw new Exception("Invalid Mir anchor "+kind);}
        static void CaptureAnchors(BattleView v,Camera cam,RenderTexture rt,string name){
            var markers=new System.Collections.Generic.List<GameObject>();
            foreach(var kind in new[]{DragonVisualAnchor.SkillOrigin,DragonVisualAnchor.GroundPosition}){
                Check(v.TryGetCharacterAnchor(kind,v.frame,out var p)&&float.IsFinite(p.x)&&float.IsFinite(p.y),"finite "+kind);
                var go=new GameObject("Validation only "+kind,typeof(RectTransform),typeof(Image));var r=go.GetComponent<RectTransform>();r.SetParent(v.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=p;r.sizeDelta=Vector2.one*7;go.GetComponent<Image>().color=kind==DragonVisualAnchor.SkillOrigin?Color.cyan:Color.magenta;markers.Add(go);
            }
            Capture(v,cam,rt,name);foreach(var go in markers)UnityEngine.Object.DestroyImmediate(go);
        }
        static void Capture(BattleView v,Camera cam,RenderTexture rt,string name){Canvas.ForceUpdateCanvases();v.frame.localScale=Vector3.one;v.frame.anchoredPosition=Vector2.zero;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(480,850,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,480,850),0,0);t.Apply();File.WriteAllBytes("Validation/Mir/"+name+".png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);RenderTexture.active=old;}
    }
}
