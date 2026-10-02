using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace DragonTower
{
    public partial class BattleView
    {
        public BattleGestureSurface Gestures {get;private set;}
        Text tapStatus,swipeStatus;
        UnityAction arenaAttack;Action<int> arenaDodge;Func<bool> controlsAllowed;
        public bool IsBattleUncovered
        {
            get { var collection=frame==null?null:frame.Find("Collection screens");return frame!=null&&frame.gameObject.activeInHierarchy&&!resultPanel.activeSelf&&(collection==null||!collection.gameObject.activeSelf); }
        }
        void EnsureFinalControls()
        {
            if(frame==null||skillButton==null)return;
            RemoveLegacy(ref attackButton);RemoveLegacy(ref dodgeButton);attackLabel=dodgeLabel=null;
            MoveControl(skillButton,160,783,100,100);
            skillLabel.rectTransform.anchorMin=Vector2.zero;skillLabel.rectTransform.anchorMax=Vector2.one;
            skillLabel.rectTransform.offsetMin=new Vector2(8,4);skillLabel.rectTransform.offsetMax=new Vector2(-8,-4);skillLabel.fontSize=20;
            if(tapStatus==null)
            {
                var left=Panel("Tap status",frame,-165,790,120,64,new Color(.035f,.09f,.11f,.96f));
                DragonTowerTheme.Frame(left,new Color(.20f,.47f,.43f));
                tapStatus=Label("탭 공격\n준비",left.transform,0,32,112,56,14,new Color(.69f,.93f,.83f));
                var right=Panel("Swipe status",frame,165,790,120,64,new Color(.035f,.09f,.11f,.96f));
                DragonTowerTheme.Frame(right,new Color(.20f,.47f,.43f));
                swipeStatus=Label("좌우 회피\n준비",right.transform,0,32,112,56,14,new Color(.69f,.93f,.83f));
                // Place controls below overlays even when created after the serialized scene loads.
                left.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());right.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
                var hint=Label("전투 영역 탭 · 공격    좌우 스와이프 · 회피",frame,0,836,450,20,12,new Color(.61f,.80f,.75f));hint.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());hint.gameObject.SetActive(false);
                if(dragonInfo!=null)dragonInfo.gameObject.SetActive(false);
                var line=Panel("Mint control divider",frame,0,708,400,2,new Color(.2f,.52f,.45f));line.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());line.gameObject.SetActive(false);
            }
            if(Gestures==null)
            {
                var surface=Panel("Battle gesture area",frame,0,404,480,592,Color.clear);surface.raycastTarget=true;
                // Above decorative combat graphics, below result/collection and all controls.
                surface.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
                Gestures=surface.gameObject.AddComponent<BattleGestureSurface>();
                Gestures.CanInteract=()=>IsBattleUncovered&&controlsAllowed!=null&&controlsAllowed();
                Gestures.Tap=AttackFromArena;Gestures.Swipe=DodgeFromArena;
            }
        }
        static void RemoveLegacy(ref Button button)
        {
            if(button==null)return;var go=button.gameObject;go.SetActive(false);
            if(Application.isPlaying)Destroy(go);else DestroyImmediate(go);button=null;
        }
        public void AttackFromArena(){if(IsBattleUncovered&&controlsAllowed!=null&&controlsAllowed())arenaAttack?.Invoke();}
        public void DodgeFromArena(int direction){if(IsBattleUncovered&&controlsAllowed!=null&&controlsAllowed())arenaDodge?.Invoke(direction);}
        public void SetDodgeDirection(int direction){if(motion!=null)motion.SetDodgeDirection(direction);}
        public void CancelGesture(){if(Gestures!=null)Gestures.Cancel();}
        public void BindCombat(UnityAction attack,UnityAction skill,Action<int> dodge,UnityAction restart,Func<bool> allowed)
        {
            EnsureFinalControls();arenaAttack=attack;arenaDodge=dodge;controlsAllowed=allowed;
            skillButton.onClick.RemoveAllListeners();skillButton.onClick.AddListener(()=>{if(IsBattleUncovered&&allowed())skill();});
            restartButton.onClick.RemoveAllListeners();restartButton.onClick.AddListener(restart);
        }
        void ShowGestureStatus(BattleModel b)
        {
            EnsureFinalControls();
            string attack=b.AttackReady>b.Time?(b.AttackReady-b.Time).ToString("0.0")+"초":"준비";
            string dodge=b.DodgeReady>b.Time?(b.DodgeReady-b.Time).ToString("0.0")+"초":"준비";
            if(b.PlayerAttackSealed)attack="공격 봉인";
            if(b.PlayerStunned)attack=dodge="기절";
            if(b.Result!=BattleResult.Fighting)attack=dodge="전투 종료";
            ApplyFantasyHud();
            tapStatus.text="탭 공격\n"+attack;swipeStatus.text="좌우 회피\n"+dodge;
        }
    }
}


