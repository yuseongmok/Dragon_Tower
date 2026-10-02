using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class ZephyrFinalSetup
    {
        public const string Art="Assets/Art/ZephyrFinal";
        public const string SetPath="Assets/Resources/DragonAnimations/ZephyrFinal.asset";
        [MenuItem("Dragon Tower/Production/Install Zephyr Final Evolution")]
        public static void Install()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");
            if(dragon==null||dragon.StableId!="zephyr")throw new System.InvalidOperationException("Zephyr data missing");
            EvolutionSpriteSetup.Install(dragon,2,Art,SetPath,"ZephyrFinal",string.IsNullOrWhiteSpace(dragon.finalName)?dragon.displayName:dragon.finalName,DragonAnimationArchetype.LargeQuadruped);
        }
        public static RectInt OpaqueBounds(Sprite sprite)=>EvolutionSpriteSetup.OpaqueBounds(sprite);
    }
}
