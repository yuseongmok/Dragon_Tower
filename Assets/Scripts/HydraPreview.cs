using UnityEngine;using Unity.Profiling;
namespace DragonTower {
 // Isolated combat review: one normal cast, followed by cooldown-paid recasts.
 public sealed class HydraPreview:MonoBehaviour {
  public DragonData dragon;public SkillData skill;BattleController c;ProfilerRecorder draws;float elapsed;bool cast;int cycle,hits;double activeMs,idleMs;int activeFrames,idleFrames;long peakDraw;
  void Start(){c=FindFirstObjectByType<BattleController>();c.enabled=false;var screens=c.view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);var stats=dragon.Snapshot(20);stats.skill=skill.Snapshot();stats.criticalChance=0;var enemy=BattleEnemyStats.Normal();enemy.maxHP=99999;enemy.interval=1000;enemy.damage=0;c.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,20,skill);c.SendMessage("OnApplicationFocus",true);c.CurrentBattle.Cue+=(cue,n)=>{if(c.CurrentBattle.ResolvingSkillHit)hits++;};draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);}
  void Update(){if(c==null)return;float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);elapsed+=dt;if(!cast&&elapsed>.3f)cast=c.RequestSkill();var b=c.CurrentBattle;b.Tick(dt);c.view.StepAnimation(b,dt);c.view.Show(b);var fx=c.view.GetComponent<HydraVfx>();if(elapsed>.1f){if(!fx.Clean){activeMs+=Time.unscaledDeltaTime*1000;activeFrames++;}else{idleMs+=Time.unscaledDeltaTime*1000;idleFrames++;}if(draws.Valid)peakDraw=System.Math.Max(peakDraw,draws.LastValue);}if(elapsed>10.5f){Debug.Log("HYDRA_WEB cycle="+cycle+" activeMs="+activeMs/System.Math.Max(1,activeFrames)+" idleMs="+idleMs/System.Math.Max(1,idleFrames)+" draw="+peakDraw+" hp="+b.PlayerHP+" hits="+hits+" clean="+fx.Clean+" dropped="+fx.Dropped);b.SkillReady=0;cycle++;elapsed=0;cast=false;activeFrames=idleFrames=0;activeMs=idleMs=0;peakDraw=0;}}
  void OnDisable(){if(c!=null)c.EndBattle();}void OnDestroy(){draws.Dispose();}
 }
}
