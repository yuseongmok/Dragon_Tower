using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonTower
{
    public static class ElementHudThemes
    {
        static Dictionary<ElementType,ElementHudTheme> themes;
        static ElementHudTheme fallback;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset(){themes=null;if(fallback!=null)UnityEngine.Object.Destroy(fallback);fallback=null;}
        public static ElementHudTheme Get(ElementType element)
        {
            if(themes==null)
            {
                themes=new Dictionary<ElementType,ElementHudTheme>();
                var assets=Resources.LoadAll<ElementHudTheme>("HudThemes");
                Array.Sort(assets,(a,b)=>string.CompareOrdinal(a.name,b.name));
                foreach(var asset in assets)
                {
                    if(themes.ContainsKey(asset.element)){Debug.LogWarning("Duplicate HUD theme for "+asset.element+": "+asset.name);continue;}
                    themes.Add(asset.element,asset);
                }
            }
            if(themes.TryGetValue(element,out var theme))return theme;
            if(themes.TryGetValue(ElementType.Neutral,out theme))return theme;
            if(fallback==null){fallback=ScriptableObject.CreateInstance<ElementHudTheme>();fallback.hideFlags=HideFlags.HideAndDontSave;}
            return fallback;
        }
    }
}
