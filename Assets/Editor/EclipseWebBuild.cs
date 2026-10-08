using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class EclipseWebBuild {
  public static void Run(){EclipseSetup.Install();EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Eclipse review only").AddComponent<EclipsePreview>();driver.dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon6.asset");driver.skill=AssetDatabase.LoadAssetAtPath<SkillData>(EclipseSetup.SkillPath);const string scene="Assets/Scenes/Prototypes/EclipsePreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);var report=BuildPipeline.BuildPlayer(new[]{scene},"../Eclipse-WebGL",BuildTarget.WebGL,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Eclipse WebGL failed");Debug.Log("ECLIPSE_WEBGL_OK");}
 }
}
