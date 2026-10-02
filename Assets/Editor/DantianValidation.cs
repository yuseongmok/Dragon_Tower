using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class DantianValidation
    {
        static void Check(bool ok,string name){if(!ok)throw new Exception("DANTIAN: "+name);Debug.Log("DANTIAN_CHECK "+name);}
        static void Call(BattleModel model,string method)=>typeof(BattleModel).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(model,null);
        public static void Validate(BattleView view,DragonData dragon,Action<string> capture)
        {
            var data=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/skill_dantian.asset");
            Check(data!=null&&data.rarity==ContentRarity.Legendary&&data.icon!=null,"Legendary data and icon");
            Check(data.CanEquip("zephyr")&&!data.CanEquip("other")&&data.rewardEligibilityPercent==.5f,"Exclusive eligibility and rare draft gate");
            var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");
            foreach(var existing in db.skills)if(existing!=null&&existing!=data)Check(existing.protectedCastDuration==0&&existing.rewardEligibilityPercent==100,"Existing skill defaults preserved");
            var run=new TowerRun(dragon.maxHP,0,1,ElementType.Wind,ElementType.Neutral,dragon.StableId);run.ReplaceSkill(data,false);
            var stats=run.BuildBattleStats(dragon.Snapshot());stats.criticalChance=0;stats.passiveMechanic=DragonPassiveMechanic.None;
            Check(stats.skill.protectedCastDuration==1.3f&&stats.speciesId=="zephyr","Protection survives run snapshot");
            var other=new TowerRun(100,0,1,ElementType.Wind,ElementType.Neutral,"other");bool denied=false;try{other.ReplaceSkill(data,false);}catch(InvalidOperationException){denied=true;}Check(denied,"Other Wind dragons cannot equip signature");
            var enemy=BattleEnemyStats.Normal();enemy.maxHP=10000;enemy.interval=.2f;
            var model=new BattleModel(stats,enemy);int hp=model.PlayerHP;Check(model.Skill()&&model.ProtectedSkillActive,"Protection begins at accepted cast");
            Check(!model.Attack()&&!model.Dodge()&&!model.Skill(),"Cinematic cannot be interrupted by repeated input");
            double ready=model.SkillReady;model.SkillReady=0;Check(!model.Skill(),"Cooldown reset cannot overlap protected casts");model.SkillReady=ready;
            model.Tick(.44f);Check(model.EnemyHP==10000&&model.PlayerHP==hp,"Cut/silence has no early damage and is invulnerable");
            model.Tick(.02f);Check(model.EnemyHP==10000-stats.skill.damage,"Damage occurs at rupture");
            model.Tick(.83f);Check(model.PlayerHP==hp&&model.ProtectedSkillActive,"Enemy attacks blocked through recovery");model.Tick(.32f);Check(!model.ProtectedSkillActive&&model.PlayerHP<hp,"Normal damage resumes after protection");
            Check(Math.Abs(model.SkillReady-data.cooldown)<.0001,"Protection does not alter cooldown");
            enemy=BattleEnemyStats.Normal();enemy.maxHP=10000;enemy.interval=100;enemy.floor=21;enemy.bossPattern=EnemyBossPattern.GarudaRend;
            model=new BattleModel(stats,enemy);Call(model,"ApplyAreaHitEffects");hp=model.PlayerHP;model.Skill();model.Tick(1.29f);Check(model.PlayerHP==hp,"Existing burn and rend ticks blocked");model.Tick(.22f);Check(model.PlayerHP<hp,"Existing DOT resumes instead of being cleansed");
            enemy=BattleEnemyStats.Normal();enemy.maxHP=10000;enemy.interval=100;enemy.bossPattern=EnemyBossPattern.UrielReflection;enemy.specialPatternDuration=3;
            model=new BattleModel(stats,enemy);model.Tick(6.01f);Check(model.EnemyReflecting,"Reflection active in test");hp=model.PlayerHP;model.Skill();model.Tick(.50f);Check(model.PlayerHP==hp,"Rupture reflection blocked");model.Tick(.81f);model.Attack();Check(model.PlayerHP<hp,"Reflection resumes after protection");
            enemy.bossPattern=EnemyBossPattern.AzazelSeals;model=new BattleModel(stats,enemy);model.Tick(5.8f);model.Skill();model.Tick(.5f);Check(!model.PlayerSkillSealed&&model.EnemyHP<10000,"Timed seal cannot cancel protected cast");
            enemy.bossPattern=EnemyBossPattern.ForgeBarrier;model=new BattleModel(stats,enemy);model.Skill();hp=model.PlayerHP;Call(model,"ActivateForgeBarrier");Call(model,"ResolveForgeBarrierFailure");Check(model.PlayerHP==hp,"Barrier explosion blocked");
            model=new BattleModel(stats,enemy);model.Skill();model.Tick(.2f);model.CancelProtectedSkill();model.Tick(.5f);Check(!model.ProtectedSkillActive&&model.EnemyHP==10000,"Cancellation removes protection and pending damage");
            var wrong=dragon.Snapshot();wrong.skill=data.Snapshot();wrong.speciesId="other";Check(!new BattleModel(wrong,enemy).Skill(),"Model also enforces species restriction");
            var normal=new BattleModel(dragon.Snapshot(),enemy);normal.Skill();Check(!normal.ProtectedSkillActive,"Existing Common skill has no new immunity");
            enemy.bossPattern=EnemyBossPattern.SentryParalysis;model=new BattleModel(stats,enemy,stats.maxHP,()=>0);model.Skill();Check(!model.ProtectedSkillActive,"Failed paralyzed cast grants no invulnerability");

            var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();var vfx=view.GetComponent<DantianVfx>();var feedback=view.GetComponent<DantianFeedback>();
            var arena=view.frame.Find("Arena").GetComponent<Image>();var material=arena.material;float timeScale=Time.timeScale;
            var framePosition=view.frame.anchoredPosition;int objects=view.frame.GetComponentsInChildren<Transform>(true).Length;
            for(int cast=0;cast<3;cast++)
            {
                enemy=BattleEnemyStats.Normal();enemy.maxHP=1000;enemy.interval=.4f;
                controller.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,1,data);controller.SendMessage("OnApplicationFocus",true);
                view.BindCombat(()=>controller.RequestAttack(),()=>controller.RequestSkill(),d=>controller.RequestDodge(d),()=>{},()=>controller.CanControl);
                var b=controller.CurrentBattle;hp=b.PlayerHP;int hits=0;double hitTime=-1;b.Cue+=(cue,damage)=>{if(cue==CombatCue.Skill){hits++;hitTime=b.Time;}};
                view.skillButton.onClick.Invoke();Check(b.ProtectedSkillActive&&vfx.Active,"Real HUD button starts cinematic and immunity");
                for(int i=0;i<70;i++)
                {
                    b.Tick(.02f);view.StepAnimation(b,.02f);view.Show(b);
                    if(b.ProtectedSkillActive)Check(b.PlayerHP==hp,"HP protected while overlay active");
                    if(i==20)Check(!feedback.Active&&!vfx.Rupturing,"Silence has no impact response");
                    if(i==23)Check(feedback.FlashVisible&&feedback.Holding&&vfx.Rupturing,"Rupture flash/local hold synchronized");
                    if(cast==0)capture("dantian-"+i.ToString("00"));
                }
                Check(hits==1&&Math.Abs(hitTime-.45)<.021,"Exactly one data-timed damage event");
                Check(!b.ProtectedSkillActive&&vfx.Clean&&arena.material==material,"Completion restores background and immunity");
                Check(view.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text=="단천","Existing HUD reads signature name");
            }
            Check(objects==view.frame.GetComponentsInChildren<Transform>(true).Length,"Repeated casts reuse objects");
            Check(Time.timeScale==timeScale&&framePosition==view.frame.anchoredPosition,"Global time and HUD position unchanged");
            for(int scenario=0;scenario<8;scenario++)
            {
                controller.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,1,data);controller.RequestSkill();var b=controller.CurrentBattle;
                float elapsed=scenario<4?.2f:.5f;b.Tick(elapsed);view.StepAnimation(b,elapsed);
                if(scenario%4==0)vfx.enabled=false;else if(scenario%4==1)vfx.Overlay.gameObject.SetActive(false);else if(scenario%4==2)controller.EndBattle();else controller.BeginBattle(dragon);
                Check(!b.ProtectedSkillActive&&vfx.Clean&&arena.material==material,"Cancel/disable/exit cleanup "+scenario);vfx.enabled=true;
            }
            enemy.maxHP=1;controller.BeginBattle(dragon,stats,enemy,1,false,stats.maxHP,null,1,data);controller.RequestSkill();controller.CurrentBattle.Tick(.46f);view.StepAnimation(controller.CurrentBattle,.46f);
            Check(vfx.Clean&&!controller.CurrentBattle.ProtectedSkillActive&&!feedback.FlashVisible,"Lethal hit safely removes cinematic");
            controller.BeginBattle(dragon);
            // Scene teardown can destroy actors/overlay before their owning component.
            var temp=new GameObject("Dantian teardown check");var detached=temp.AddComponent<DantianVfx>();detached.Initialize(view);detached.Configure(data,view.playerArt);bool canceled=false;detached.Cancelled=()=>canceled=true;
            detached.Cast(new Vector2(-26,-532),new Vector2(30,-264));UnityEngine.Object.DestroyImmediate(detached.Overlay.gameObject);UnityEngine.Object.DestroyImmediate(temp);
            Check(canceled&&arena.material==material,"Destroyed overlay restores background before owner destruction");
            temp=new GameObject("Dantian actor teardown check");var local=temp.AddComponent<DantianFeedback>();local.Initialize();
            var p=new GameObject("test player",typeof(RectTransform),typeof(Image));var e=new GameObject("test enemy",typeof(RectTransform),typeof(Image));
            local.Bind(p.GetComponent<RectTransform>(),e.GetComponent<RectTransform>(),p.GetComponent<Image>(),e.GetComponent<Image>());local.Hit();local.Step(.02f,true,true);
            UnityEngine.Object.DestroyImmediate(p);UnityEngine.Object.DestroyImmediate(e);local.Clear();UnityEngine.Object.DestroyImmediate(temp);
            Debug.Log("DANTIAN_VALIDATION_OK");
        }
    }
}
