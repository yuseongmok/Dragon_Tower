using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class HydraWebBuild {
  public static void Run(){HydraSetup.Install();EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Hydra review only").AddComponent<HydraPreview>();driver.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon14.asset");driver.skill=AssetDatabase.LoadAssetAtPath<SkillData>(HydraSetup.SkillPath);const string scene="Assets/Scenes/Prototypes/HydraPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);var report=BuildPipeline.BuildPlayer(new[]{scene},"../Hydra-WebGL",BuildTarget.WebGL,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Hydra WebGL failed");Debug.Log("HYDRA_WEBGL_OK");}
 }
}
