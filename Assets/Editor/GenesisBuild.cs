using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class GenesisBuild {
  public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var p=new GameObject("Genesis review only").AddComponent<GenesisPreview>();p.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon3.asset");p.skill=AssetDatabase.LoadAssetAtPath<SkillData>(GenesisSetup.SkillPath);const string scene="Assets/Scenes/Prototypes/GenesisPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);AssetDatabase.SaveAssets();var r=BuildPipeline.BuildPlayer(new[]{scene},"../Genesis-WebGL",BuildTarget.WebGL,BuildOptions.None);if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("GENESIS_BUILD_FAILED");Debug.Log("GENESIS_WEBGL_OK");}
 }
}
