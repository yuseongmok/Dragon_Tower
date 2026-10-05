using System.IO;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class LightBatchSetup {
  public static readonly string[] Paths={"Assets/Data/Skill7.asset","Assets/Data/Skills/skill_holy_explosion.asset","Assets/Data/Skills/skill_light_pillar.asset","Assets/Data/Skills/skill_big_bang.asset"};
  public static void Install(int through){const string folder="Assets/Resources/LightSkills";Directory.CreateDirectory(folder);AssetDatabase.Refresh();var lib=AssetDatabase.LoadAssetAtPath<LightSkillLibrary>(folder+"/Library.asset");if(lib==null){lib=ScriptableObject.CreateInstance<LightSkillLibrary>();AssetDatabase.CreateAsset(lib,folder+"/Library.asset");}lib.style=AssetDatabase.LoadAssetAtPath<LightVfxStyle>(LightStyleSetup.StylePath);lib.entries=new LightSkillLibrary.Entry[through+1];var names=new[]{"섬광 폭발","신성 폭발","성역","빅뱅"};for(int i=0;i<=through;i++){var s=AssetDatabase.LoadAssetAtPath<SkillData>(Paths[i]);if(s.displayName!=names[i]){s.displayName=names[i];EditorUtility.SetDirty(s);}lib.entries[i]=new LightSkillLibrary.Entry{skill=s,kind=(LightSkillKind)i};}EditorUtility.SetDirty(lib);AssetDatabase.SaveAssets();}
 }
}
