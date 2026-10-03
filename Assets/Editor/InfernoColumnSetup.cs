using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class InfernoColumnSetup {
  public static void Generate(FireSkillLibrary lib){const string path="Assets/Resources/FireSkills/InfernoColumn.png";const int w=768,h=384;var t=new Texture2D(w,h,TextureFormat.RGBA32,false);var pixels=new Color[w*h];
   for(int layer=0;layer<3;layer++)for(int f=0;f<8;f++)for(int y=0;y<128;y++)for(int x=0;x<96;x++){
    float py=y/127f,px=(x-47.5f)/48f,phase=f*Mathf.PI*.25f;
    float center=.05f*Mathf.Sin(py*12+phase+layer)+.04f*Mathf.Sin(py*26+phase*2-layer);
    float radius=.64f+.07f*Mathf.Sin(py*21+phase*2+layer)+.045f*Mathf.Sin(py*43+phase*3)+.14f*Mathf.Pow(1-py,5);
    float depth=Mathf.Abs(px-center)/radius;if(depth>1)continue;
    float stream=Mathf.Sin(px*20+py*9+phase*2)+.45f*Mathf.Sin(px*37-py*16+phase*3);
    Color c;if(layer==0)c=depth>.85f?lib.style.deepCrimson:depth>.65f?lib.style.crimson:lib.style.redOrange;
    else if(layer==1)c=depth>.86f?lib.style.redOrange:stream>.65f?lib.style.gold:stream<-.65f?lib.style.redOrange:lib.style.orange;
    else {if(depth>.83f+.1f*Mathf.Sin(py*32+phase*2))continue;c=depth>.62f?lib.style.gold:stream>.3f?lib.style.whiteHot:lib.style.pale;}
    c.a=Mathf.Clamp01(py*24)*Mathf.Clamp01((1-py)*24);pixels[(layer*128+y)*w+f*96+x]=c;
   }t.SetPixels(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.Refresh();var im=(TextureImporter)AssetImporter.GetAtPath(path);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Multiple;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.npotScale=TextureImporterNPOTScale.None;im.textureCompression=TextureImporterCompression.Uncompressed;im.maxTextureSize=1024;var slices=new SpriteMetaData[24];for(int l=0;l<3;l++)for(int f=0;f<8;f++)slices[l*8+f]=new SpriteMetaData{name="Inferno_"+l+"_"+f,rect=new Rect(f*96,l*128,96,128),alignment=0,pivot=Vector2.one*.5f};
#pragma warning disable 618
   im.spritesheet=slices;
#pragma warning restore 618
   im.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();lib.infernoColumn=slices.Select(s=>sprites.First(p=>p.name==s.name)).ToArray();EditorUtility.SetDirty(lib);AssetDatabase.SaveAssets();if(lib.infernoColumn.Length!=24)throw new Exception("Incomplete inferno column atlas");
  }
 }
}
