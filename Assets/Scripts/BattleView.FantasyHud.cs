using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public partial class BattleView
    {
        Image skillCharge,skillEmblem,skillShade;
        PixelHudFrame skillFrame;
        BattleModel chargeBattle;
        float chargeSpan;
        double previousRemaining;
        bool fantasyHud;
        static readonly Color Jade=new Color(.31f,.72f,.61f),Paper=new Color(.85f,.94f,.85f);
        static PixelHudFrame PixelFrame(Image image,bool prominent=false)
        {
            foreach(var effect in image.GetComponents<Shadow>())effect.enabled=false;
            image.enabled=false;
            var obj=new GameObject("Pixel bevel",typeof(RectTransform),typeof(CanvasRenderer),typeof(PixelHudFrame));
            var rect=obj.GetComponent<RectTransform>();rect.SetParent(image.transform,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.SetAsFirstSibling();
            var border=obj.GetComponent<PixelHudFrame>();
            border.color=new Color(.065f,.16f,.18f,.96f);border.prominent=prominent;border.raycastTarget=image.raycastTarget;
            border.edge=prominent?Jade:new Color(.23f,.39f,.38f);border.SetAllDirty();return border;
        }
        void ApplyFantasyHud()
        {
            if(fantasyHud||tapStatus==null)return;if(levelLabel==null||experienceFill==null)SetRunProgress(1,0,100);fantasyHud=true;
            var header=frame.Find("Pixel header");
            if(header!=null){var r=(RectTransform)header;r.anchoredPosition=new Vector2(0,-24);r.sizeDelta=new Vector2(480,48);header.gameObject.SetActive(false);}
            var footer=frame.Find("Pixel controls backing");
            if(footer!=null){var r=(RectTransform)footer;r.anchoredPosition=new Vector2(0,-775);r.sizeDelta=new Vector2(480,150);footer.gameObject.SetActive(false);}
            var enemyCard=frame.Find("Enemy card");if(enemyCard!=null){var r=(RectTransform)enemyCard;r.anchoredPosition=new Vector2(0,-80);r.sizeDelta=new Vector2(440,56);enemyCard.gameObject.SetActive(false);}
            foreach(Transform child in frame)
            {
                var t=child.GetComponent<Text>();if(t!=null&&t.text.StartsWith("D R A G O N"))child.gameObject.SetActive(false);
            }
            floorLabel.rectTransform.anchoredPosition=new Vector2(-198,-35);floorLabel.rectTransform.sizeDelta=new Vector2(80,34);floorLabel.fontSize=22;floorLabel.color=Paper;
            modeLabel.rectTransform.anchoredPosition=new Vector2(10,-35);modeLabel.rectTransform.sizeDelta=new Vector2(300,30);modeLabel.fontSize=15;modeLabel.color=Paper;
            levelLabel.rectTransform.anchoredPosition=new Vector2(174,-60);levelLabel.fontSize=11;
            experienceFill.transform.parent.GetComponent<RectTransform>().anchoredPosition=new Vector2(-143,-38);
            enemyName.fontSize=18;enemyName.color=Paper;playerName.color=Paper;
            enemyName.rectTransform.anchoredPosition=new Vector2(-45,-90);enemyHP.rectTransform.anchoredPosition=new Vector2(140,-90);enemyElementIcon.rectTransform.anchoredPosition=new Vector2(-198,-90);
            enemyFill.transform.parent.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-114);
            playerName.rectTransform.anchoredPosition=new Vector2(-45,-684);playerHP.rectTransform.anchoredPosition=new Vector2(140,-684);playerElementIcon.rectTransform.anchoredPosition=new Vector2(-198,-684);playerName.fontSize=17;
            playerFill.transform.parent.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-704);
            MoveControl(itemButton1,-90,783,48,48);MoveControl(itemButton2,-32,783,48,48);MoveControl(itemButton3,26,783,48,48);
            playerHP.color=enemyHP.color=new Color(.69f,.81f,.79f);
            foreach(var fill in new[]{playerFill,enemyFill,windupFill})
            {
                var back=fill.transform.parent.GetComponent<Image>();back.color=new Color(.04f,.11f,.13f);back.rectTransform.sizeDelta=new Vector2(400,10);
                fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,6);
            }
            playerFill.color=new Color(.18f,.76f,.65f);enemyFill.color=new Color(.91f,.29f,.38f);windupFill.color=new Color(.85f,.69f,.35f);
            foreach(var button in new[]{itemButton1,itemButton2,itemButton3})
            {
                var border=PixelFrame(button.GetComponent<Image>());button.targetGraphic=border;
                var c=button.colors;c.normalColor=Color.white;c.highlightedColor=new Color(.85f,1,.93f);c.pressedColor=new Color(.6f,.8f,.7f);c.disabledColor=new Color(.68f,.76f,.75f);button.colors=c;
            }
            foreach(var label in new[]{itemLabel1,itemLabel2,itemLabel3}){label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=new Vector2(5,2);label.rectTransform.offsetMax=new Vector2(-5,-2);label.fontSize=11;label.color=Paper;label.resizeTextForBestFit=true;label.resizeTextMinSize=9;label.resizeTextMaxSize=11;}
            foreach(var label in new[]{tapStatus,swipeStatus})
            {
                var panel=label.transform.parent.GetComponent<Image>();panel.enabled=false;foreach(var effect in panel.GetComponents<Shadow>())effect.enabled=false;
                label.transform.parent.gameObject.SetActive(false);
            }
            skillFrame=PixelFrame(skillButton.GetComponent<Image>(),true);skillButton.targetGraphic=skillFrame;
            var colors=skillButton.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(.82f,1,.92f);colors.pressedColor=new Color(.52f,.78f,.68f);colors.disabledColor=new Color(.65f,.74f,.72f);skillButton.colors=colors;
            skillShade=Panel("Cooldown veil",skillButton.transform,0,0,0,0,Color.clear);skillShade.gameObject.SetActive(false);
            skillShade.rectTransform.anchorMin=skillShade.rectTransform.anchorMax=new Vector2(1,.5f);skillShade.rectTransform.pivot=new Vector2(1,.5f);skillShade.rectTransform.anchoredPosition=new Vector2(-6,0);
            skillCharge=Panel("Skill recharge",skillButton.transform,0,0,100,4,Jade);
            skillCharge.rectTransform.anchorMin=skillCharge.rectTransform.anchorMax=new Vector2(0,.5f);skillCharge.rectTransform.pivot=new Vector2(0,.5f);skillCharge.rectTransform.anchoredPosition=new Vector2(28,-36);
            skillEmblem=ElementIcon(skillButton.transform,0,23,28);skillEmblem.raycastTarget=false;
            foreach(var label in new[]{floorLabel,modeLabel,levelLabel,enemyName,enemyHP,playerName,playerHP,warning,message}){var outline=label.GetComponent<Outline>()??label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.01f,.025f,.035f,.95f);outline.effectDistance=new Vector2(1,-1);}
            EnsureOverlaySlots();
            EnsureReferenceDetails();
        }
        void UpdateFantasyHud(BattleModel b)
        {
            ApplyFantasyHud();if(skillCharge==null)return;
            if(message.text=="게이지가 차기 직전에 회피하세요")message.text="";
            if(warning.text=="적의 움직임을 살피세요")warning.text="";
            windupFill.transform.parent.gameObject.SetActive(b.EnemyWindupProgress>0);
            float remaining=Mathf.Max(0,(float)(b.SkillReady-b.Time));
            if(chargeBattle!=b){chargeBattle=b;chargeSpan=remaining;previousRemaining=remaining;}
            if(remaining>previousRemaining+.02)chargeSpan=Mathf.Max(.01f,remaining);
            previousRemaining=remaining;
            float progress=remaining<=0?1:1-Mathf.Clamp01(remaining/Mathf.Max(.01f,chargeSpan));
            skillCharge.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,100*progress);
            UpdateElementTheme(b);
            skillLabel.rectTransform.offsetMin=new Vector2(20,16);skillLabel.rectTransform.offsetMax=new Vector2(-20,-38);skillLabel.fontSize=16;
            UpdateReferenceDetails(b,remaining,progress);
        }
    }
}




