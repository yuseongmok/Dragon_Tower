using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public partial class BattleView
    {
        PixelWindEmblem windSkill,windBadge;
        PixelHudFrame screenRim;
        Text skillCaption;
        Image[] slotJewels;
        Image radialShade;
        Texture2D cooldownTexture;Sprite cooldownSprite;
        void OnDestroy(){if(cooldownSprite!=null)Destroy(cooldownSprite);if(cooldownTexture!=null)Destroy(cooldownTexture);}
        void EnsureReferenceDetails()
        {
            skillCharge.enabled=false; // Ring now displays the same progress value.
            skillEmblem.rectTransform.anchoredPosition=new Vector2(0,-50);skillEmblem.rectTransform.sizeDelta=new Vector2(50,50);
            windSkill=WindGlyph(skillButton.transform,0,50,52);
            windBadge=WindGlyph(frame,-198,684,26);
            skillCaption=Label("",skillButton.transform,0,105,120,18,13,Paper);skillCaption.name="Equipped skill name";
            skillCaption.resizeTextForBestFit=true;skillCaption.resizeTextMinSize=10;skillCaption.resizeTextMaxSize=13;
            var captionOutline=skillCaption.gameObject.AddComponent<Outline>();captionOutline.effectColor=new Color(.01f,.025f,.035f,1);captionOutline.effectDistance=new Vector2(1,-1);
            radialShade=Panel("Radial cooldown mask",skillButton.transform,0,50,62,62,new Color(.015f,.025f,.045f,.68f));
            cooldownTexture=new Texture2D(32,32);cooldownTexture.filterMode=FilterMode.Point;var pixels=new Color[1024];
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)pixels[y*32+x]=new Vector2(x-15.5f,y-15.5f).sqrMagnitude<=256?Color.white:Color.clear;
            cooldownTexture.SetPixels(pixels);cooldownTexture.Apply();
            cooldownSprite=Sprite.Create(cooldownTexture,new Rect(0,0,32,32),Vector2.one*.5f);radialShade.sprite=cooldownSprite;radialShade.type=Image.Type.Filled;radialShade.fillMethod=Image.FillMethod.Radial360;radialShade.fillOrigin=2;radialShade.fillClockwise=true;
            skillLabel.transform.SetAsLastSibling();
            var outline=skillLabel.GetComponent<Outline>()??skillLabel.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.01f,.025f,.035f,1);outline.effectDistance=new Vector2(2,-2);
            var rim=Panel("Fine screen rim",frame,0,425,472,842,Color.clear);rim.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
            screenRim=PixelFrame(rim);screenRim.color=Color.clear;screenRim.outlineOnly=true;screenRim.SetVerticesDirty();screenRim.raycastTarget=false;
            slotJewels=new Image[4];float[] xs={-182,-90,-32,26};
            for(int i=0;i<4;i++)
            {
                var jewel=Panel("Slot accent "+i,frame,xs[i],826,5,5,Jade);jewel.rectTransform.localRotation=Quaternion.Euler(0,0,45);slotJewels[i]=jewel;if(i>0)jewel.gameObject.SetActive(false);
            }
            experienceFill.transform.parent.gameObject.SetActive(false);
        }
        PixelWindEmblem WindGlyph(Transform parent,float x,float y,float size)
        {
            var rect=Rect("Wind spiral",parent,x,y,size,size);rect.gameObject.AddComponent<CanvasRenderer>();var glyph=rect.gameObject.AddComponent<PixelWindEmblem>();glyph.raycastTarget=false;return glyph;
        }
        void UpdateReferenceDetails(BattleModel battle,float remaining,float progress)
        {
            enemyName.rectTransform.anchoredPosition=new Vector2(-45,-90);
            playerName.rectTransform.anchoredPosition=new Vector2(-45,-684);
            floorLabel.gameObject.SetActive(false);
            modeLabel.rectTransform.anchoredPosition=new Vector2(0,-35);
            levelLabel.rectTransform.anchoredPosition=new Vector2(-146,-684);levelLabel.rectTransform.sizeDelta=new Vector2(58,24);levelLabel.fontSize=13;levelLabel.alignment=TextAnchor.MiddleLeft;
            playerName.rectTransform.anchoredPosition=new Vector2(-21,-684);playerName.rectTransform.sizeDelta=new Vector2(194,28);playerName.fontSize=16;playerName.text=playerName.text.Replace(" / "," · ");
            var theme=ElementHudThemes.Get(battle.Dragon.elementType);
            bool wind=battle.Dragon.elementType==ElementType.Wind&&theme.elementIcon==null;
            windSkill.gameObject.SetActive(wind);windBadge.gameObject.SetActive(wind);
            if(wind){skillEmblem.enabled=false;playerElementIcon.enabled=false;}
            windSkill.color=battle.CanUseSkill?theme.primaryColor:Color.Lerp(theme.secondaryColor,Color.black,.45f);
            windBadge.color=theme.primaryColor;
            if(skillFrame.progress!=progress){skillFrame.progress=progress;skillFrame.SetVerticesDirty();}
            SetThemeEdge(screenRim,WithAlpha(theme.secondaryColor,.38f));
            foreach(var jewel in slotJewels)jewel.color=WithAlpha(theme.primaryColor,.65f);
            skillEmblem.color=battle.CanUseSkill?Color.white:new Color(.36f,.36f,.36f);
            radialShade.fillAmount=1-progress;radialShade.enabled=remaining>0;
            var recoveryBorder=RecoveryButton.targetGraphic as PixelHudFrame;SetThemeEdge(recoveryBorder,RecoveryButton.interactable?theme.primaryColor:theme.secondaryColor);
            recoveryIcon.color=RecoveryButton.interactable?Color.white:new Color(.5f,.5f,.5f);
            recoveryCount.color=RecoveryButton.interactable?Paper:new Color(.65f,.69f,.72f);
            skillLabel.rectTransform.offsetMin=new Vector2(14,20);skillLabel.rectTransform.offsetMax=new Vector2(-14,-20);
            skillLabel.fontSize=remaining>0?28:15;
            if(battle.CanUseSkill)skillLabel.text="";
            else if(remaining>0&&!battle.PlayerStunned&&!battle.PlayerSkillSealed&&!battle.Dragon.skillDisabled)skillLabel.text=remaining.ToString("0.0");
            if(battle.Enemy.elementType==ElementType.Neutral&&CurrentEnemySprite!=null){enemyElementIcon.sprite=CurrentEnemySprite;enemyElementIcon.enabled=true;enemyElementIcon.gameObject.SetActive(true);enemyElementIcon.color=Color.white;}
            skillCaption.text=battle.Dragon.skill.displayName;
            if(equippedSkillPresentation!=null&&equippedSkillPresentation.icon!=null){windSkill.gameObject.SetActive(false);skillEmblem.enabled=true;skillEmblem.sprite=equippedSkillPresentation.icon;}
        }
    }
}






