using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
 public static class DarkStyleSetup {
  public const string Folder="Assets/Art/DarkStylePrototype",StylePath=Folder+"/DarkStyle.asset",ScenePath="Assets/Scenes/Prototypes/DarkStylePreview.unity";
  [MenuItem("Dragon Tower/Dark Style/Install isolated prototypes")]
  public static void Install(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var style=AssetDatabase.LoadAssetAtPath<DarkVfxStyle>(StylePath);if(style==null){style=ScriptableObject.CreateInstance<DarkVfxStyle>();AssetDatabase.CreateAsset(style,StylePath);}var go=new GameObject("Dark art prototype - no combat logic");go.AddComponent<DarkStylePrototype>().style=style;PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Prototypes/DarkStylePrototype.prefab");Object.DestroyImmediate(go);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Dark style review only").AddComponent<DarkStylePreview>();driver.style=style;driver.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon6.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon14.asset")};EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();Debug.Log("DARK_STYLE_INSTALLED");}
 }
}

