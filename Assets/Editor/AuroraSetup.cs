using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class AuroraSetup
    {
        public static void InstallBaseIdle()=>Install(0,true);
        public static void InstallBase()=>Install(0,false);
        public static void InstallMiddle()=>Install(1,false);
        public static void InstallFinal()=>Install(2,false);
        [MenuItem("Dragon Tower/Production/Install Aurora Character Sets")]
        public static void InstallAll(){InstallBase();InstallMiddle();InstallFinal();}
        static void Install(int stage,bool idleOnly)
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset");
            if(dragon==null||dragon.StableId!="aurora")throw new System.InvalidOperationException("Aurora data missing");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            string form=new[]{"Base","Middle","Final"}[stage];
            string name=stage==0?dragon.displayName:stage==1?dragon.intermediateName:dragon.finalName;
            Directory.CreateDirectory("Assets/Resources/DragonAnimations");AssetDatabase.Refresh();
            EvolutionSpriteSetup.Install(dragon,stage,"Assets/Art/Aurora/"+form,"Assets/Resources/DragonAnimations/Aurora"+form+".asset","Aurora"+form,name,
                stage==0?DragonAnimationArchetype.Biped:DragonAnimationArchetype.LargeQuadruped,template,idleOnly,new[]{192f,226f,246f}[stage],true);
            // Content-authoring measurements from the 480x850 battle captures, in the
            // shared 256px actor rectangle. Future VFX query the set; no runtime name branch.
            var set=dragon.LoadAnimationSet(stage);
            var anchors=set.anchors;
            anchors.attackOrigin=new[]{new Vector2(96,-4),new Vector2(85,30),new Vector2(79,41)}[stage];
            anchors.skillOrigin=new[]{new Vector2(92,6),new Vector2(80,39),new Vector2(87,52)}[stage];
            anchors.characterCenter=new[]{new Vector2(45,-53),new Vector2(6,-37),new Vector2(7,-30)}[stage];
            anchors.hitPosition=new[]{new Vector2(50,-47),new Vector2(26,-24),new Vector2(36,-22)}[stage];
            EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();
            Debug.Log("AURORA_INSTALLED "+form+(idleOnly?" Idle pilot":" six states"));
        }
    }
}
