using System.Linq;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class CatastropheSetup {
  public const string SkillPath="Assets/Data/Skills/skill_catastrophe.asset";
  public static void Install(){AssetDatabase.Refresh();var im=(TextureImporter)AssetImporter.GetAtPath("Assets/Art/Catastrophe/CatastropheIcon.png");im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Single;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.maxTextureSize=128;im.textureCompression=TextureImporterCompression.Uncompressed;im.SaveAndReimport();
   var s=AssetDatabase.LoadAssetAtPath<SkillData>(SkillPath);if(s==null){s=ScriptableObject.CreateInstance<SkillData>();AssetDatabase.CreateAsset(s,SkillPath);s.damage=48;s.cooldown=22;s.hitCount=7;s.initialHitDelay=.95f;s.hitInterval=.18f;s.hitTimeOffsets=new[]{.95f,1.02f,1.40f,1.47f,1.85f,1.92f,3.10f};s.finalHitDamageMultiplier=4;var thunder=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/skill_thunder_strike.asset");s.statusDuration=thunder.statusDuration;}
   s.skillId="skill_catastrophe";s.displayName="재해";s.description="스톰 전용 · 전장을 휩쓰는 다중 허리케인과 낙뢰. 바람/번개 중 유리한 상성을 적용하고 최종 적중 시 확정 마비.";s.elementType=ElementType.Lightning;s.useSecondaryElement=true;s.secondaryElement=ElementType.Wind;s.effectKind=SkillEffectKind.Lightning;s.rarity=ContentRarity.Legendary;s.exclusiveDragonId="storm";s.statusEffect=CombatStatusEffect.Paralyze;s.statusChancePercent=100;s.statusOnFinalHit=true;s.statusOnHit=true;s.icon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Catastrophe/CatastropheIcon.png");EditorUtility.SetDirty(s);
   var d=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon12.asset");d.signatureSkill=s;EditorUtility.SetDirty(d);var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");if(!db.skills.Contains(s)){db.skills=db.skills.Concat(new[]{s}).ToArray();EditorUtility.SetDirty(db);}AssetDatabase.SaveAssets();
  }
 }
}
