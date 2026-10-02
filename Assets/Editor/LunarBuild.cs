using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class LunarBuild
    {
        public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var p=new GameObject("Lunar signature review only").AddComponent<LunarPreview>();p.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon1.asset");p.skill=AssetDatabase.LoadAssetAtPath<SkillData>(LunarSetup.SkillPath);const string scene="Assets/Scenes/Prototypes/LunarPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);AssetDatabase.SaveAssets();var result=BuildPipeline.BuildPlayer(new[]{scene},"../Lunar-WebGL",BuildTarget.WebGL,BuildOptions.None);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("LUNAR_BUILD_FAILED");Debug.Log("LUNAR_WEBGL_OK");}
    }
}
