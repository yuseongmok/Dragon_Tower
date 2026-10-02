using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class WindClawSharpSetup
    {
        [MenuItem("Dragon Tower/VFX/Install Sharp Wind Claw")]
        public static void Install()
        {
            const string path="Assets/Resources/VFX/WindClawSharp.png";if(File.Exists(path))return;
            var texture=new Texture2D(96,96,TextureFormat.RGBA32,false);var pixels=new Color[96*96];
            for(int y=0;y<96;y++)for(int x=0;x<96;x++)for(int i=0;i<3;i++)
            {
                float lo=i==0?16:i==1?3:22,hi=i==0?85:i==1?93:80;
                if(y<lo||y>hi)continue;float t=(hi-y)/(hi-lo);
                float center=23+i*25+6*t*t+(i==2?-2:1)*Mathf.Sin(t*3.14f);
                float width=(i==1?9:i==0?7.5f:6)*Mathf.Pow(1-t,1.45f)*Mathf.Min(1,.45f+t*14);
                float distance=Mathf.Abs(x-center);
                if(distance>Mathf.Max(.5f,width))continue;
                Color c=distance>width-1?new Color(.015f,.22f,.27f):distance>width*.68f?new Color(.04f,.61f,.58f):distance>width*.28f?new Color(.35f,.98f,.85f):new Color(.96f,1,.98f);
                if(width<1.6f||t>.95f)c=new Color(.88f,1,.96f);pixels[y*96+x]=c;
            }
            texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
    }
}
