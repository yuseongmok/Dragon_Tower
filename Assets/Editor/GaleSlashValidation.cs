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
            var vfx=view.GetComponent<GaleSlashVfx>();var feedback=view.GetComponent<GaleSlashFeedback>();int count=view.frame.GetComponentsInChildren<Transform>(true).Length;
            for(int cast=0;cast<4;cast++)
            {
                int hp=battle.EnemyHP,before=cues;double start=battle.Time;times.Clear();view.skillButton.onClick.Invoke();
                Check(battle.EnemyHP==hp&&battle.SkillReady>start,"Button starts cast without early damage");Check(!controller.RequestSkill(),"Cooldown rejects extra cast");
                float sinceHit=-1;bool checkedContact=false,checkedNumber=false;
                for(int i=0;i<60;i++)
                {
                    int beforeHp=battle.EnemyHP;battle.Tick(.01f);view.StepAnimation(battle,.01f);view.Show(battle);
                    if(battle.EnemyHP<beforeHp){sinceHit=0;Check(feedback.FlashVisible&&feedback.Holding,"Contact flash and local stop coincide with real damage");checkedContact=true;}
                    else if(sinceHit>=0)sinceHit+=.01f;
                    bool numberVisible=Array.Exists(view.frame.GetComponentsInChildren<Text>(true),t=>t.name.StartsWith("Damage ")&&t.gameObject.activeSelf&&t.text==stats.skill.damage+"!");
                    if(sinceHit>=0&&sinceHit<.045f)Check(!numberVisible,"Damage number does not cover initial contact");
                    if(sinceHit>=.08f&&!checkedNumber){Check(numberVisible,"Damage number appears after contact");checkedNumber=true;}
                    if(cast==0&&i%2==0)capture("galeimpact-"+(i/2).ToString("00"));
                }
                Check(checkedContact&&checkedNumber&&!feedback.Active&&!feedback.FlashVisible,"Feedback completes and silhouette is removed");
                Check(cues-before==1,"Exactly one confirmed hit");
                for(int i=0;i<1;i++)Check(Math.Abs(times[i]-start-data.initialHitDelay-i*data.hitInterval)<.011,"Hit arrival timing "+i);
                Check(hp-battle.EnemyHP==stats.skill.damage,"Single data-driven damage");
                Check(!vfx.Active,"VFX expires");battle.Tick(5.1f);view.StepAnimation(battle,5.1f);view.Show(battle);
            }
            Check(view.frame.GetComponentsInChildren<Transform>(true).Length==count,"Repeated casts reuse fixed pool");
            Check(view.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text==data.displayName,"HUD reads current skill name");
            Check(Array.Exists(view.skillButton.GetComponentsInChildren<Image>(),im=>im.sprite==data.icon&&im.enabled),"HUD reads current skill icon");
            var beforeRecoil=view.enemyArt.rectTransform.anchoredPosition;float timeScale=Time.timeScale;double ready=battle.SkillReady;
            feedback.Hit();feedback.Step(.08f,true,true);Check(view.enemyArt.rectTransform.anchoredPosition!=beforeRecoil,"Visual recoil applied");feedback.enabled=false;Check(view.enemyArt.rectTransform.anchoredPosition==beforeRecoil&&!feedback.FlashVisible,"Disable during recoil restores position and silhouette");feedback.enabled=true;Check(Time.timeScale==timeScale&&battle.SkillReady==ready,"Local feedback leaves global time and cooldown unchanged");
            var weak=BattleEnemyStats.Normal();weak.maxHP=1;controller.BeginBattle(dragon,stats,weak,1,false,stats.maxHP,null,1,data);controller.RequestSkill();controller.CurrentBattle.Tick(.3f);view.StepAnimation(controller.CurrentBattle,.3f);Check(!vfx.Active,"Lethal hit clears pending visual blades");
            vfx.enabled=false;feedback.enabled=false;Check(!feedback.FlashVisible&&!feedback.Active,"Disable clears presentation offsets and flash");vfx.enabled=true;feedback.enabled=true;
            controller.BeginBattle(dragon);Check(!vfx.Configured,"Switch back preserves independent Gale Strike");
            Debug.Log("GALE_SLASH_VALIDATION_OK");
        }
    }
}

