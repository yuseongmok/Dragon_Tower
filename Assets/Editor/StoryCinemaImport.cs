using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public class StoryCinemaImport:AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Resources/Story/") || !assetPath.EndsWith(".png"))return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.filterMode=FilterMode.Point;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=2048;t.npotScale=TextureImporterNPOTScale.None;}
 }
}
