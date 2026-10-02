using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public partial class BattleView
    {
        ElementHudTheme hudTheme;
        Outline skillGlow,elementGlow;
        Image playerAccent;
        bool skillWasReady;
        float readyGlowUntil;
        void UpdateElementTheme(BattleModel battle)
        {
            var theme=ElementHudThemes.Get(battle.Dragon.elementType);
            if(skillGlow==null)
            {
                skillGlow=skillFrame.gameObject.AddComponent<Outline>();skillGlow.effectDistance=new Vector2(2,-2);
                elementGlow=playerElementIcon.gameObject.AddComponent<Outline>();elementGlow.effectDistance=new Vector2(1,-1);
                playerAccent=Panel("Element accent",frame,-177,684,2,17,Color.white);playerAccent.raycastTarget=false;
            }
            if(hudTheme!=theme)
            {
            hudTheme=theme;
            foreach(var button in new[]{itemButton1,itemButton2,itemButton3,RecoveryButton})
            {
                var colors=button.colors;colors.normalColor=colors.highlightedColor=Color.white;
                colors.pressedColor=new Color(.75f,.75f,.75f);colors.disabledColor=new Color(.68f,.68f,.68f);button.colors=colors;
                var border=button.targetGraphic as PixelHudFrame;
                SetThemeEdge(border,Color.Lerp(new Color(.16f,.19f,.23f),theme.secondaryColor,.8f));
            }
            foreach(var text in equipmentFallbacks)text.color=theme.primaryColor;
            recoveryProgress.color=theme.primaryColor;
            playerAccent.color=theme.secondaryColor;
            elementGlow.effectColor=WithAlpha(theme.glowColor,.35f);
            }
            var icon=theme.element==battle.Dragon.elementType&&theme.elementIcon!=null?theme.elementIcon:ElementIconLibrary.Get(battle.Dragon.elementType);
            playerElementIcon.sprite=icon;playerElementIcon.enabled=icon!=null;playerElementIcon.gameObject.SetActive(icon!=null);
            playerElementIcon.color=Color.white;
            skillEmblem.sprite=icon;skillEmblem.enabled=icon!=null;
            // Cosmetic readiness pulse only. The existing model remains the source of availability and timers.
            bool ready=battle.CanUseSkill;
            if(ready&&!skillWasReady)readyGlowUntil=Time.unscaledTime+.55f;
            skillWasReady=ready;
            float pulse=ready?Mathf.Clamp01((readyGlowUntil-Time.unscaledTime)/.55f):0;
            SetThemeEdge(skillFrame,ready?Color.Lerp(theme.primaryColor,theme.glowColor,.3f+pulse*.5f):theme.secondaryColor);
            skillGlow.effectColor=WithAlpha(theme.glowColor,ready?.24f+pulse*.36f:0);
            skillCharge.color=ready?theme.primaryColor:theme.secondaryColor;
            // Neutral interaction multipliers keep all element palettes intact.
            var skillColors=skillButton.colors;skillColors.normalColor=Color.white;skillColors.highlightedColor=Color.white;
            skillColors.pressedColor=new Color(.75f,.75f,.75f);skillColors.disabledColor=new Color(.68f,.68f,.68f);skillButton.colors=skillColors;
        }
        static Color WithAlpha(Color color,float alpha){color.a=alpha;return color;}
        static void SetThemeEdge(PixelHudFrame frame,Color color){if(frame!=null&&frame.edge!=color){frame.edge=color;frame.SetVerticesDirty();}}
    }
}




