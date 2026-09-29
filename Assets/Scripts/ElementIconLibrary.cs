using System.Collections.Generic;
using UnityEngine;

namespace DragonTower
{
    public static class ElementIconLibrary
    {
        static readonly Dictionary<ElementType, Sprite> Cache = new Dictionary<ElementType, Sprite>();
        static Texture2D sheet;

        public static Sprite Get(ElementType element)
        {
            if(element==ElementType.Neutral)return null;
            if(Cache.TryGetValue(element,out var cached))return cached;
            if(sheet==null)
            {
                sheet=Resources.Load<Texture2D>("UI/element-symbols");
                if(sheet==null)return null;
                sheet.filterMode=FilterMode.Point;
            }
            int column,row;
            switch(element)
            {
                case ElementType.Fire:column=0;row=1;break;
                case ElementType.Water:column=1;row=1;break;
                case ElementType.Wind:column=2;row=1;break;
                case ElementType.Earth:column=3;row=1;break;
                case ElementType.Lightning:column=0;row=0;break;
                case ElementType.Ice:column=1;row=0;break;
                case ElementType.Light:column=2;row=0;break;
                case ElementType.Dark:column=3;row=0;break;
                default:return null;
            }
            float width=sheet.width/4f,height=sheet.height/2f;
            var sprite=Sprite.Create(sheet,new Rect(column*width,row*height,width,height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            sprite.name="element_"+element.ToString().ToLowerInvariant();
            Cache[element]=sprite;
            return sprite;
        }
    }
}
