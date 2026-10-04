using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class EarthBatchBuild {
  public static void Run(){EarthBatchSetup.Install(3);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var p=new GameObject("Earth shared skill comparison").AddComponent<EarthBatchPreview>();p.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon3.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon11.asset")};p.skills=new SkillData[4];for(int i=0;i<4;i++)p.skills[i]=AssetDatabase.LoadAssetAtPath<SkillData>(EarthBatchSetup.Paths[i]);const string scene="Assets/Scenes/Prototypes/EarthBatchPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);var r=BuildPipeline.BuildPlayer(new[]{scene},"../EarthBatch-WebGL",BuildTarget.WebGL,BuildOptions.None);if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Earth batch build failed");Debug.Log("EARTH_BATCH_WEBGL_OK");}
 }
}
