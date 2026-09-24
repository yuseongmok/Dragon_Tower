using UnityEngine;
namespace DragonTower
{
    public class BattleController : MonoBehaviour
    {
        public DragonData[] dragons;
        public BattleView view;
        BattleModel battle;
        public BattleModel CurrentBattle => battle;
        public CollectionFlow Flow { get; private set; }
        bool focused=true;
        void Start()
        {
            Application.targetFrameRate=60;
            Flow=gameObject.AddComponent<CollectionFlow>();
            view.Bind(()=>{if(battle!=null)battle.Attack();},()=>{if(battle!=null)battle.Skill();},()=>{if(battle!=null)battle.Dodge();},()=>Flow.ResolveBattleResult());
            Flow.Initialize(this,view,dragons);
        }
        public void EndBattle() { battle=null; }
        public void BeginBattle(DragonData dragon) { BeginBattle(dragon,BattleEnemyStats.Normal(),1,false); }
        public void BeginBattle(DragonData dragon,BattleEnemyStats enemy,int floor,bool boss)
        { BeginBattle(dragon,dragon.Snapshot(),enemy,floor,boss,dragon.maxHP,null); }
        public void BeginBattle(DragonData dragon,BattleStats stats,BattleEnemyStats enemy,int floor,bool boss,int initialHP,UnityEngine.Sprite enemySprite=null,int dragonLevel=1,SkillData activeSkill=null)
        {
            battle=new BattleModel(stats,enemy,initialHP);
            view.SetDragonArt(dragon,dragonLevel);
            view.SetSkillPresentation(activeSkill??dragon.skill);
            view.SetEncounter(enemy,floor,boss,enemySprite);
            battle.Cue+=view.PlayCue;
            battle.Feedback+=text=>view.message.text=text;
            view.message.text="공격을 터치하세요 · 게이지가 차기 직전에 회피";
            view.Show(battle);
            view.restartButton.GetComponentInChildren<UnityEngine.UI.Text>().text="결과 확인";
        }
        void Update()
        {
            if(battle==null || !focused) return;
            // Returning from a hidden browser tab must not cause accumulated hits.
            float delta=Mathf.Min(Time.deltaTime,.1f);
            battle.Tick(delta);
            view.StepAnimation(battle,delta);
            view.Show(battle);
        }
        void OnApplicationFocus(bool value) { focused=value; }
        void OnApplicationPause(bool paused) { focused=!paused; }
    }
}

