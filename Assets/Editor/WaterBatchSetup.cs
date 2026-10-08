using System.IO;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class WaterBatchSetup {
  public static readonly string[] Paths={"Assets/Data/Skill5.asset","Assets/Data/Skills/skill_power_whip.asset","Assets/Data/Skills/skill_snipe.asset","Assets/Data/Skills/skill_splash.asset"};
  public static void Install(int through){const string folder="Assets/Resources/WaterSkills";Directory.CreateDirectory(folder);AssetDatabase.Refresh();var lib=AssetDatabase.LoadAssetAtPath<WaterSkillLibrary>(folder+"/Library.asset");if(lib==null){lib=ScriptableObject.CreateInstance<WaterSkillLibrary>();AssetDatabase.CreateAsset(lib,folder+"/Library.asset");}lib.style=AssetDatabase.LoadAssetAtPath<WaterVfxStyle>(WaterStyleSetup.StylePath);lib.entries=new WaterSkillLibrary.Entry[through+1];var names=new[]{"아쿠아샷","수류강타","저격","스플레쉬"};for(int i=0;i<=through;i++){var s=AssetDatabase.LoadAssetAtPath<SkillData>(Paths[i]);if(s.displayName!=names[i]){s.displayName=names[i];EditorUtility.SetDirty(s);}lib.entries[i]=new WaterSkillLibrary.Entry{skill=s,kind=(WaterSkillKind)i};}EditorUtility.SetDirty(lib);AssetDatabase.SaveAssets();}
 }
}
