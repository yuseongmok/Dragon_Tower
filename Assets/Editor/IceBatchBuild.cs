using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class IceBatchBuild
    {
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var p=new GameObject("Ice batch comparison only").AddComponent<IceBatchPreview>();p.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon1.asset")};p.skills=new SkillData[4];for(int i=0;i<4;i++)p.skills[i]=AssetDatabase.LoadAssetAtPath<SkillData>(IceBatchSetup.Paths[i]);
            const string scene="Assets/Scenes/Prototypes/IceBatchPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);AssetDatabase.SaveAssets();
            var result=BuildPipeline.BuildPlayer(new[]{scene},"../IceBatch-WebGL",BuildTarget.WebGL,BuildOptions.None);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("ICE_BATCH_WEBGL_FAILED");Debug.Log("ICE_BATCH_WEBGL_OK");
        }
    }
}
