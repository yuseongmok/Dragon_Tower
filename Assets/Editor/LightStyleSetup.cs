using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
 public static class LightStyleSetup {
  public const string Folder="Assets/Art/LightStylePrototype",StylePath=Folder+"/LightStyle.asset",ScenePath="Assets/Scenes/Prototypes/LightStylePreview.unity";
  [MenuItem("Dragon Tower/Light Style/Install isolated prototypes")]
  public static void Install(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var style=AssetDatabase.LoadAssetAtPath<LightVfxStyle>(StylePath);if(style==null){style=ScriptableObject.CreateInstance<LightVfxStyle>();AssetDatabase.CreateAsset(style,StylePath);}var go=new GameObject("Light art prototype - no combat logic");go.AddComponent<LightStylePrototype>().style=style;PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Prototypes/LightStylePrototype.prefab");Object.DestroyImmediate(go);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Light style review only").AddComponent<LightStylePreview>();driver.style=style;driver.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon15.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon7.asset")};EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();Debug.Log("LIGHT_STYLE_INSTALLED");}
 }
}
