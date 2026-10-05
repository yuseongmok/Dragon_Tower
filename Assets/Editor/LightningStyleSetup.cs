using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
namespace DragonTower.Editor {
 public static class LightningStyleSetup {
  public const string Folder="Assets/Art/LightningStylePrototype",StylePath=Folder+"/LightningStyle.asset",ScenePath="Assets/Scenes/Prototypes/LightningStylePreview.unity";
  [MenuItem("Dragon Tower/Lightning Style/Install isolated prototypes")]
  public static void Install(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var style=AssetDatabase.LoadAssetAtPath<LightningVfxStyle>(StylePath);if(style==null){style=ScriptableObject.CreateInstance<LightningVfxStyle>();AssetDatabase.CreateAsset(style,StylePath);}var go=new GameObject("Lightning art prototype - no damage or paralysis logic");go.AddComponent<LightningStylePrototype>().style=style;PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Prototypes/LightningStylePrototype.prefab");Object.DestroyImmediate(go);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Lightning style review only").AddComponent<LightningStylePreview>();driver.style=style;driver.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon4.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon12.asset")};EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();Debug.Log("LIGHTNING_STYLE_INSTALLED");}
 }
}
