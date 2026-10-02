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
        int requestedDodgeDirection=-1;
        public bool CanControl=>focused&&battle!=null&&battle.Result==BattleResult.Fighting&&view.IsBattleUncovered;
        public bool RequestAttack()=>CanControl&&battle.Attack();
        public bool RequestSkill()=>CanControl&&battle.Skill();
        public bool RequestDodge(int direction)
        {
            if(!CanControl)return false;
            requestedDodgeDirection=direction>0?1:-1;
            try{return battle.Dodge();}finally{requestedDodgeDirection=-1;}
        }
        void HandleCue(CombatCue cue,int damage)
        {
            if(cue==CombatCue.Dodge)view.SetDodgeDirection(requestedDodgeDirection);
            view.PlayCue(cue,damage);
        }
        void Start()
        {
            Application.targetFrameRate=60;
            Flow=gameObject.AddComponent<CollectionFlow>();
            view.BindCombat(()=>RequestAttack(),()=>RequestSkill(),direction=>RequestDodge(direction),()=>Flow.ResolveBattleResult(),()=>CanControl);
            view.BindItems(()=>UseItem(0),()=>UseItem(1),()=>UseItem(2));
            Flow.Initialize(this,view,dragons);
        }
        public void EndBattle() { view.CancelGesture();battle=null; }
        public void BeginBattle(DragonData dragon) { BeginBattle(dragon,BattleEnemyStats.Normal(),1,false); }
        public void BeginBattle(DragonData dragon,BattleEnemyStats enemy,int floor,bool boss)
        { BeginBattle(dragon,dragon.Snapshot(),enemy,floor,boss,dragon.maxHP,null); }
        public void BeginBattle(DragonData dragon,BattleStats stats,BattleEnemyStats enemy,int floor,bool boss,int initialHP,UnityEngine.Sprite enemySprite=null,int dragonLevel=1,SkillData activeSkill=null)
        {
            enemy.floor=floor;
            view.CancelGesture();
            battle=new BattleModel(stats,enemy,initialHP);
            view.SetDragonArt(dragon,dragonLevel);
            view.SetSkillPresentation(activeSkill??dragon.skill);
            view.SetEncounter(enemy,floor,boss,enemySprite);
            var run=Flow==null?null:Flow.CurrentRun;
            view.SetRunProgress(run==null?dragonLevel:run.Level,run==null?0:run.Experience,run==null?100:run.ExperienceToNext);
            battle.Cue+=HandleCue;
            battle.Feedback+=text=>view.message.text=text;
            view.message.text=battle.AreaHint;
            view.Show(battle);
            view.SetItems(Flow==null?null:Flow.CurrentRun);
            view.restartButton.GetComponentInChildren<UnityEngine.UI.Text>().text="결과 확인";
        }
        void UseItem(int index)
        {
            var run=Flow==null?null:Flow.CurrentRun;var item=run==null?null:run.ItemAt(index);
            if(battle==null||item==null||!battle.UseItem(item))return;
            run.ConsumeItem(index);view.SetItems(run);view.Show(battle);
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
        void OnApplicationFocus(bool value) { focused=value;if(!value)view.CancelGesture(); }
        void OnApplicationPause(bool paused) { focused=!paused;if(paused)view.CancelGesture(); }
    }
}

