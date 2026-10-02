using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Content import recipe only. Runtime uses the shared data-driven player.
    public static class LunaSetup
    {
        public static void InstallBaseIdle()=>Install(0,true);
        public static void InstallBase()=>Install(0,false);
        public static void InstallMiddle()=>Install(1,false);
        public static void InstallFinal()=>Install(2,false);
        [MenuItem("Dragon Tower/Production/Install Luna Character Sets")]
        public static void InstallAll(){InstallBase();InstallMiddle();InstallFinal();}
        static void Install(int stage,bool idleOnly)
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon1.asset");
            if(dragon==null||dragon.StableId!="luna")throw new System.InvalidOperationException("Luna data missing");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            string form=new[]{"Base","Middle","Final"}[stage];
            string name=stage==0?dragon.displayName:stage==1?dragon.intermediateName:dragon.finalName;
            EvolutionSpriteSetup.Install(dragon,stage,"Assets/Art/Luna/"+form,"Assets/Resources/DragonAnimations/Luna"+form+".asset","Luna"+form,name,
                stage==2?DragonAnimationArchetype.Biped:stage==1?DragonAnimationArchetype.LargeQuadruped:DragonAnimationArchetype.SmallQuadruped,
                template,idleOnly,new[]{192f,226f,246f}[stage],true);
            var set=dragon.LoadAnimationSet(stage);
            // Measured against the rendered battle pose; ground registration stays shared.
            set.anchors.attackOrigin=new[]{new Vector2(88,-18),new Vector2(98,20),new Vector2(105,68)}[stage];
            set.anchors.skillOrigin=new[]{new Vector2(83,-10),new Vector2(92,27),new Vector2(105,68)}[stage];
            set.anchors.characterCenter=new[]{new Vector2(20,-62),new Vector2(6,-39),new Vector2(27,-15)}[stage];
            set.anchors.hitPosition=new[]{new Vector2(36,-52),new Vector2(39,-32),new Vector2(43,-8)}[stage];
            if(stage==2&&!idleOnly)ConfigureStaff(set);
            EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();
            Debug.Log("LUNA_INSTALLED "+form+(idleOnly?" Idle pilot":" six states"));
        }
        static void ConfigureStaff(DragonAnimationSet set)
        {
            // Authored moon-orb centers, normalized within each sprite cell.
            var points=new[]{
                new[]{new Vector2(.843f,.65f),new Vector2(.842f,.65f),new Vector2(.845f,.65f),new Vector2(.842f,.65f)},
                new[]{new Vector2(.73f,.611f),new Vector2(.773f,.637f),new Vector2(.79f,.584f),new Vector2(.707f,.584f)},
                new[]{new Vector2(.72f,.54f),new Vector2(.733f,.804f),new Vector2(.813f,.806f),new Vector2(.69f,.596f)},
                new[]{new Vector2(.844f,.65f),new Vector2(.863f,.52f),new Vector2(.875f,.68f),new Vector2(.824f,.67f)},
                new[]{new Vector2(.838f,.62f),new Vector2(.825f,.62f),new Vector2(.843f,.65f),new Vector2(.843f,.65f)},
                new[]{new Vector2(.70f,.532f),new Vector2(.713f,.51f),new Vector2(.73f,.426f),new Vector2(.744f,.429f)}
            };
            var sockets=new System.Collections.Generic.List<DragonSpriteAttachment>();
            foreach(DragonAnimationState state in System.Enum.GetValues(typeof(DragonAnimationState)))
                for(int i=0;i<4;i++)sockets.Add(new DragonSpriteAttachment{name="StaffTip",sprite=set.Get(state).frames[i],normalizedPosition=points[(int)state][i],useAsAttackOrigin=true,useAsSkillOrigin=true});
            set.attachments=sockets.ToArray();
            // Fallen body and staff share the ground, but do not let the long staff
            // bias foot-based registration toward the screen edge.
            for(int i=2;i<4;i++)set.death.offsets[i]+=new Vector2(-.20f,0);
            EditorUtility.SetDirty(set.death);
        }
    }
}
