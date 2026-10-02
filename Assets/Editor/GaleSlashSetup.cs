using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class GaleSlashSetup
    {
        [MenuItem("Dragon Tower/VFX/Install Gale Slash")]
        public static void Install()
        {
            const string path="Assets/Resources/VFX/GaleSlashArc.png";
            if(!File.Exists(path))
            {
                var texture=new Texture2D(96,64,TextureFormat.RGBA32,false);var pixels=new Color[96*64];
                for(int y=0;y<64;y++)for(int x=0;x<96;x++)
                {
                    float px=(x-47.5f)/48,py=(y-26f)/32;
                    float r=Mathf.Sqrt(px*px+py*py),inner=Mathf.Sqrt((px+.10f)*(px+.10f)+(py+.35f)*(py+.35f));
                    // Broad asymmetrical hooked sweep, with three distinct pixel palette bands.
                    if(r<.94f&&inner>.87f&&py>-.2f)
                        pixels[y*96+x]=r>.90f?new Color(.02f,.25f,.29f):r>.83f?new Color(.90f,1,.96f):r>.74f?new Color(.34f,1,.89f):new Color(.06f,.64f,.60f);
                }
                texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.Refresh();
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            var skill=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/skill_gale_slash.asset");
            if(skill.hitCount==5&&skill.damage==11){skill.damage=55;skill.hitCount=1;skill.initialHitDelay=.24f;skill.rarity=ContentRarity.Rare;skill.description="거대한 바람 횡베기 · 단일 타격 · Slow 35%";}
            if(skill.icon==null){skill.initialHitDelay=.24f;skill.rarity=ContentRarity.Rare;}
            skill.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);EditorUtility.SetDirty(skill);AssetDatabase.SaveAssets();
        }
    }
}
