using UnityEngine;
using UnityEngine.UI;

namespace DragonTower
{
    public static class DragonTowerTheme
    {
        public static readonly Color Night=new Color(.025f,.035f,.06f,1);
        public static readonly Color Slate=new Color(.075f,.105f,.15f,.94f);
        public static readonly Color SlateLight=new Color(.12f,.16f,.22f,.96f);
        public static readonly Color Gold=new Color(.88f,.66f,.30f,1);
        public static readonly Color GoldDim=new Color(.43f,.31f,.15f,1);
        public static readonly Color Parchment=new Color(.82f,.74f,.59f,1);

        static RectTransform Strip(string name,Transform parent,Vector2 anchorMin,Vector2 anchorMax,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=anchorMin;rect.anchorMax=anchorMax;rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=size;
            var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;return rect;
        }
        public static void Frame(Image panel,Color edge,bool corners=true)
        {
            if(panel==null||panel.transform.Find("Theme top")!=null)return;
            var outline=panel.gameObject.GetComponent<Outline>()??panel.gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.8f);outline.effectDistance=new Vector2(3,-3);outline.useGraphicAlpha=true;
            Strip("Theme top",panel.transform,new Vector2(0,1),new Vector2(1,1),new Vector2(-10,3),edge);
            Strip("Theme bottom",panel.transform,new Vector2(0,0),new Vector2(1,0),new Vector2(-10,3),GoldDim);
            Strip("Theme left",panel.transform,new Vector2(0,0),new Vector2(0,1),new Vector2(3,-10),edge);
            Strip("Theme right",panel.transform,new Vector2(1,0),new Vector2(1,1),new Vector2(3,-10),GoldDim);
            if(!corners)return;
            foreach(var p in new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,0),new Vector2(1,1)})
            {var c=Strip("Theme corner",panel.transform,p,p,new Vector2(10,10),edge);c.localRotation=Quaternion.Euler(0,0,45);}
        }
        public static void StyleButton(Button button,Color accent)
        {
            if(button==null)return;var image=button.targetGraphic as Image;if(image==null)return;Frame(image,accent);
            var colors=button.colors;colors.normalColor=image.color;colors.highlightedColor=Color.Lerp(image.color,Color.white,.14f);colors.pressedColor=Color.Lerp(image.color,Color.black,.22f);colors.selectedColor=colors.highlightedColor;colors.disabledColor=new Color(.15f,.17f,.2f,.78f);colors.colorMultiplier=1;button.colors=colors;
        }
        public static Color Grade(ItemGrade grade)
        {switch(grade){case ItemGrade.Rare:return new Color(.20f,.58f,.82f);case ItemGrade.Epic:return new Color(.63f,.32f,.88f);case ItemGrade.Unique:return new Color(1,.63f,.18f);default:return Parchment;}}
        public static Color Grade(AugmentGrade grade)
        {switch(grade){case AugmentGrade.Rare:return new Color(.20f,.58f,.82f);case AugmentGrade.Epic:return new Color(.63f,.32f,.88f);case AugmentGrade.Unique:return new Color(1,.63f,.18f);default:return Parchment;}}
    }
}
