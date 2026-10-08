using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class MirLegendaryWebBuild {
  public static void Run(){MirLegendarySetup.Install();EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("MirLegendary review only").AddComponent<MirLegendaryPreview>();driver.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon10.asset");driver.skill=AssetDatabase.LoadAssetAtPath<SkillData>(MirLegendarySetup.SkillPath);const string scene="Assets/Scenes/Prototypes/MirLegendaryPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);var report=BuildPipeline.BuildPlayer(new[]{scene},"../MirLegendary-WebGL",BuildTarget.WebGL,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("MirLegendary WebGL failed");Debug.Log("HWARYONG_WEBGL_OK");}
 }
}
