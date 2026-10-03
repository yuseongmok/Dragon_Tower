using System.IO;using System.Linq;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class FireSkillIcons {
  public static void Install(FireVfxStyle style){const string path="Assets/Resources/FireSkills/Icons.png";var t=new Texture2D(128,32,TextureFormat.RGBA32,false);var p=new Color[4096];for(int k=0;k<4;k++)for(int y=0;y<32;y++)for(int x=0;x<32;x++){float px=(x-15.5f)/14,py=(y-15.5f)/14;float d=10;
   if(k==0){float r=Mathf.Sqrt((px-.13f)*(px-.13f)+(py+.13f)*(py+.13f));d=r/.66f;if(px<.05f&&py>0)d=Mathf.Min(d,Mathf.Abs(px+py*.75f)/.28f+(1-py)*.4f);}
   if(k==1)for(int j=0;j<3;j++){float qx=px-(j-1)*.50f,qy=py-((j%2)*.23f-.1f);if(Mathf.Abs(qy)<.7f)d=Mathf.Min(d,Mathf.Abs(qx-qy*.3f)/(.18f*(1-(qy+.7f)/1.65f))+Mathf.Abs(qy)*.2f);}
   if(k==2){float r=Mathf.Sqrt((px-.1f)*(px-.1f)+(py+.23f)*(py+.23f));if(r<.56f){p[y*128+k*32+x]=Mathf.Abs(px+Mathf.Sin(py*10)*.12f)<.075f?style.gold:r>.44f?style.deepCrimson:new Color32(62,31,37,255);continue;}if(py>-.05f)d=Mathf.Abs(px+py*.6f)/.35f+py*.45f;}
   if(k==3){if(Mathf.Abs(px)<.88f&&Mathf.Abs(py-.65f)<.16f)d=Mathf.Abs(py-.65f)/.16f;for(int j=0;j<3;j++){float qx=px-(j-1)*.5f;if(py<.6f&&py>-.85f)d=Mathf.Min(d,Mathf.Abs(qx)/(.23f*(1-(.6f-py)/1.6f)));}}
   if(d<1)p[y*128+k*32+x]=d>.82f?style.deepCrimson:d>.6f?style.redOrange:d>.38f?style.orange:d>.17f?style.gold:style.whiteHot;
  }t.SetPixels(p);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.Refresh();var im=(TextureImporter)AssetImporter.GetAtPath(path);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Multiple;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.textureCompression=TextureImporterCompression.Uncompressed;var slices=new SpriteMetaData[4];for(int i=0;i<4;i++)slices[i]=new SpriteMetaData{name="FireSkillIcon_"+i,rect=new Rect(i*32,0,32,32),pivot=Vector2.one*.5f,alignment=0};
#pragma warning disable 618
  im.spritesheet=slices;
#pragma warning restore 618
  im.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();for(int i=0;i<4;i++){var data=AssetDatabase.LoadAssetAtPath<SkillData>(FireBatchSetup.Paths[i]);data.icon=sprites.First(s=>s.name=="FireSkillIcon_"+i);EditorUtility.SetDirty(data);}AssetDatabase.SaveAssets();}
 }
}
