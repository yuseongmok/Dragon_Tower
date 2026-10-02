using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class GaleSlashImpactSetup
    {
        [MenuItem("Dragon Tower/VFX/Install Gale Slash Impact")]
        public static void Install()
        {
            InstallRing();
            const string path="Assets/Resources/VFX/GaleSlashImpact.png";
            if(File.Exists(path))return;
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float px=x-31.5f,py=y-31.5f;
                float u=Mathf.Abs(px*.94f+py*.342f),v=Mathf.Abs(-px*.342f+py*.94f);
                bool main=u/30+v/4.5f<1, cross=u/3.5f+v/25<1,diamond=Mathf.Abs(px)/8+Mathf.Abs(py)/8<1;
                if(main||cross||diamond){float center=Mathf.Abs(px)+Mathf.Abs(py);pixels[y*64+x]=center<13?new Color(.96f,1,.98f):center<25?new Color(.61f,1,.90f):new Color(.10f,.78f,.70f);}
            }
            texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        static void InstallRing()
        {
            const string path="Assets/Resources/VFX/GaleSlashShockwave.png";if(File.Exists(path))return;
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);var pixels=new Color[4096];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float dx=x-31.5f,dy=y-31.5f,r=Mathf.Sqrt(dx*dx+dy*dy);
                if(r>28&&r<29.5f)pixels[y*64+x]=new Color(.6f,1,.9f);
            }
            texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }    }
}
