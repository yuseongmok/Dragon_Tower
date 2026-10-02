using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class ZephyrMiddleSetup
    {
        public const string Art="Assets/Art/ZephyrMiddle";
        public const string SetPath="Assets/Resources/DragonAnimations/ZephyrMiddle.asset";
        [MenuItem("Dragon Tower/Production/Install Zephyr Middle Evolution")]
        public static void Install()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");
            if(dragon==null||dragon.StableId!="zephyr")throw new System.InvalidOperationException("Zephyr data missing");
            EvolutionSpriteSetup.Install(dragon,1,Art,SetPath,"ZephyrMiddle",string.IsNullOrWhiteSpace(dragon.intermediateName)?dragon.displayName:dragon.intermediateName,DragonAnimationArchetype.SmallQuadruped);
        }
        public static RectInt OpaqueBounds(Sprite sprite)=>EvolutionSpriteSetup.OpaqueBounds(sprite);
    }
}
