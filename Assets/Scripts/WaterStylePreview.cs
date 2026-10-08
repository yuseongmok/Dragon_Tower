using UnityEngine;using Unity.Profiling;
namespace DragonTower {
 public sealed class WaterStylePreview:MonoBehaviour {
  public WaterVfxStyle style;public DragonData[] dragons;public int dragonIndex;public int evolution=2;public WaterSample sample;public bool autoplay=true;
  BattleController controller;BattleView view;WaterStylePrototype fx;float elapsed;ProfilerRecorder draws;double activeMs,idleMs;int activeFrames,idleFrames;long peakDraw;int cycle;
  public WaterStylePrototype Effect=>fx;public BattleModel Model=>controller.CurrentBattle;
  void Start(){Initialize();Restart();draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);}
  public void Initialize(){if(fx!=null)return;controller=FindFirstObjectByType<BattleController>();view=controller.view;controller.enabled=false;var screens=view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);fx=gameObject.AddComponent<WaterStylePrototype>();fx.style=style;fx.Initialize(view);}
  [ContextMenu("Replay selected Water sample")]
  public void Restart(){Initialize();var d=dragons[Mathf.Clamp(dragonIndex,0,dragons.Length-1)];int level=evolution==0?1:evolution*10;var stats=d.Snapshot(level);stats.passiveMechanic=DragonPassiveMechanic.None;stats.criticalChance=0;var enemy=BattleEnemyStats.Normal();enemy.maxHP=99999;enemy.interval=100;controller.BeginBattle(d,stats,enemy,1,false,stats.maxHP,null,level,d.skill);view.StepAnimation(Model,.001f);view.Show(Model);elapsed=0;view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out var origin);var ground=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));fx.Play(sample,origin,ground);}
  public void Step(float delta){elapsed+=delta;Model.Tick(delta);view.StepAnimation(Model,delta);view.Show(Model);fx.Step(Model,delta);}
  void Update(){if(!autoplay||fx==null)return;Step(Mathf.Min(Time.unscaledDeltaTime,.05f));if(elapsed>.1f){if(fx.ActiveCount>0){activeMs+=Time.unscaledDeltaTime*1000;activeFrames++;}else{idleMs+=Time.unscaledDeltaTime*1000;idleFrames++;}if(draws.Valid)peakDraw=System.Math.Max(peakDraw,draws.LastValue);}if(elapsed>4.1f){Debug.Log("WATER_WEB cycle="+cycle+" sample="+sample+" dragon="+dragonIndex+" activeMs="+activeMs/System.Math.Max(1,activeFrames)+" idleMs="+idleMs/System.Math.Max(1,idleFrames)+" peakDraw="+peakDraw+" peakImages="+fx.PeakQuads+" dropped="+fx.DroppedLayers);cycle++;sample=(WaterSample)(((int)sample+1)%4);if(sample==0)dragonIndex=(dragonIndex+1)%dragons.Length;activeFrames=idleFrames=0;activeMs=idleMs=0;peakDraw=0;Restart();}}
  void OnDisable(){fx?.Clear();if(controller!=null&&view!=null)controller.EndBattle();}void OnDestroy(){draws.Dispose();}
 }
}

