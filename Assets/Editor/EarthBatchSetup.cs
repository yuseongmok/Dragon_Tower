using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor
{
    public static class EarthBatchSetup
    {
        public static readonly string[] Paths={"Assets/Data/Skill3.asset","Assets/Data/Skills/skill_dust_storm.asset","Assets/Data/Skills/skill_earthquake.asset","Assets/Data/Skills/skill_shockwave.asset"};
        public static void Install(int through)
        {
            const string folder="Assets/Resources/EarthSkills";
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var lib=AssetDatabase.LoadAssetAtPath<EarthSkillLibrary>(folder+"/Library.asset");
            if(lib==null){lib=ScriptableObject.CreateInstance<EarthSkillLibrary>();AssetDatabase.CreateAsset(lib,folder+"/Library.asset");}
            lib.style=AssetDatabase.LoadAssetAtPath<EarthVfxStyle>(EarthStyleSetup.StylePath);
            lib.entries=new EarthSkillLibrary.Entry[through+1];
            for(int i=0;i<=through;i++)
            {
                var skill=AssetDatabase.LoadAssetAtPath<SkillData>(Paths[i]);
                if(i==0)skill.displayName="대지 강타";if(i==1)skill.displayName="강철 클로";
                if(i<2)EditorUtility.SetDirty(skill);
                lib.entries[i]=new EarthSkillLibrary.Entry{skill=skill,kind=(EarthSkillKind)i};
            }
            MakeShapes(lib,through);EditorUtility.SetDirty(lib);AssetDatabase.SaveAssets();
        }
        static Color C(byte r,byte g,byte b)=>new Color32(r,g,b,255);
        static Color Pixel(int row,int frame,float x,float y)
        {
            if(row==0)
            {
                // Asymmetric stone blade: broad fractured base, a long oblique edge,
                // dark side thickness and multiple planar faces, not a stretched boulder.
                float u=(y+1)*.5f;
                float center=-.20f+.38f*u*u;
                float width=.64f*Mathf.Pow(1-u,.66f)+.012f;
                float left=center-width*(.76f+.08f*Mathf.Sin(u*19));
                float right=center+width;
                if(u<.025f||u>.98f||x<left||x>right)return Color.clear;
                float q=(x-left)/(right-left);
                Color c=q<.18f?C(40,31,30):q<.53f?C(108,75,48):q<.81f?C(176,133,77):C(229,203,145);
                if(u<.29f&&q>.3f)c=C(78,58,43);
                if(u>.29f&&u<.48f&&q<.57f)c=C(147,107,65);
                float seam=Mathf.Abs(y+.32f*x+.18f*Mathf.Sin(x*7));
                if(seam<.015f&&u<.70f)c=C(44,34,29);
                if(right-x<.025f)c=C(251,230,181);
                return c;
            }
            if(row>=1&&row<=3)
            {
                float u=(y+1)*.5f,progress=.53f+frame/7f*.47f;
                float center=.28f*Mathf.Sin(u*2.45f)-.20f;
                float width=.46f*Mathf.Pow(1-u,.65f)+.006f;
                if(u<.03f||u>progress||u>.985f)return Color.clear;
                float q=(x-center)/width;
                if(row==1){if(Mathf.Abs(q)>1)return Color.clear;return q<-.65f?C(21,23,25):C(48,46,43);}
                if(row==2)
                {
                    if(q<-.60f||q>.83f)return Color.clear;
                    float facet=q+.17f*Mathf.Floor(u*5)%2;
                    Color c=facet<-.15f?C(75,77,78):facet<.40f?C(131,135,132):C(172,178,168);
                    if(Mathf.Abs(u-.28f-.12f*q)<.012f||Mathf.Abs(u-.63f+.16f*q)<.010f)c=C(52,52,48);
                    return c;
                }
                if(q<.57f||q>.91f)return Color.clear;
                return q>.80f?C(245,241,213):C(204,211,203);
            }
            if(row==4)
            {
                float r=Mathf.Sqrt(x*x+y*y),a=Mathf.Atan2(y,x)+frame*Mathf.PI/4;
                float edge=.81f+.055f*Mathf.Sin(a*9);
                if(r>edge||r<edge-.24f||Mathf.Sin(a*3)<-.35f)return Color.clear;
                return r>edge-.026f?C(238,210,150):r<edge-.18f?C(55,42,30):C(157,123,62);
            }
            return Color.clear;
        }
        static void MakeShapes(EarthSkillLibrary lib,int through)
        {
            const int tile=128,rows=5,w=1024,h=rows*tile;
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var pixels=new Color[w*h];
            for(int row=0;row<rows;row++)for(int f=0;f<8;f++)for(int y=0;y<tile;y++)for(int x=0;x<tile;x++)
                pixels[(row*tile+y)*w+f*tile+x]=Pixel(row,f,(x-63.5f)/64,(y-63.5f)/64);
            tex.SetPixels(pixels);tex.Apply();const string path="Assets/Resources/EarthSkills/Shapes.png";
            File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.Refresh();
            var im=(TextureImporter)AssetImporter.GetAtPath(path);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Multiple;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.npotScale=TextureImporterNPOTScale.None;im.textureCompression=TextureImporterCompression.Uncompressed;im.spritePixelsPerUnit=100;im.maxTextureSize=2048;im.isReadable=false;
            var slices=new SpriteMetaData[rows*8];for(int row=0;row<rows;row++)for(int f=0;f<8;f++)slices[row*8+f]=new SpriteMetaData{name="EarthShape_"+row+"_"+f,rect=new Rect(f*tile,row*tile,tile,tile),pivot=Vector2.one*.5f,alignment=0};
            #pragma warning disable 618
            im.spritesheet=slices;
            #pragma warning restore 618
            im.SaveAndReimport();var all=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();lib.shapes=slices.Select(s=>all.First(x=>x.name==s.name)).ToArray();
        }
    }
}
