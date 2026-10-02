using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace DragonTower
{
    public partial class BattleView
    {
        readonly ItemData[] equippedVisuals=new ItemData[3];
        readonly Image[] equipmentIcons=new Image[3];
        readonly Text[] equipmentFallbacks=new Text[3];
        public Button RecoveryButton {get;private set;}
        Image recoveryIcon,recoveryProgress;Text recoveryCount,recoveryTime;
        UnityAction recoveryAction;
        bool recoveryAvailable;int recoveryAmount;
        void EnsureOverlaySlots()
        {
            var buttons=new[]{itemButton1,itemButton2,itemButton3};var labels=new[]{itemLabel1,itemLabel2,itemLabel3};
            for(int i=0;i<3;i++)
            {
                labels[i].gameObject.SetActive(false);buttons[i].enabled=false;
                foreach(var graphic in buttons[i].GetComponentsInChildren<Graphic>(true))graphic.raycastTarget=false;
                equipmentIcons[i]=ElementIcon(buttons[i].transform,0,24,30);
                equipmentFallbacks[i]=Label("장착",buttons[i].transform,0,24,30,28,10,Jade);
                SetEquipmentVisual(i,equippedVisuals[i]);
            }
            Text label;RecoveryButton=MakeButton("",frame,-182,783,60,64,new Color(.06f,.12f,.15f),out label);label.gameObject.SetActive(false);
            RecoveryButton.name="Recovery slot (unbound)";RecoveryButton.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
            RecoveryButton.targetGraphic=PixelFrame(RecoveryButton.GetComponent<Image>());
            recoveryIcon=ElementIcon(RecoveryButton.transform,0,29,32);recoveryIcon.gameObject.SetActive(false);
            recoveryCount=Label("—",RecoveryButton.transform,12,54,28,16,12,Paper);
            recoveryTime=Label("",RecoveryButton.transform,0,24,48,24,14,Paper);
            recoveryProgress=Panel("Recovery recharge",RecoveryButton.transform,0,51,40,2,Jade);
            recoveryProgress.rectTransform.pivot=new Vector2(0,.5f);recoveryProgress.rectTransform.anchoredPosition=new Vector2(-20,-51);
            RecoveryButton.onClick.AddListener(()=>{if(recoveryAction!=null&&recoveryAvailable&&recoveryAmount>0&&IsBattleUncovered&&controlsAllowed!=null&&controlsAllowed())recoveryAction();});
            SetRecoverySlot(null,0,false);
        }
        void SetEquipmentVisual(int index,ItemData item)
        {
            equippedVisuals[index]=item;if(equipmentIcons[index]==null)return;
            equipmentIcons[index].sprite=item==null?null:item.icon;
            equipmentIcons[index].gameObject.SetActive(item!=null&&item.icon!=null);
            equipmentFallbacks[index].gameObject.SetActive(false);
        }
        // Presentation-only extension points; no inventory, healing, consumption or timer logic.
        public void BindRecoveryAction(UnityAction action){recoveryAction=action;if(RecoveryButton!=null)RecoveryButton.interactable=action!=null&&recoveryAvailable&&recoveryAmount>0;}
        public void SetRecoverySlot(Sprite icon,int count,bool available,float remaining=0,float duration=0)
        {
            if(RecoveryButton==null)return;
            recoveryAvailable=available&&remaining<=0;recoveryAmount=Mathf.Max(0,count);
            recoveryIcon.sprite=icon;recoveryIcon.gameObject.SetActive(icon!=null&&recoveryAmount>0);

            recoveryCount.text=recoveryAmount>0?"×"+recoveryAmount:"";
            recoveryTime.text=recoveryAmount>0&&remaining>0?remaining.ToString("0.0"):"";
            recoveryProgress.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,recoveryAmount>0&&duration>0?40*Mathf.Clamp01(1-remaining/duration):0);
            RecoveryButton.interactable=recoveryAction!=null&&available&&count>0&&remaining<=0;
        }
    }
}



