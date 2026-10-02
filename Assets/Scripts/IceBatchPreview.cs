using UnityEngine;
namespace DragonTower
{
    // Only used by the independent comparison scene; invokes the normal battle API.
    public sealed class IceBatchPreview : MonoBehaviour
    {
        public DragonData[] dragons;public SkillData[] skills;
        BattleController controller;int index;float age;
        void Start(){controller=FindFirstObjectByType<BattleController>();controller.enabled=false;var screens=controller.view.frame.Find("Collection screens");if(screens!=null)screens.gameObject.SetActive(false);Begin();}
        void Begin(){var dragon=dragons[index/4%dragons.Length];var skill=skills[index%4];var stats=dragon.Snapshot();stats.skill=skill.Snapshot();stats.criticalChance=0;stats.passiveMechanic=DragonPassiveMechanic.None;var enemy=BattleEnemyStats.Normal();enemy.maxHP=9999;enemy.interval=100;controller.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,20,skill);controller.SendMessage("OnApplicationFocus",true);age=0;}
        void Update(){if(controller==null)return;float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);float before=age;age+=dt;if(before<.5f&&age>=.5f)controller.RequestSkill();var battle=controller.CurrentBattle;if(battle!=null){battle.Tick(dt);controller.view.StepAnimation(battle,dt);controller.view.Show(battle);}if(age>4){index++;Begin();}}
        void OnDisable(){if(controller!=null)controller.EndBattle();}
    }
}
