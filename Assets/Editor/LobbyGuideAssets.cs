using System.IO;using UnityEngine;using UnityEditor;using TMPro;using UnityEngine.TextCore.LowLevel;
namespace DragonTower.Editor {
 public sealed class LobbyGuideImporter:AssetPostprocessor {
  public override int GetPostprocessOrder(){return 100;}
  void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Resources/UI/LobbyGuide/"))return;var im=(TextureImporter)assetImporter;im.filterMode=FilterMode.Point;im.maxTextureSize=2048;im.textureCompression=TextureImporterCompression.Uncompressed;im.npotScale=TextureImporterNPOTScale.None;im.GetSourceTextureWidthAndHeight(out int w,out int h);string n=Path.GetFileNameWithoutExtension(assetPath);float b=h*(n=="panel"?.15f:.23f);im.spriteBorder=n=="primary"||n=="secondary"||n=="panel"||n=="nameplate"?new Vector4(b,b,b,b):Vector4.zero;}
 }
 public static class LobbyGuideAssets {
  const string Input="C:/Users/PC/Documents/Codex/LobbyGuide-20261011/",Output="Assets/Resources/UI/LobbyGuide/";
  public static void Prepare(){
   Directory.CreateDirectory(Output);
   foreach(string name in new[]{"primary","secondary","panel","nameplate"}){var t=Load(name);Export(t,new RectInt(0,0,t.width,t.height),name);}
   var icons=Load("icons");string[] names={"gear","person","pencil","tower","book","dragon","trophy","chart","arrow"};
   for(int i=0;i<9;i++){int col=i%3,row=i/3;int x=col*icons.width/3,y=(2-row)*icons.height/3;Export(icons,new RectInt(x,y,icons.width/3,icons.height/3),names[i]);}
   AssetDatabase.Refresh();
   foreach(string file in Directory.GetFiles(Output,"*.png")){var im=(TextureImporter)AssetImporter.GetAtPath(file);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Single;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.textureCompression=TextureImporterCompression.Uncompressed;im.maxTextureSize=2048;im.alphaIsTransparency=true;im.spritePixelsPerUnit=100;im.SaveAndReimport();}
   var package=Directory.GetFiles("Library/PackageCache","TMP Essential Resources.unitypackage",SearchOption.AllDirectories);if(!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset")&&package.Length>0)AssetDatabase.ImportPackage(package[0],false);
   AssetDatabase.Refresh();
   if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Output+"LobbyFont.asset")==null){var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Jua-Regular.ttf");var fa=TMP_FontAsset.CreateFontAsset(font,48,5,GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);fa.name="Lobby Jua Dynamic";AssetDatabase.CreateAsset(fa,Output+"LobbyFont.asset");AssetDatabase.AddObjectToAsset(fa.material,fa);foreach(var atlas in fa.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,fa);fa.TryAddCharacters("유대의탑드래곤로비닉네임설정나최고점수층최대아직완료한도전이없습니다이어하기타워오르도감상태업적기록저장브라우눌러교체0123456789 ·()—",out string missing);EditorUtility.SetDirty(fa);}
   AssetDatabase.SaveAssets();Debug.Log("LOBBY_GUIDE_ASSETS_OK");EditorApplication.Exit(0);
  }
  static Texture2D Load(string name){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(Input+name+"-source.png"));return t;}
  static void Export(Texture2D t,RectInt region,string name){int left=region.xMax,right=region.xMin,bottom=region.yMax,top=region.yMin;for(int y=region.yMin;y<region.yMax;y++)for(int x=region.xMin;x<region.xMax;x++)if(t.GetPixel(x,y).a>.12f){left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}var crop=new Texture2D(right-left+1,top-bottom+1,TextureFormat.RGBA32,false);crop.SetPixels(t.GetPixels(left,bottom,crop.width,crop.height));crop.Apply();File.WriteAllBytes(Output+name+".png",crop.EncodeToPNG());Object.DestroyImmediate(crop);}
 }
}
