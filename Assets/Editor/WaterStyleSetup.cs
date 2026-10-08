using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
 public static class WaterStyleSetup {
  public const string Folder="Assets/Art/WaterStylePrototype",StylePath=Folder+"/WaterStyle.asset",ScenePath="Assets/Scenes/Prototypes/WaterStylePreview.unity";
  [MenuItem("Dragon Tower/Water Style/Install isolated prototypes")]
  public static void Install(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var style=AssetDatabase.LoadAssetAtPath<WaterVfxStyle>(StylePath);if(style==null){style=ScriptableObject.CreateInstance<WaterVfxStyle>();AssetDatabase.CreateAsset(style,StylePath);}var go=new GameObject("Water art prototype - no combat logic");go.AddComponent<WaterStylePrototype>().style=style;PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Prototypes/WaterStylePrototype.prefab");Object.DestroyImmediate(go);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Water style review only").AddComponent<WaterStylePreview>();driver.style=style;driver.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon13.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon5.asset")};EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();Debug.Log("WATER_STYLE_INSTALLED");}
 }
}

