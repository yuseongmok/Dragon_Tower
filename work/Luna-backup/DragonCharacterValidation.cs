using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class DragonCharacterValidation
    {
        static void Check(bool ok,string name){if(!ok)throw new Exception("CHARACTER: "+name);Debug.Log("CHARACTER_CHECK "+name);}
        public static void Validate(BattleView view,DragonData dragon,Camera camera,Action<string> capture,int stage=0,bool idleOnly=false)
        {
            int level=stage==0?1:stage*10;var output=capture;capture=name=>output(name.Replace("middle-",dragon.StableId+"-"+stage+"-"));
            var touchField=typeof(BattleGestureSurface).GetField("lastTouch",BindingFlags.NonPublic|BindingFlags.Instance);float previousTouch=(float)touchField.GetValue(view.Gestures);
            var set=dragon.LoadAnimationSet(stage);var basic=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            Check(set!=null&&set!=basic,"Independent character set");
            Check(dragon.signatureSkill==null,"Signature remains unplanned");
            foreach(DragonAnimationState state in Enum.GetValues(typeof(DragonAnimationState)))
            {
                if(idleOnly&&state!=DragonAnimationState.Idle)continue;
                var clip=set.Get(state);Check(clip.frames.Length==4&&clip.durations.SequenceEqual(basic.Get(state).durations),state+" four frames and original timing");
                Check(clip.frames.All(s=>!s.texture.isReadable&&s.texture.width<=512&&s.texture.filterMode==FilterMode.Point),state+" bounded Point texture, no CPU pixel copy");
            }
            var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();var player=view.playerArt.transform.Find("Pixel sprite").GetComponent<Image>();
            if(!idleOnly)Check(DragonProductionChecks.Validate(dragon,true).Count==0,"Character production data validates without designing a signature");
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
            view.Show(controller.CurrentBattle);
            var theme=ElementHudThemes.Get(dragon.elementType);
            var themeField=typeof(BattleView).GetField("hudTheme",BindingFlags.NonPublic|BindingFlags.Instance);
            Check(themeField.GetValue(view)==theme,"Existing element theme selected");
            var accent=(Image)typeof(BattleView).GetField("playerAccent",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(view);
            Check(accent.color==theme.secondaryColor,"Existing element accent applied");
            Check(view.playerArt.rectTransform.sizeDelta==new Vector2(256,256),"Existing actor rectangle preserved");
            Check(view.playerArt.GetComponentsInChildren<Image>().Count(x=>x.name=="Pixel sprite")==1,"Exactly one active character sprite across evolutions");
            for(int i=0;i<4;i++){view.StepAnimation(controller.CurrentBattle,i==0?.001f:set.idle.durations[i-1]);view.Show(controller.CurrentBattle);Check(player.sprite==set.idle.frames[i],"Middle Idle frame "+i);capture("middle-idle-"+i);}
            Check(set.anchors!=null,"Five visual anchors configured");
            foreach(DragonVisualAnchor kind in Enum.GetValues(typeof(DragonVisualAnchor)))
            {
                Check(view.TryGetCharacterAnchor(kind,view.frame,out var point),"Anchor resolved "+kind);
                var expected=(Vector2)view.frame.InverseTransformPoint(view.playerArt.rectTransform.TransformPoint(set.anchors.Get(kind)));
                Check(Vector2.Distance(point,expected)<.01f,"Anchor follows actor "+kind);
            }
            CaptureAnchors(view,output,dragon.StableId+"-"+stage+"-anchors",set);
            view.TryGetCharacterAnchor(DragonVisualAnchor.AttackOrigin,view.frame,out var beforeMove);
            view.playerArt.rectTransform.anchoredPosition=home+new Vector2(20,0);
            view.TryGetCharacterAnchor(DragonVisualAnchor.AttackOrigin,view.frame,out var afterMove);
            Check(Vector2.Distance(afterMove-beforeMove,new Vector2(20,0))<.01f,"Anchor follows actor movement without a VFX-specific offset");
            view.playerArt.rectTransform.anchoredPosition=home;
            if(idleOnly){controller.BeginBattle(dragon);touchField.SetValue(view.Gestures,previousTouch);Debug.Log("CHARACTER_IDLE_PILOT_OK");return;}
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
            foreach(var skill in new[]{dragon.skill})
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
            var weak=dragon.Snapshot(level);weak.maxHP=1;weak.passiveMechanic=DragonPassiveMechanic.None;var killer=BattleEnemyStats.Normal();killer.damage=9999;killer.interval=.01f;
            controller.BeginBattle(dragon,weak,killer,1,false,1,null,level,dragon.skill);var death=controller.CurrentBattle;
            for(int i=0;i<500&&death.Result==BattleResult.Fighting;i++)death.Tick(.02f);
            Check(death.Result==BattleResult.Defeat,"Middle existing HP/defeat logic");
            for(int i=0;i<4;i++){view.StepAnimation(death,i==0?.001f:set.death.durations[i-1]);view.Show(death);Check(set.death.frames.Contains(player.sprite),"Middle Death frame "+i);capture("middle-death-"+i);}
            view.StepAnimation(death,1);Check(player.sprite==set.death.frames[3],"Middle final death pose held");
            view.SetDragonArt(dragon,1);Check(player.sprite==dragon.LoadAnimationSet(0).idle.frames[0],"Return to base restores approved sprite");
            controller.BeginBattle(dragon);view.resultPanel.SetActive(false);view.CancelGesture();touchField.SetValue(view.Gestures,previousTouch);
            Debug.Log("CHARACTER_VALIDATION_OK "+dragon.StableId+" stage "+stage);
        }
        static void CaptureAnchors(BattleView view,Action<string> capture,string name,DragonAnimationSet set)
        {
            var markers=new System.Collections.Generic.List<GameObject>();int index=0;
            foreach(DragonVisualAnchor kind in Enum.GetValues(typeof(DragonVisualAnchor)))
            {
                view.TryGetCharacterAnchor(kind,view.frame,out var point);
                var go=new GameObject("Anchor debug",typeof(RectTransform),typeof(Image));markers.Add(go);
                var rect=go.GetComponent<RectTransform>();rect.SetParent(view.frame,false);rect.anchorMin=rect.anchorMax=view.frame.pivot;rect.anchoredPosition=point;rect.sizeDelta=new Vector2(6,6);
                go.GetComponent<Image>().color=new[]{Color.red,Color.cyan,Color.yellow,Color.magenta,Color.green}[index++];go.GetComponent<Image>().raycastTarget=false;
            }
            capture(name);foreach(var go in markers)UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
