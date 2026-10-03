using UnityEngine;using Unity.Profiling;
namespace DragonTower {
 public sealed class VolcanicMeteorPreview:MonoBehaviour {
  public DragonData dragon;public SkillData skill;BattleController c;float age;int cycle;ProfilerRecorder draws;long peakDraw,allocated;double activeMs,idleMs,updateMs;int activeFrames,idleFrames;System.Diagnostics.Stopwatch timer=new System.Diagnostics.Stopwatch();
  static long AllocatedBytes(){
#if UNITY_WEBGL && !UNITY_EDITOR
   return 0; // Per-thread allocation API is unavailable in WebGL; do not invoke it.
#else
   return System.GC.GetAllocatedBytesForCurrentThread();
#endif
  }
  void Start(){c=FindFirstObjectByType<BattleController>();c.enabled=false;var screens=c.view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);Begin();}
  void Begin(){var stats=dragon.Snapshot(20);stats.skill=skill.Snapshot();stats.criticalChance=0;stats.passiveMechanic=DragonPassiveMechanic.None;var enemy=BattleEnemyStats.Normal();enemy.maxHP=99999;enemy.interval=100;c.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP/2,null,20,skill);c.SendMessage("OnApplicationFocus",true);age=0;}
  void Update(){if(c==null)return;long beforeBytes=AllocatedBytes();timer.Restart();float dt=Mathf.Min(Time.unscaledDeltaTime,.05f),before=age;age+=dt;if(before<.65f&&age>=.65f)c.RequestSkill();var b=c.CurrentBattle;if(b!=null){b.Tick(dt);c.view.StepAnimation(b,dt);c.view.Show(b);}timer.Stop();var fx=c.view.GetComponent<VolcanicMeteorVfx>();if(fx.Active){activeMs+=Time.unscaledDeltaTime*1000;activeFrames++;updateMs+=timer.Elapsed.TotalMilliseconds;allocated+=AllocatedBytes()-beforeBytes;}else if(age>.15f){idleMs+=Time.unscaledDeltaTime*1000;idleFrames++;}if(draws.Valid)peakDraw=System.Math.Max(peakDraw,draws.LastValue);if(age>9){Debug.Log("VOLCANIC_WEB cycle="+cycle+" activeFrameMs="+activeMs/System.Math.Max(1,activeFrames)+" idleFrameMs="+idleMs/System.Math.Max(1,idleFrames)+" updateMs="+updateMs/System.Math.Max(1,activeFrames)+" allocationCounter="+(Application.platform==RuntimePlatform.WebGLPlayer?"unavailable":(allocated/System.Math.Max(1,activeFrames)).ToString())+" peakDraw="+peakDraw+" peakImages="+fx.PeakImages+" dropped="+fx.DroppedLayers+" HP="+b.PlayerHP);activeMs=idleMs=updateMs=0;activeFrames=idleFrames=0;peakDraw=allocated=0;cycle++;Begin();}}
  void OnDisable(){if(c!=null&&c.view!=null)c.EndBattle();}void OnDestroy(){draws.Dispose();}
 }
}
