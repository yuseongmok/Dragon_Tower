using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class WindBladeSetup
    {
        [MenuItem("Dragon Tower/VFX/Install Wind Blade")]
        public static void Install()
        {
            const string path="Assets/Resources/VFX/WindBlade.png";
            if(!File.Exists(path))
            {
                var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];
                for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                {
                    float px=(x-31.5f)/32,py=(y-31.5f)/32;
                    float outer=Mathf.Sqrt(px*px+py*py),inner=Mathf.Sqrt(px*px+(py+.33f)*(py+.33f));
                    if(outer<.87f&&inner>.83f)
                        pixels[y*64+x]=outer>.83f?new Color(.025f,.3f,.34f):outer>.77f?new Color(.85f,1,.96f):outer>.71f?new Color(.35f,.97f,.96f):new Color(.12f,.75f,.65f);
                }
                texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.Refresh();
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            var skill=AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/skill_falling_flower.asset");
            skill.displayName="바람칼날";skill.rarity=ContentRarity.Rare;skill.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);skill.initialHitDelay=.16f;
            EditorUtility.SetDirty(skill);AssetDatabase.SaveAssets();
        }
    }
}
