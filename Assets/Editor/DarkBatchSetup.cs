using System.IO;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class DarkBatchSetup {
  public static readonly string[] Paths={"Assets/Data/Skill6.asset","Assets/Data/Skills/skill_paranoia.asset","Assets/Data/Skills/skill_space_rift.asset","Assets/Data/Skills/skill_apocalypse.asset"};
  public static void Install(int through){const string folder="Assets/Resources/DarkSkills";Directory.CreateDirectory(folder);AssetDatabase.Refresh();var lib=AssetDatabase.LoadAssetAtPath<DarkSkillLibrary>(folder+"/Library.asset");if(lib==null){lib=ScriptableObject.CreateInstance<DarkSkillLibrary>();AssetDatabase.CreateAsset(lib,folder+"/Library.asset");}lib.style=AssetDatabase.LoadAssetAtPath<DarkVfxStyle>(DarkStyleSetup.StylePath);lib.entries=new DarkSkillLibrary.Entry[through+1];var names=new[]{"어둠일격","피해망상","어둠구체","아포칼립스"};for(int i=0;i<=through;i++){var s=AssetDatabase.LoadAssetAtPath<SkillData>(Paths[i]);if(s.displayName!=names[i]){s.displayName=names[i];EditorUtility.SetDirty(s);}lib.entries[i]=new DarkSkillLibrary.Entry{skill=s,kind=(DarkSkillKind)i};}EditorUtility.SetDirty(lib);AssetDatabase.SaveAssets();}
 }
}
