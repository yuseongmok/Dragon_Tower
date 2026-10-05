using System.IO;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class LightningBatchSetup {
  public static readonly string[] Paths={"Assets/Data/Skill4.asset","Assets/Data/Skills/skill_electric_bolt.asset","Assets/Data/Skills/skill_lightning_orb.asset","Assets/Data/Skills/skill_thunder_strike.asset"};
  public static void Install(int through){const string folder="Assets/Resources/LightningSkills";Directory.CreateDirectory(folder);AssetDatabase.Refresh();var lib=AssetDatabase.LoadAssetAtPath<LightningSkillLibrary>(folder+"/Library.asset");if(lib==null){lib=ScriptableObject.CreateInstance<LightningSkillLibrary>();AssetDatabase.CreateAsset(lib,folder+"/Library.asset");}lib.style=AssetDatabase.LoadAssetAtPath<LightningVfxStyle>(LightningStyleSetup.StylePath);lib.entries=new LightningSkillLibrary.Entry[through+1];var names=new[]{"쇼크","오버히트","플라즈마","뇌격"};for(int i=0;i<=through;i++){var s=AssetDatabase.LoadAssetAtPath<SkillData>(Paths[i]);if(s.displayName!=names[i]){s.displayName=names[i];EditorUtility.SetDirty(s);}lib.entries[i]=new LightningSkillLibrary.Entry{skill=s,kind=(LightningSkillKind)i};}EditorUtility.SetDirty(lib);AssetDatabase.SaveAssets();}
 }
}
