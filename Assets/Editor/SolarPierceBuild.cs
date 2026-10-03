using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class SolarPierceBuild {
  public static void Run(){EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var preview=new GameObject("Solar Pierce review only").AddComponent<SolarPiercePreview>();preview.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon8.asset");preview.skill=AssetDatabase.LoadAssetAtPath<SkillData>(SolarPierceSetup.SkillPath);const string scene="Assets/Scenes/Prototypes/SolarPiercePreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);AssetDatabase.SaveAssets();var result=BuildPipeline.BuildPlayer(new[]{scene},"../SolarPierce-WebGL",BuildTarget.WebGL,BuildOptions.None);if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("SOLAR_BUILD_FAILED");Debug.Log("SOLAR_WEBGL_OK");}
 }
}
