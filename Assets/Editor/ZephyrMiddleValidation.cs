using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class ZephyrMiddleValidation
    {
        static void Check(bool ok,string name){if(!ok)throw new Exception("MIDDLE: "+name);Debug.Log("MIDDLE_CHECK "+name);}
        public static void Validate(BattleView view,DragonData dragon,Camera camera,Action<string> capture,int stage=1)
        {
            int level=stage*10;var output=capture;capture=name=>output(name.Replace("middle-",stage==2?"final-":"middle-"));
            var touchField=typeof(BattleGestureSurface).GetField("lastTouch",BindingFlags.NonPublic|BindingFlags.Instance);float previousTouch=(float)touchField.GetValue(view.Gestures);
            var set=dragon.LoadAnimationSet(stage);var basic=dragon.LoadAnimationSet(0);
            Check(set!=null&&set!=basic&&dragon.StableId=="zephyr","Same dragon ID, independent stage-1 set");
            Check(dragon.Form(stage).defaultSkill==null&&dragon.Form(stage).healthMultiplier==0&&dragon.Form(stage).attackMultiplier==0,"Existing skills and evolution balance preserved");
            Check(DragonProductionChecks.Validate(dragon).Count==0,"Production validation passes");
            foreach(DragonAnimationState state in Enum.GetValues(typeof(DragonAnimationState)))
            {
                var clip=set.Get(state);Check(clip.frames.Length==4&&clip.durations.SequenceEqual(basic.Get(state).durations),state+" four frames and original timing");
                Check(clip.frames.All(s=>!s.texture.isReadable&&s.texture.width<=512&&s.texture.filterMode==FilterMode.Point),state+" bounded Point texture, no CPU pixel copy");
            }
            var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();var player=view.playerArt.transform.Find("Pixel sprite").GetComponent<Image>();
            var run=new TowerRun(dragon.maxHP,0,1,dragon.elementType,dragon.alternateSkillElement,dragon.StableId,dragon);
            typeof(TowerRun).GetField("<Level>k__BackingField",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(run,level);
            typeof(TowerRun).GetMethod("RecalculateMaxHP",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(run,null);
            var stats=run.BuildBattleStats(dragon.Snapshot(level));stats.criticalChance=0;
            var enemy=BattleEnemyStats.Normal();enemy.maxHP=100000;enemy.interval=100;
            void Fresh(SkillData skill=null)
            {
                var selected=skill??dragon.skill;var s=run.BuildBattleStats(dragon.Snapshot(level));s.skill=selected.Snapshot();s.criticalChance=0;
                controller.BeginBattle(dragon,s,enemy,1,false,s.maxHP,null,level,selected);controller.SendMessage("OnApplicationFocus",true);
                view.BindCombat(()=>controller.RequestAttack(),()=>controller.RequestSkill(),d=>controller.RequestDodge(d),()=>{},()=>controller.CanControl);
            }
            controller.BeginBattle(dragon,stats,BattleEnemyStats.Normal(),1,false,stats.maxHP,null,level,dragon.skill);view.StepAnimation(controller.CurrentBattle,.001f);view.Show(controller.CurrentBattle);capture("middle-overview");
            Fresh();var home=view.playerArt.rectTransform.anchoredPosition;var hud=view.frame.anchoredPosition;
            Check(view.playerArt.rectTransform.sizeDelta==new Vector2(256,256),"Existing actor rectangle preserved");
            for(int i=0;i<4;i++){view.StepAnimation(controller.CurrentBattle,i==0?.001f:set.idle.durations[i-1]);view.Show(controller.CurrentBattle);Check(player.sprite==set.idle.frames[i],"Middle Idle frame "+i);capture("middle-idle-"+i);}
            foreach(var state in new[]{DragonAnimationState.Attack,DragonAnimationState.Skill,DragonAnimationState.Dodge,DragonAnimationState.Hit})
            {
                Fresh();if(state==DragonAnimationState.Attack)controller.RequestAttack();else if(state==DragonAnimationState.Skill)controller.RequestSkill();else if(state==DragonAnimationState.Dodge)controller.RequestDodge(-1);else view.PlayCue(CombatCue.EnemyHit,10);
                var clip=set.Get(state);
                for(int i=0;i<4;i++){view.StepAnimation(controller.CurrentBattle,i==0?.001f:clip.durations[i-1]);view.Show(controller.CurrentBattle);Check(clip.frames.Contains(player.sprite),"Middle "+state+" uses own sprite "+i);capture("middle-"+state.ToString().ToLower()+"-"+i);}
            }
            var rect=(RectTransform)view.Gestures.transform;var raycaster=view.GetComponent<GraphicRaycaster>();
            PointerEventData E(int id,Vector2 local)=>new PointerEventData(EventSystem.current){pointerId=id,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(local)),pointerPressRaycast=new RaycastResult{module=raycaster,gameObject=view.Gestures.gameObject}};
            foreach(int id in new[]{-1,7})
            {
                Fresh();int hp=controller.CurrentBattle.EnemyHP;ExecuteEvents.Execute(view.Gestures.gameObject,E(id,Vector2.zero),ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(view.Gestures.gameObject,E(id,Vector2.zero),ExecuteEvents.pointerUpHandler);
                Check(controller.CurrentBattle.EnemyHP<hp,"Middle mouse/touch tap "+id);
                foreach(int dir in new[]{-1,1}){Fresh();ExecuteEvents.Execute(view.Gestures.gameObject,E(id,Vector2.zero),ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(view.Gestures.gameObject,E(id,new Vector2(dir*70,0)),ExecuteEvents.dragHandler);ExecuteEvents.Execute(view.Gestures.gameObject,E(id,new Vector2(dir*70,0)),ExecuteEvents.pointerUpHandler);Check(controller.CurrentBattle.DodgeReady>0,"Middle swipe "+id+"/"+dir);}
            }
            foreach(var skill in dragon.elementSkillPool.skills.Where(s=>s.StableId!="skill_storm_slash").Concat(new[]{dragon.signatureSkill}))
            {
                Fresh(skill);var battle=controller.CurrentBattle;int hits=0;battle.Cue+=(cue,damage)=>{if(cue==CombatCue.Skill||cue==CombatCue.SkillHit)hits++;};int objects=view.frame.GetComponentsInChildren<Transform>(true).Length;
                for(int cast=0;cast<3;cast++)
                {
                    int hp=battle.EnemyHP,before=hits;view.skillButton.onClick.Invoke();Check(battle.SkillReady>battle.Time&&!controller.RequestSkill(),"Middle skill button / cooldown: "+skill.displayName);
                    for(int f=0;f<75;f++){battle.Tick(.02f);view.StepAnimation(battle,.02f);view.Show(battle);if(cast==0&&f%2==0)capture("middle-"+skill.StableId+"-"+(f/2).ToString("00"));}
                    Check(hits-before==skill.hitCount&&hp-battle.EnemyHP==skill.damage*skill.hitCount,"Middle original damage and hit count: "+skill.displayName);
                    Check(view.frame.anchoredPosition==hud&&view.playerArt.rectTransform.anchoredPosition==home,"Middle skill restores actor/HUD: "+skill.displayName);
                    battle.Tick(skill.cooldown+.1f);view.StepAnimation(battle,skill.cooldown+.1f);view.Show(battle);
                }
                Check(view.frame.GetComponentsInChildren<Transform>(true).Length==objects,"No extra renderers/pool growth: "+skill.displayName);
                Check(view.skillButton.transform.Find("Equipped skill name").GetComponent<Text>().text==skill.displayName,"Middle HUD reads existing skill: "+skill.displayName);
            }
            Fresh(dragon.signatureSkill);controller.RequestSkill();Check(controller.CurrentBattle.ProtectedSkillActive,"Middle Dantian retains cast protection");controller.EndBattle();Check(!view.GetComponent<DantianVfx>().Active,"Middle Dantian cancellation clears effect");
            var weak=dragon.Snapshot(level);weak.maxHP=1;weak.passiveMechanic=DragonPassiveMechanic.None;var killer=BattleEnemyStats.Normal();killer.damage=9999;killer.interval=.01f;
            controller.BeginBattle(dragon,weak,killer,1,false,1,null,level,dragon.skill);var death=controller.CurrentBattle;
            for(int i=0;i<500&&death.Result==BattleResult.Fighting;i++)death.Tick(.02f);
            Check(death.Result==BattleResult.Defeat,"Middle existing HP/defeat logic");
            for(int i=0;i<4;i++){view.StepAnimation(death,i==0?.001f:set.death.durations[i-1]);view.Show(death);Check(set.death.frames.Contains(player.sprite),"Middle Death frame "+i);capture("middle-death-"+i);}
            view.StepAnimation(death,1);Check(player.sprite==set.death.frames[3],"Middle final death pose held");
            view.SetDragonArt(dragon,1);Check(player.sprite==basic.idle.frames[0],"Return to base restores approved sprite");
            controller.BeginBattle(dragon);view.resultPanel.SetActive(false);view.CancelGesture();touchField.SetValue(view.Gestures,previousTouch);
            Debug.Log(stage==2?"ZEPHYR_FINAL_VALIDATION_OK":"ZEPHYR_MIDDLE_VALIDATION_OK");
        }
    }
}
