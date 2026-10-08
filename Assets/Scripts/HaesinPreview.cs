using UnityEngine;using Unity.Profiling;
namespace DragonTower {
 // Isolated review scene. Real equipped SkillData / BattleController path.
 public sealed class HaesinPreview:MonoBehaviour {
  public DragonData dragonData;public SkillData signature;
  BattleController c;float elapsed;int index,dragon;bool cast;ProfilerRecorder draws;double activeMs,idleMs;int activeFrames,idleFrames;long peakDraw;int hits;
  void Start(){c=FindFirstObjectByType<BattleController>();c.enabled=false;var screens=c.view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);Restart();}
  void Restart(){var d=dragonData;var skill=signature;var stats=d.Snapshot(20);stats.skill=skill.Snapshot();stats.criticalChance=0;stats.passiveMechanic=DragonPassiveMechanic.None;var enemy=BattleEnemyStats.Normal();enemy.maxHP=99999;enemy.interval=100;c.BeginBattle(d,stats,enemy,1,false,stats.maxHP,null,20,skill);c.SendMessage("OnApplicationFocus",true);elapsed=0;cast=false;activeFrames=idleFrames=0;activeMs=idleMs=0;peakDraw=0;hits=0;}
  void Update(){if(c==null)return;float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);elapsed+=dt;if(!cast&&elapsed>.35f){cast=c.RequestSkill();}c.CurrentBattle.Tick(dt);c.view.StepAnimation(c.CurrentBattle,dt);c.view.Show(c.CurrentBattle);var fx=c.view.GetComponent<HaesinVfx>();hits=Mathf.Max(hits,fx.HitsShown);if(elapsed>.1f){if(!fx.Clean){activeMs+=Time.unscaledDeltaTime*1000;activeFrames++;}else{idleMs+=Time.unscaledDeltaTime*1000;idleFrames++;}if(draws.Valid)peakDraw=System.Math.Max(peakDraw,draws.LastValue);}if(elapsed>5.0f){Debug.Log("HAESIN_WEB dragon="+dragon+" skill="+index+" activeMs="+activeMs/System.Math.Max(1,activeFrames)+" idleMs="+idleMs/System.Math.Max(1,idleFrames)+" draw="+peakDraw+" hits="+hits+" dropped="+fx.DroppedLayers);index++;Restart();}}
  void OnDisable(){if(c!=null)c.EndBattle();}void OnDestroy(){draws.Dispose();}
 }
}
