using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class VolcanicMeteorBuild {
  public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var preview=new GameObject("Volcanic Meteor review only").AddComponent<VolcanicMeteorPreview>();preview.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");preview.skill=AssetDatabase.LoadAssetAtPath<SkillData>(VolcanicMeteorSetup.SkillPath);const string scene="Assets/Scenes/Prototypes/VolcanicMeteorPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);AssetDatabase.SaveAssets();var result=BuildPipeline.BuildPlayer(new[]{scene},"../VolcanicMeteor-WebGL",BuildTarget.WebGL,BuildOptions.None);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("VOLCANIC_BUILD_FAILED");Debug.Log("VOLCANIC_WEBGL_OK");}
 }
}
