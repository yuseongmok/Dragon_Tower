using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class ElementHudThemeSetup
    {
        // Explicit installer only: existing artist-authored theme data is never overwritten.
        [MenuItem("Dragon Tower/UI/Create missing element themes")]
        public static void Install()
        {
            System.IO.Directory.CreateDirectory("Assets/Resources/HudThemes");AssetDatabase.Refresh();
            Add(ElementType.Wind,"4DE1C3","2CB6A3","8FFFE7");
            Add(ElementType.Fire,"E86340","973E37","FFB36A");
            Add(ElementType.Ice,"73CFFF","438CC9","CAF3FF");
            Add(ElementType.Dark,"B77DDD","78498F","EDAFFF");
            Add(ElementType.Water,"519DE0","326496","A3DBFF");
            Add(ElementType.Earth,"C4A369","827044","F2D89E");
            Add(ElementType.Lightning,"E5CB58","9E853C","FFF0A8");
            Add(ElementType.Light,"E8DCAE","A59972","FFF9DF");
            Add(ElementType.Neutral,"9EAEC1","5F7289","D6E3F4");
            AssetDatabase.SaveAssets();
        }
        static Color Hex(string value){ColorUtility.TryParseHtmlString("#"+value,out var color);return color;}
        static void Add(ElementType element,string primary,string secondary,string glow)
        {
            string path="Assets/Resources/HudThemes/"+element+".asset";
            if(AssetDatabase.LoadAssetAtPath<ElementHudTheme>(path)!=null)return;
            var theme=ScriptableObject.CreateInstance<ElementHudTheme>();theme.element=element;
            theme.primaryColor=Hex(primary);theme.secondaryColor=Hex(secondary);theme.glowColor=Hex(glow);
            AssetDatabase.CreateAsset(theme,path);
        }
    }
}
