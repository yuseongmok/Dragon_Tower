using System;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;
namespace DragonTower.Editor {
 public static class FireBatchBuild {
  public static void Run(){FireBatchSetup.Install(3);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var p=new GameObject("Fire shared skill comparison").AddComponent<FireBatchPreview>();p.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon8.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset")};p.skills=new SkillData[4];for(int i=0;i<4;i++)p.skills[i]=AssetDatabase.LoadAssetAtPath<SkillData>(FireBatchSetup.Paths[i]);const string scene="Assets/Scenes/Prototypes/FireBatchPreview.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);var r=BuildPipeline.BuildPlayer(new[]{scene},"../FireBatch-WebGL",BuildTarget.WebGL,BuildOptions.None);if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Fire batch build failed");Debug.Log("FIRE_BATCH_WEBGL_OK");}
 }
}
