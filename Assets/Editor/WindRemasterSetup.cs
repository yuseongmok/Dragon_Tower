using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class WindRemasterSetup
    {
        const string Path="Assets/Resources/VFX/WindRemaster.png";
        [MenuItem("Dragon Tower/VFX/Install Wind rendering remaster")]
        public static void Install()
        {
            if(File.Exists(Path))return;
            // Derive discrete pixel layers from existing silhouettes; originals/data untouched.
            var modules=Read("WindModules");var blade=Read("WindBlade");var arc=Read("GaleSlashArc");
            var pixels=new Color[512*384];var slices=new List<SpriteMetaData>();
            for(int f=0;f<8;f++)for(int layer=0;layer<3;layer++)
            {
                int sy=(layer==2?3:4)*64;
                Put(layer==0?"StormBack"+f:layer==1?"StormFront"+f:"StormCore"+f,f*64,layer*64,64,64,(x,y)=>
                {
                    Color c=modules.GetPixel(f*64+x,sy+y);if(c.a<.01f)return Color.clear;
                    if(layer==0)return c.r<.1f?WindVFXStyle.Alpha(WindVFXStyle.Deep,.88f):Color.clear;
                    if(layer==1)return c.r<.1f?Color.clear:c.r>.8f?WindVFXStyle.Pale:c.r>.25f?WindVFXStyle.Mint:WindVFXStyle.Cyan;
                    return c.r>.8f?WindVFXStyle.Alpha(WindVFXStyle.Pale,.72f):WindVFXStyle.Alpha(WindVFXStyle.Cyan,.75f);
                });
            }
            Layers(blade,"Blade",64,64,192);Layers(arc,"Arc",96,64,256);
            var atlas=new Texture2D(512,384,TextureFormat.RGBA32,false);atlas.SetPixels(pixels);atlas.Apply();File.WriteAllBytes(Path,atlas.EncodeToPNG());
            Object.DestroyImmediate(atlas);Object.DestroyImmediate(modules);Object.DestroyImmediate(blade);Object.DestroyImmediate(arc);
            AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath(Path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.filterMode=FilterMode.Point;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
            importer.spritePixelsPerUnit=100;importer.maxTextureSize=512;
#pragma warning disable 618
            importer.spritesheet=slices.ToArray();
#pragma warning restore 618
            importer.SaveAndReimport();
            void Put(string name,int ox,int oy,int w,int h,System.Func<int,int,Color> pixel)
            {
                slices.Add(new SpriteMetaData{name=name,rect=new Rect(ox,oy,w,h),pivot=Vector2.one*.5f,alignment=0});
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)pixels[(oy+y)*512+ox+x]=pixel(x,y);
            }
            void Layers(Texture2D source,string name,int w,int h,int oy)
            {
                for(int layer=0;layer<3;layer++)
                {
                    int l=layer;
                    Put(name+(l==0?"Body":l==1?"Edge":"Core"),l*w,oy,w,h,(x,y)=>
                    {
                        Color c=source.GetPixel(x,y);if(c.a<.01f)return Color.clear;
                        if(l==0)return c.g<.4f?WindVFXStyle.Deep:WindVFXStyle.Cyan;
                        if(l==1)return c.r>.25f?WindVFXStyle.Mint:Color.clear;
                        // Only one thin pixel row of the original bright cutting band.
                        bool outer=y+1>=h||source.GetPixel(x,y+1).r<.8f;
                        return c.r>.8f&&outer?WindVFXStyle.Pale:Color.clear;
                    });
                }
            }
        }
        static Texture2D Read(string name){var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);texture.LoadImage(File.ReadAllBytes("Assets/Resources/VFX/"+name+".png"));return texture;}
    }
}
