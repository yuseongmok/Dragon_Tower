using System.Reflection;
using UnityEngine;
namespace DragonTower
{
    // Independent review only: alternate no-Slow and pre-existing-Slow scenarios.
    public sealed class LunarPreview:MonoBehaviour
    {
        public DragonData dragon;public SkillData skill;BattleController c;float age;int cycle;
        void Start(){c=FindFirstObjectByType<BattleController>();c.enabled=false;var screens=c.view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);Begin();}
        void Begin(){var s=dragon.Snapshot();s.skill=skill.Snapshot();s.criticalChance=0;s.passiveMechanic=DragonPassiveMechanic.None;var e=BattleEnemyStats.Normal();e.maxHP=99999;e.interval=100;c.BeginBattle(dragon,s,e,1,false,s.maxHP,null,20,skill);c.SendMessage("OnApplicationFocus",true);if(cycle%2==1)typeof(BattleModel).GetMethod("ApplySlow",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(c.CurrentBattle,new object[]{3f,60f});age=0;}
        void Update(){if(c==null)return;float dt=Mathf.Min(Time.unscaledDeltaTime,.05f),before=age;age+=dt;if(before<.5f&&age>=.5f)c.RequestSkill();var b=c.CurrentBattle;if(b!=null){b.Tick(dt);c.view.StepAnimation(b,dt);c.view.Show(b);}if(age>4.5f){cycle++;Begin();}}
        void OnDisable(){if(c!=null)c.EndBattle();}
    }
}
