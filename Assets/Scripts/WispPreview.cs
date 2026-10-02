using UnityEngine;
namespace DragonTower
{
    // Isolated review scene only. Normal battle scene and rewards are unchanged.
    public sealed class WispPreview : MonoBehaviour
    {
        public DragonData dragon;public SkillData skill;BattleController c;float age;int cycle;
        void Start(){c=FindFirstObjectByType<BattleController>();c.enabled=false;var screens=c.view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);Begin();}
        void Begin(){var stats=dragon.Snapshot();stats.skill=skill.Snapshot();stats.criticalChance=0;stats.passiveMechanic=DragonPassiveMechanic.None;var enemy=BattleEnemyStats.Normal();enemy.maxHP=99999;enemy.interval=100;c.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,20,skill);c.SendMessage("OnApplicationFocus",true);age=0;}
        void Update(){if(c==null)return;float dt=Mathf.Min(Time.unscaledDeltaTime,.05f),before=age;age+=dt;if(before<.5f&&age>=.5f)c.RequestSkill();if(age>3&&age<5.7f&&(int)(before*4)!=(int)(age*4))c.RequestDodge((int)(age*4)%2==0?1:-1);var b=c.CurrentBattle;if(b!=null){b.Tick(dt);c.view.StepAnimation(b,dt);c.view.Show(b);}if(age>8){cycle++;Begin();}}
        void OnDisable(){if(c!=null)c.EndBattle();}
    }
}
