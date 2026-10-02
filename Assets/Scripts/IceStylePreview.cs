using UnityEngine;
namespace DragonTower
{
    // Only the standalone prototype scene contains this driver. Never added to Battle.
    public sealed class IceStylePreview : MonoBehaviour
    {
        public IceVfxStyle style;public DragonData[] dragons;
        [Range(0,1)]public int dragonIndex;
        [Range(0,2)]public int evolution=2;
        [Range(1,5)]public int visualIntensity=1;
        public bool autoplay=true;
        BattleController controller;BattleView view;IceStylePrototype fx;float elapsed;
        public IceStylePrototype Effect=>fx;
        void Start(){Initialize();Restart();}
        public void Initialize()
        {
            if(fx!=null)return;
            controller=FindFirstObjectByType<BattleController>();view=controller.view;controller.enabled=false;
            var screens=view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);
            fx=gameObject.AddComponent<IceStylePrototype>();fx.style=style;fx.Initialize(view.frame);
        }
        [ContextMenu("Restart style sample")]
        public void Restart()
        {
            if(!Application.isPlaying)return;Initialize();
            var dragon=dragons[Mathf.Clamp(dragonIndex,0,dragons.Length-1)];int level=evolution==0?1:evolution*10;
            controller.BeginBattle(dragon,dragon.Snapshot(level),BattleEnemyStats.Normal(),1,false,dragon.Snapshot(level).maxHP,null,level,dragon.skill);
            view.StepAnimation(controller.CurrentBattle,.001f);view.Show(controller.CurrentBattle);elapsed=0;
            view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out var origin);
            var feet=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(new Vector2(0,-75)));
            fx.VisualIntensity=visualIntensity;fx.Play(origin,new Vector2(116,-58),feet);
        }
        public void Step(float delta)
        {elapsed+=delta;view.StepAnimation(controller.CurrentBattle,delta);view.Show(controller.CurrentBattle);fx.Step(delta);}
        void Update(){if(!autoplay||fx==null)return;Step(Mathf.Min(Time.unscaledDeltaTime,.05f));if(elapsed>3.5f)Restart();}
        void OnDisable(){if(fx!=null)fx.Clear();}
    }
}
