using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace DragonTower.Editor
{
    public static class PrototypeSetup
    {
        [MenuItem("Dragon Tower/Create prototype scene")]
        public static void Create()
        {
            Directory.CreateDirectory("Assets/Data");Directory.CreateDirectory("Assets/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var dragons=new DragonData[3];
            string[] names={"엠버","루나","제피르"},elements={"FIRE","ICE","WIND"},skills={"화염 폭발","서리 숨결","질풍 강타"};
            ElementType[] elementTypes={ElementType.Fire,ElementType.Ice,ElementType.Wind};
            Color[] colors={new Color(1,.49f,.29f),new Color(.40f,.77f,.95f),new Color(.44f,.87f,.66f)};
            int[] damage={38,52,24},hp={100,115,90};float[] cooldown={4,5.5f,2.5f};
            for(int i=0;i<3;i++)
            {
                var skill=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skill"+i+".asset");
                if(skill==null){skill=ScriptableObject.CreateInstance<SkillData>();skill.displayName=skills[i];skill.damage=damage[i];skill.cooldown=cooldown[i];AssetDatabase.CreateAsset(skill,"Assets/Data/Skill"+i+".asset");}
                var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+i+".asset");
                if(dragon==null){dragon=ScriptableObject.CreateInstance<DragonData>();dragon.displayName=names[i];dragon.element=elements[i];dragon.maxHP=hp[i];dragon.color=colors[i];dragon.skill=skill;AssetDatabase.CreateAsset(dragon,"Assets/Data/Dragon"+i+".asset");}
                dragons[i]=dragon;
                if(string.IsNullOrEmpty(dragon.speciesId))
                {dragon.speciesId=new[]{"ember","luna","zephyr"}[i];EditorUtility.SetDirty(dragon);}
                dragon.skillEffect=(SkillEffectKind)i;EditorUtility.SetDirty(dragon);
                dragon.elementType=elementTypes[i];EditorUtility.SetDirty(dragon);
                if(dragon.battleSprite==null)
                {
                    string[] spriteFiles={"ember-baby.png","luna-baby.png","zephyr-baby.png"};
                    dragon.battleSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/PixelBattle/"+spriteFiles[i]);
                    EditorUtility.SetDirty(dragon);
                }
            }
            var camera=new GameObject("Camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.03f,.04f,.07f);camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);
            var view=new GameObject("Battle UI").AddComponent<BattleView>();view.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansKR.ttf");view.Build();
            var controller=new GameObject("Battle Controller").AddComponent<BattleController>();controller.dragons=dragons;controller.view=view;
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            PlayerSettings.companyName="Dragon Tower Prototype";PlayerSettings.productName="Dragon Tower";
            PlayerSettings.defaultScreenWidth=480;PlayerSettings.defaultScreenHeight=850;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.runInBackground=false;
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.template="PROJECT:DragonTower";
            QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Battle.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Battle.unity",true)};
            AssetDatabase.SaveAssets();
            Verify();Debug.Log("DRAGON_TOWER_SETUP_OK");
        }
        [MenuItem("Dragon Tower/Verify combat rules")]
        public static void Verify()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");
            var b=new BattleModel(dragon.Snapshot());Check(b.Attack(),"First attack");Check(!b.Attack(),"Spam rejected");
            b.Tick(.299f);Check(!b.Attack(),"Cooldown before 0.3");b.Tick(.002f);Check(b.Attack(),"Cooldown after 0.3");
            Check(b.Skill(),"Skill fires");Check(!b.Skill(),"Skill cooldown");b.Tick(4.01f);Check(b.Skill(),"Skill recharges");
            b=new BattleModel(dragon.Snapshot());b.Tick(2.1f);Check(b.Dodge(),"Dodge fires");Check(!b.Dodge(),"Dodge cooldown");b.Tick(.31f);Check(b.PlayerHP==dragon.maxHP,"Timed dodge avoids damage");
            b=new BattleModel(dragon.Snapshot());b.Dodge();b.Tick(2.41f);Check(b.PlayerHP==dragon.maxHP-18,"Early dodge fails");
            b=new BattleModel(dragon.Snapshot());for(int i=0;i<60&&b.Result==BattleResult.Fighting;i++){b.Attack();b.Skill();b.Tick(.301f);}
            Check(b.Result==BattleResult.Victory && b.EnemyHP==0,"Victory and clamped HP");Check(!b.Attack()&&!b.Skill()&&!b.Dodge(),"Input blocked after win");
            b=new BattleModel(dragon.Snapshot());b.Tick(30);Check(b.Result==BattleResult.Defeat && b.PlayerHP==0,"Defeat and clamped HP");Check(!b.Attack(),"Input blocked after defeat");
            for(int i=0;i<3;i++){var d=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+i+".asset");Check(d!=null&&d.skill!=null,"Dragon data "+i);}
            Debug.Log("DRAGON_TOWER_TESTS_PASSED: 18 assertions");
        }
        static void Check(bool valid,string label){if(!valid)throw new Exception("Combat check failed: "+label);}
        [MenuItem("Dragon Tower/Build Web")]
        public static void BuildWeb()
        {
            Verify();
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Battle.unity"},locationPathName="Builds/Web",target=BuildTarget.WebGL,options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Web build failed: "+report.summary.result);
            Debug.Log("DRAGON_TOWER_WEB_BUILD_OK");
        }
    }
}

