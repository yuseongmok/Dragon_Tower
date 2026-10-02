using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class GaleSlashValidation
    {
        static void Check(bool value,string name){if(!value)throw new Exception("GALESLASH: "+name);Debug.Log("GALESLASH_CHECK "+name);}
        public static void Validate(BattleView view,DragonData dragon,Action<string> capture)
        {
            var data=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/skill_gale_slash.asset");
            Check(data.displayName=="질풍참"&&data.rarity==ContentRarity.Rare&&data.icon!=null,"Rare wind blade data and icon installed");
            var run=new TowerRun(dragon.maxHP,0,1,ElementType.Wind);run.ReplaceSkill(data,false);var stats=run.BuildBattleStats(dragon.Snapshot());stats.criticalChance=0;
            Check(Math.Abs(stats.skill.initialHitDelay-data.initialHitDelay)<.0001f,"Equipped skill preserves initial travel delay");
            var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();var enemy=BattleEnemyStats.Normal();enemy.interval=100;
            controller.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,1,data);controller.SendMessage("OnApplicationFocus",true);
            view.BindCombat(()=>controller.RequestAttack(),()=>controller.RequestSkill(),d=>controller.RequestDodge(d),()=>{},()=>controller.CanControl);
            var battle=controller.CurrentBattle;var times=new List<double>();int total=0;int cues=0;
            battle.Cue+=(cue,damage)=>{if(cue==CombatCue.Skill||cue==CombatCue.SkillHit){times.Add(battle.Time);total+=damage;cues++;}};
            var vfx=view.GetComponent<GaleSlashVfx>();int count=view.frame.GetComponentsInChildren<Transform>(true).Length;
            for(int cast=0;cast<4;cast++)
            {
                int hp=battle.EnemyHP,before=cues;double start=battle.Time;times.Clear();view.skillButton.onClick.Invoke();
                Check(battle.EnemyHP==hp&&battle.SkillReady>start,"Button starts cast without early damage");Check(!controller.RequestSkill(),"Cooldown rejects extra cast");
                for(int i=0;i<60;i++){battle.Tick(.01f);view.StepAnimation(battle,.01f);view.Show(battle);if(cast==0&&i%2==0)capture("galeslash-"+(i/2).ToString("00"));}
                Check(cues-before==1,"Exactly one confirmed hit");
                for(int i=0;i<1;i++)Check(Math.Abs(times[i]-start-data.initialHitDelay-i*data.hitInterval)<.011,"Hit arrival timing "+i);
                Check(hp-battle.EnemyHP==stats.skill.damage,"Single data-driven damage");
                Check(!vfx.Active,"VFX expires");battle.Tick(5.1f);view.StepAnimation(battle,5.1f);view.Show(battle);
            }
            Check(view.frame.GetComponentsInChildren<Transform>(true).Length==count,"Repeated casts reuse fixed pool");
            Check(view.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text==data.displayName,"HUD reads current skill name");
            Check(Array.Exists(view.skillButton.GetComponentsInChildren<Image>(),im=>im.sprite==data.icon&&im.enabled),"HUD reads current skill icon");
            var weak=BattleEnemyStats.Normal();weak.maxHP=1;controller.BeginBattle(dragon,stats,weak,1,false,stats.maxHP,null,1,data);controller.RequestSkill();controller.CurrentBattle.Tick(.3f);view.StepAnimation(controller.CurrentBattle,.3f);Check(!vfx.Active,"Lethal hit clears pending visual blades");
            controller.BeginBattle(dragon);Check(!vfx.Configured,"Switch back preserves independent Gale Strike");
            Debug.Log("GALE_SLASH_VALIDATION_OK");
        }
    }
}

