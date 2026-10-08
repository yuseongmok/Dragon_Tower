using System.IO;using System.Linq;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class MirLegendarySetup {
  public const string SkillPath="Assets/Data/Skills/skill_hwaryong.asset";
  public static void Install(){
   foreach(string path in new[]{"Assets/Resources/MirLegendary/Dragon.png","Assets/Art/MirLegendary/Icon.png"}){var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=path.Contains("Icon")?TextureImporterType.Sprite:TextureImporterType.Default;if(path.Contains("Icon"))imp.spriteImportMode=SpriteImportMode.Single;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=path.Contains("Icon")?128:2048;imp.SaveAndReimport();}
   var s=AssetDatabase.LoadAssetAtPath<SkillData>(SkillPath);if(s==null){s=ScriptableObject.CreateInstance<SkillData>();s.damage=170;s.cooldown=24;s.hitCount=2;s.initialHitDelay=.95f;s.hitInterval=2.65f;s.hitTimeOffsets=new[]{.95f,3.6f};s.finalHitDamageMultiplier=2;s.castLockDuration=4.7f;s.protectedCastDuration=4.7f;AssetDatabase.CreateAsset(s,SkillPath);}
   s.skillId="skill_hwaryong";s.displayName="화룡점정";s.description="미르 전용 · 청룡 관통 후 승천, 황금빛 수직 낙하. 시전부터 정상 복귀까지 무적.";s.elementType=ElementType.Wind;s.effectKind=SkillEffectKind.Wind;s.rarity=ContentRarity.Legendary;s.exclusiveDragonId="mir";s.icon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/MirLegendary/Icon.png");EditorUtility.SetDirty(s);
   var d=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon10.asset");d.signatureSkill=s;EditorUtility.SetDirty(d);var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");if(!db.skills.Contains(s)){db.skills=db.skills.Concat(new[]{s}).ToArray();EditorUtility.SetDirty(db);}AssetDatabase.SaveAssets();Debug.Log("HWARYONG_INSTALLED");
  }
 }
}
