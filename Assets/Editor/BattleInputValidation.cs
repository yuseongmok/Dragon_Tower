using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class BattleInputValidation
    {
        static void Check(bool ok,string name){if(!ok)throw new Exception("INPUT: "+name);Debug.Log("INPUT_CHECK "+name);}
        public static void Validate(BattleView view,DragonData dragon,Camera camera)
        {
            var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();
            view.BindCombat(()=>controller.RequestAttack(),()=>controller.RequestSkill(),d=>controller.RequestDodge(d),()=>{},()=>controller.CanControl);
            controller.SendMessage("OnApplicationFocus",true);controller.BeginBattle(dragon);Canvas.ForceUpdateCanvases();
            view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;
            var surface=view.Gestures;var rect=(RectTransform)surface.transform;
            var raycaster=view.GetComponent<GraphicRaycaster>();
            PointerEventData Event(int id,Vector2 local)
            {
                var e=new PointerEventData(EventSystem.current){pointerId=id,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(local))};
                e.pointerPressRaycast=new RaycastResult{module=raycaster,gameObject=surface.gameObject};return e;
            }
            void Down(int id,Vector2 p)=>ExecuteEvents.Execute(surface.gameObject,Event(id,p),ExecuteEvents.pointerDownHandler);
            void Drag(int id,Vector2 p)=>ExecuteEvents.Execute(surface.gameObject,Event(id,p),ExecuteEvents.dragHandler);
            void Up(int id,Vector2 p)=>ExecuteEvents.Execute(surface.gameObject,Event(id,p),ExecuteEvents.pointerUpHandler);
            void Fresh(){controller.BeginBattle(dragon);}
            Check(view.attackButton==null&&view.dodgeButton==null,"Legacy buttons removed");
            Check(view.skillButton.GetComponent<RectTransform>().anchoredPosition.x==137,"Main skill within right thumb reach");
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(Event(-1,Vector2.zero),hits);
            Check(hits.Count>0&&hits[0].gameObject==surface.gameObject,"Combat graphics route to gesture surface");
            var skillEvent=Event(-1,Vector2.zero);skillEvent.position=RectTransformUtility.WorldToScreenPoint(camera,view.skillButton.transform.position);
            hits.Clear();EventSystem.current.RaycastAll(skillEvent,hits);
            Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==view.skillButton,"Skill button intercepts its own input");
            int hp=controller.CurrentBattle.EnemyHP;
            Down(-1,Vector2.zero);Up(-1,new Vector2(3,2));
            Check(controller.CurrentBattle.EnemyHP<hp,"Mouse tap attacks");
            hp=controller.CurrentBattle.EnemyHP;Down(-1,Vector2.zero);Up(-1,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP==hp,"Repeated tap respects attack cooldown");
            controller.CurrentBattle.Tick(.31f);Down(-1,Vector2.zero);Up(-1,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP<hp,"Attack available after original cooldown");
            foreach(int direction in new[]{-1,1})
            {
                Fresh();hp=controller.CurrentBattle.EnemyHP;
                Down(7,Vector2.zero);Drag(7,new Vector2(60*direction,2));Up(7,new Vector2(65*direction,2));
                Check(controller.CurrentBattle.DodgeReady>0&&controller.CurrentBattle.EnemyHP==hp,"Touch swipe only dodges "+direction);
                var motion=view.GetComponent<BattleAnimation>();var field=typeof(BattleAnimation).GetField("dodgeDirection",BindingFlags.NonPublic|BindingFlags.Instance);
                Check((int)field.GetValue(motion)==direction,"Dodge direction applied "+direction);
                Check(!controller.RequestDodge(-direction)&&(int)field.GetValue(motion)==direction,"Rejected dodge preserves direction");
            }
            Fresh();hp=controller.CurrentBattle.EnemyHP;Down(8,Vector2.zero);Up(8,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP<hp,"Touch tap attacks once");
            Fresh();hp=controller.CurrentBattle.EnemyHP;Down(8,Vector2.zero);Down(9,Vector2.zero);Up(9,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP==hp&&surface.Tracking,"Second finger ignored");Up(8,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP<hp,"First finger retained");
            Fresh();hp=controller.CurrentBattle.EnemyHP;Down(8,Vector2.zero);Drag(8,new Vector2(0,60));Up(8,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP==hp&&controller.CurrentBattle.DodgeReady==0,"Vertical gesture cannot attack or dodge");
            Down(8,Vector2.zero);Up(8,new Vector2(300,0));Check(controller.CurrentBattle.EnemyHP==hp,"Release outside cancels");
            Down(8,Vector2.zero);typeof(BattleGestureSurface).GetField("started",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(surface,Time.unscaledTime-1);Up(8,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP==hp,"Long press cannot attack");
            Down(8,Vector2.zero);view.resultPanel.SetActive(true);Up(8,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP==hp&&!controller.RequestSkill(),"Result overlay blocks combat");view.resultPanel.SetActive(false);
            var collection=view.frame.Find("Collection screens");if(collection!=null){collection.gameObject.SetActive(true);Check(!controller.RequestAttack()&&!controller.RequestDodge(1),"Collection blocks combat");collection.gameObject.SetActive(false);}
            Down(8,Vector2.zero);controller.SendMessage("OnApplicationFocus",false);Up(8,Vector2.zero);
            Check(controller.CurrentBattle.EnemyHP==hp&&!surface.Tracking,"Focus loss cancels pointer");controller.SendMessage("OnApplicationFocus",true);
            Fresh();view.skillButton.onClick.Invoke();Check(controller.CurrentBattle.SkillReady>0&&controller.CurrentBattle.AttackReady==0,"Skill button uses only skill logic");
            Fresh();controller.EndBattle();Check(!controller.RequestAttack()&&!controller.RequestDodge(1),"No battle blocks input");
            foreach(float scale in new[]{.5f,1.6f}){Fresh();view.frame.localScale=Vector3.one*scale;Down(8,Vector2.zero);Drag(8,new Vector2(50,0));Up(8,new Vector2(50,0));Check(controller.CurrentBattle.DodgeReady>0,"Swipe threshold uses UI units at scale "+scale);}
            view.frame.localScale=Vector3.one;Fresh();view.Show(controller.CurrentBattle);
            view.skillButton.onClick.Invoke();view.Show(controller.CurrentBattle);
            var charge=view.skillButton.transform.Find("Skill recharge").GetComponent<RectTransform>();
            Check(charge.rect.width<1,"Skill gauge starts empty on cooldown");
            controller.CurrentBattle.Tick(1);view.Show(controller.CurrentBattle);Check(charge.rect.width>0&&charge.rect.width<100,"Skill gauge progresses from model time");
            Fresh();view.Show(controller.CurrentBattle);Check(Mathf.Abs(charge.rect.width-100)<.1f,"New battle restores ready gauge");
            Check(!view.frame.Find("Pixel controls backing").gameObject.activeSelf&&!view.frame.Find("Pixel header").gameObject.activeSelf&&!view.frame.Find("Enemy card").gameObject.activeSelf,"Large panels hidden");
            Check(!view.frame.Find("Tap status").gameObject.activeSelf&&!view.frame.Find("Swipe status").gameObject.activeSelf,"Persistent gesture panels hidden");
            Check(((RectTransform)view.Gestures.transform).rect.height==592,"Expanded combat surface retains gesture handler");
            Check(view.enemyName.rectTransform.anchoredPosition.y==-72&&view.playerName.rectTransform.anchoredPosition.y==-716,"Compact name placement survives battle reset");
            Check(!view.itemButton1.enabled&&view.itemButton1.GetComponent<RectTransform>().rect.width==42,"Equipment is square display-only slot");
            Check(view.RecoveryButton!=null&&!view.RecoveryButton.interactable,"Recovery placeholder is unbound and disabled");
            var beforeHP=controller.CurrentBattle.PlayerHP;view.RecoveryButton.onClick.Invoke();Check(beforeHP==controller.CurrentBattle.PlayerHP,"Recovery placeholder cannot heal");
            int recoveryCalls=0;view.SetRecoverySlot(dragon.battleSprite,3,true,2,4);view.BindRecoveryAction(()=>recoveryCalls++);view.RecoveryButton.onClick.Invoke();Check(recoveryCalls==0&&!view.RecoveryButton.interactable,"Recovery UI cooldown blocks callback even after binding");
            view.SetRecoverySlot(dragon.battleSprite,3,true);view.RecoveryButton.onClick.Invoke();Check(recoveryCalls==1&&controller.CurrentBattle.PlayerHP==beforeHP,"Recovery extension only invokes supplied callback");
            view.BindRecoveryAction(null);view.SetRecoverySlot(null,0,false);
            var item=ScriptableObject.CreateInstance<ItemData>();item.icon=dragon.battleSprite;typeof(BattleView).GetMethod("SetEquipmentVisual",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(view,new object[]{0,item});
            Check(view.itemButton1.GetComponentsInChildren<Image>().Length>0,"Equipped item icon displayed");
            typeof(BattleView).GetMethod("SetEquipmentVisual",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(view,new object[]{0,null});UnityEngine.Object.DestroyImmediate(item);
            Debug.Log("BATTLE_INPUT_VALIDATION_OK");
        }
        public static void BuildWeb()
        {
            var report=UnityEditor.BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Battle.unity"},"../DragonTower-WebGL",BuildTarget.WebGL,BuildOptions.None);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("WebGL build failed");
            Debug.Log("HUD_WEBGL_BUILD_OK");
        }
    }
}
