using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class WispBuild
    {
        public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var preview=new GameObject("Wisp review only").AddComponent<WispPreview>();preview.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset");preview.skill=AssetDatabase.LoadAssetAtPath<SkillData>(WispSetup.SkillPath);const string scene="Assets/Scenes/Prototypes/WispPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);AssetDatabase.SaveAssets();var result=BuildPipeline.BuildPlayer(new[]{scene},"../Wisp-WebGL",BuildTarget.WebGL,BuildOptions.None);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("WISP_BUILD_FAILED");Debug.Log("WISP_WEBGL_OK");}
    }
}
