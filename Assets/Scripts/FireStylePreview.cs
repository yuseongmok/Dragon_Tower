using System.Reflection;
using UnityEngine;
using Unity.Profiling;
namespace DragonTower
{
    // Review scene only. Sample Burn uses the unchanged existing status method.
    public sealed class FireStylePreview:MonoBehaviour
    {
        public FireVfxStyle style;public DragonData[] dragons;public int dragonIndex;public int evolution=2;public bool autoplay=true;
        BattleController controller;BattleView view;FireStylePrototype fx;float elapsed;bool sampleBurn;
        static readonly MethodInfo ApplyBurn=typeof(BattleModel).GetMethod("ApplyBurn",BindingFlags.Instance|BindingFlags.NonPublic);
        ProfilerRecorder draws,gc;int frames,activeFrames;double idleMs,activeMs;int idleCount;long maxDraw,maxGC;
        public FireStylePrototype Effect=>fx;public BattleModel Model=>controller.CurrentBattle;
        void Start(){Initialize();Restart();draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);}
        public void Initialize(){if(fx!=null)return;controller=FindFirstObjectByType<BattleController>();view=controller.view;controller.enabled=false;var screens=view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);fx=gameObject.AddComponent<FireStylePrototype>();fx.style=style;fx.Initialize(view);}
        [ContextMenu("Replay Fire samples")]
        public void Restart()
        {
            Initialize();var d=dragons[Mathf.Clamp(dragonIndex,0,dragons.Length-1)];int level=evolution==0?1:evolution*10;var stats=d.Snapshot(level);stats.passiveMechanic=DragonPassiveMechanic.None;stats.criticalChance=0;var enemy=BattleEnemyStats.Normal();enemy.maxHP=99999;enemy.interval=100;
            controller.BeginBattle(d,stats,enemy,1,false,stats.maxHP,null,level,d.skill);view.StepAnimation(Model,.001f);view.Show(Model);elapsed=0;sampleBurn=false;
            view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out var origin);var target=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));var feet=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(new Vector2(0,-75)));
            fx.Play(origin,target,feet);
        }
        public void Step(float delta)
        {
            elapsed+=delta;if(!sampleBurn&&elapsed>=3.6f){sampleBurn=true;ApplyBurn.Invoke(Model,new object[]{3f,1});}
            Model.Tick(delta);view.StepAnimation(Model,delta);view.Show(Model);fx.Step(Model,delta);
        }
        void Update()
        {
            if(!autoplay||fx==null)return;Step(Mathf.Min(Time.unscaledDeltaTime,.05f));
            if(elapsed>.2f){frames++;if(fx.ActiveCount>32){activeFrames++;activeMs+=Time.unscaledDeltaTime*1000;}else if(fx.ActiveCount==0){idleCount++;idleMs+=Time.unscaledDeltaTime*1000;}if(draws.Valid)maxDraw=System.Math.Max(maxDraw,draws.LastValue);if(gc.Valid)maxGC=System.Math.Max(maxGC,gc.LastValue);}
            if(elapsed>8){Debug.Log("FIRE_PREVIEW_PERF frames="+frames+" activeMeanMs="+(activeFrames>0?activeMs/activeFrames:0)+" idleMeanMs="+(idleCount>0?idleMs/idleCount:0)+" maxDraw="+maxDraw+" drawCounter="+draws.Valid+" maxFrameGC="+maxGC+" peakImages="+fx.PeakImages);frames=activeFrames=idleCount=0;idleMs=activeMs=0;maxDraw=maxGC=0;dragonIndex=(dragonIndex+1)%dragons.Length;Restart();}
        }
        void OnDisable(){if(fx!=null)fx.Clear();if(controller!=null&&view!=null)controller.EndBattle();}
        void OnDestroy(){draws.Dispose();gc.Dispose();}
    }
}
