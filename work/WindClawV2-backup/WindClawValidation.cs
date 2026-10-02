using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class WindClawValidation
    {
        static void Check(bool pass,string label){if(!pass)throw new Exception("CLAW: "+label);Debug.Log("CLAW_CHECK "+label);}
        public static void Validate(BattleView view,DragonData dragon,Action<string> capture)
        {
            var data=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/skill_wind_claw.asset");
            Check(data!=null&&data.rarity==ContentRarity.Unique&&data.icon!=null&&data.hitCount==2,"Independent Unique skill and crossed claw icon");
            Check((int)ContentRarity.Rare==2&&(int)ContentRarity.Legendary==4,"Existing serialized rarity values preserved");
            Check(Array.Exists(ContentDatabase.Load().skills,s=>s==data),"Skill registered in existing content database");
            var run=new TowerRun(dragon.maxHP,0,1,ElementType.Wind);run.ReplaceSkill(data,false);var stats=run.BuildBattleStats(dragon.Snapshot());stats.criticalChance=0;
            var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();var enemy=BattleEnemyStats.Normal();enemy.interval=100;enemy.maxHP=1000;
            controller.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,1,data);controller.SendMessage("OnApplicationFocus",true);
            view.BindCombat(()=>controller.RequestAttack(),()=>controller.RequestSkill(),d=>controller.RequestDodge(d),()=>{},()=>controller.CanControl);
            var battle=controller.CurrentBattle;var vfx=view.GetComponent<WindClawVfx>();var feedback=view.GetComponent<WindHitFeedback>();var times=new List<double>();int cues=0;
            battle.Cue+=(cue,damage)=>{if(cue==CombatCue.Skill||cue==CombatCue.SkillHit){times.Add(battle.Time);cues++;}};
            int objects=view.frame.GetComponentsInChildren<Transform>(true).Length;Vector2 hud=view.frame.anchoredPosition;
            for(int cast=0;cast<4;cast++)
            {
                int hp=battle.EnemyHP,before=cues;double start=battle.Time;times.Clear();view.skillButton.onClick.Invoke();
                Check(battle.EnemyHP==hp&&battle.SkillReady>start,"Button cast and data cooldown without premature damage");Check(!controller.RequestSkill(),"Cooldown blocks repeat input");bool cross=false;
                for(int i=0;i<70;i++)
                {
                    int prev=battle.EnemyHP;battle.Tick(.01f);view.StepAnimation(battle,.01f);view.Show(battle);
                    if(battle.EnemyHP<prev){Check(feedback.FlashVisible,"Flash synchronized with confirmed hit");Check(feedback.Holding==(cues-before==2),"Only final hit holds the actor presentation");}
                    cross|=vfx.CrossVisible;
                    if(cast==0&&i%2==0)capture("claw-"+(i/2).ToString("00"));
                }
                Check(cues-before==2&&vfx.HitsShown==2,"Exactly two damage cues and two visual contacts");
                for(int i=0;i<2;i++)Check(Math.Abs(times[i]-start-data.initialHitDelay-i*data.hitInterval)<.011,"Data-driven hit timing "+i);
                Check(hp-battle.EnemyHP==stats.skill.damage*2,"Damage equals existing two-hit calculation");Check(cross,"Both sets of three claw marks overlap after final hit");
                Check(!vfx.Active&&!feedback.Active&&!feedback.FlashVisible,"All effects and flashes expire");Check(view.frame.anchoredPosition==hud,"HUD does not shake");
                battle.Tick(6);view.StepAnimation(battle,6);view.Show(battle);
            }
            Check(view.frame.GetComponentsInChildren<Transform>(true).Length==objects,"Repeated casts keep a fixed object pool");
            Check(view.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text==data.displayName,"HUD binds equipped name");
            Check(Array.Exists(view.skillButton.GetComponentsInChildren<Image>(),im=>im.sprite==data.icon&&im.enabled),"HUD binds equipped claw icon");
            var position=view.enemyArt.rectTransform.anchoredPosition;feedback.Hit(true);feedback.Step(.08f,true,true);feedback.enabled=false;
            Check(view.enemyArt.rectTransform.anchoredPosition==position&&!feedback.FlashVisible,"Disable during recoil restores actor");feedback.enabled=true;
            var previewEnemy=BattleEnemyStats.Normal();previewEnemy.interval=100;controller.BeginBattle(dragon,stats,previewEnemy,1,false,stats.maxHP,null,1,data);view.Show(controller.CurrentBattle);capture("claw-ready");controller.RequestSkill();
            for(int i=0;i<70;i++){controller.CurrentBattle.Tick(.01f);view.StepAnimation(controller.CurrentBattle,.01f);view.Show(controller.CurrentBattle);if(i%2==0)capture("claw-"+(i/2).ToString("00"));}
            var weak=BattleEnemyStats.Normal();weak.maxHP=1;controller.BeginBattle(dragon,stats,weak,1,false,stats.maxHP,null,1,data);controller.RequestSkill();controller.CurrentBattle.Tick(.2f);view.StepAnimation(controller.CurrentBattle,.2f);
            Check(!vfx.Active&&!feedback.Active,"First-hit death clears second claw and feedback");
            controller.BeginBattle(dragon);Check(!vfx.Configured,"Switching back leaves default skill independent");
            Debug.Log("WIND_CLAW_VALIDATION_OK");
        }
    }
}
