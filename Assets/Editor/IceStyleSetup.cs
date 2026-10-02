using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class IceStyleSetup
    {
        public const string Folder="Assets/Art/IceStylePrototype",StylePath=Folder+"/IceStyle.asset",ScenePath="Assets/Scenes/Prototypes/IceStylePreview.unity";
        [MenuItem("Dragon Tower/Ice Style/Install isolated prototype")]
        public static void Install()
        {
            Directory.CreateDirectory(Folder);Directory.CreateDirectory("Assets/Prefabs/Prototypes");Directory.CreateDirectory("Assets/Scenes/Prototypes");AssetDatabase.Refresh();
            var style=AssetDatabase.LoadAssetAtPath<IceVfxStyle>(StylePath);if(style==null){style=ScriptableObject.CreateInstance<IceVfxStyle>();AssetDatabase.CreateAsset(style,StylePath);}
            // Code-authored pixel atlas follows the existing repo's VFX authoring workflow.
            // Every tile has its own geometry; no Wind artwork or Legacy Ice input is read.
            var texture=new Texture2D(256,256,TextureFormat.RGBA32,false);var pixels=new Color[256*256];
            for(int kind=0;kind<16;kind++)for(int y=0;y<64;y++)for(int x=0;x<64;x++)pixels[((kind/4)*64+y)*256+(kind%4)*64+x]=Pixel(kind,x,y,style);
            texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(Folder+"/IceStyleAtlas.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(Folder+"/IceStyleAtlas.png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=100;importer.maxTextureSize=256;importer.isReadable=false;
            var slices=new SpriteMetaData[16];for(int k=0;k<16;k++)slices[k]=new SpriteMetaData{name=((IceStyleModule)k).ToString(),rect=new Rect(k%4*64,k/4*64,64,64),pivot=Vector2.one*.5f,alignment=0};
            #pragma warning disable 618
            importer.spritesheet=slices;
            #pragma warning restore 618
            importer.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(Folder+"/IceStyleAtlas.png").OfType<Sprite>().ToArray();style.modules=slices.Select(s=>sprites.First(p=>p.name==s.name)).ToArray();EditorUtility.SetDirty(style);AssetDatabase.SaveAssets();
            var go=new GameObject("Ice style prototype (not a skill)");go.AddComponent<IceStylePrototype>().style=style;PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Prototypes/IceStylePrototype.prefab");UnityEngine.Object.DestroyImmediate(go);
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Ice style preview only").AddComponent<IceStylePreview>();driver.style=style;driver.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon1.asset")};
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();Debug.Log("ICE_STYLE_INSTALLED independent atlas/prefab/preview scene; original assets untouched");
        }
        static bool Poly(float x,float y,params Vector2[] p){bool inside=false;for(int i=0,j=p.Length-1;i<p.Length;j=i++)if((p[i].y>y)!=(p[j].y>y)&&x<(p[j].x-p[i].x)*(y-p[i].y)/(p[j].y-p[i].y)+p[i].x)inside=!inside;return inside;}
        static Vector2 V(float x,float y)=>new Vector2(x,y);
        static bool Line(float x,float y,float ax,float ay,float bx,float by,float width=1){var p=V(x-ax,y-ay);var d=V(bx-ax,by-ay);float t=Mathf.Clamp01(Vector2.Dot(p,d)/d.sqrMagnitude);return (p-d*t).sqrMagnitude<=width*width;}
        static Color Alpha(Color c,float a){c.a=a;return c;}
        static Color Pixel(int k,int x,int y,IceVfxStyle s)
        {
            bool shape=Poly(x,y,V(28,7),V(14,19),V(17,40),V(36,58),V(49,38),V(44,16));
            switch((IceStyleModule)k)
            {
                case IceStyleModule.Interior:
                    if(shape)return x<30?s.deepNavy:s.deepBlue;break;
                case IceStyleModule.Body:
                    if(Poly(x,y,V(29,9),V(30,38),V(36,55),V(46,37),V(42,18)))return x>38?s.iceBlue:Alpha(s.cyan,.84f);
                    if(Poly(x,y,V(17,20),V(20,39),V(34,54),V(28,36),V(26,10)))return y>35?s.pale:s.iceBlue;
                    if(Poly(x,y,V(23,22),V(25,34),V(28,36),V(26,17)))return Alpha(s.lavender,.85f);break;
                case IceStyleModule.Edge:
                    if(Line(x,y,28,8,30,38)||Line(x,y,30,38,36,57)||Line(x,y,36,57,48,38)||Line(x,y,48,38,43,17))return s.pale;
                    if(Line(x,y,15,20,18,40)||Line(x,y,18,40,36,57)||Line(x,y,30,38,48,38))return s.cyan;break;
                case IceStyleModule.Highlight:
                    if((Mathf.Abs(x-32)<=1&&Mathf.Abs(y-32)<10)||(Mathf.Abs(y-32)<=1&&Mathf.Abs(x-32)<6))return s.white;break;
                case IceStyleModule.Refraction:
                    if(Poly(x,y,V(24,25),V(28,26),V(39,40),V(36,42)))return s.white;
                    if(Line(x,y,21,25,24,30))return s.pale;break;
                case IceStyleModule.FrostA:case IceStyleModule.FrostB:
                    int shift=k==4?0:5;
                    if((x/3+y/3+shift)%7==0&&Poly(x,y,V(9,22),V(18,35),V(34,39),V(55,28),V(46,22)))return Alpha(s.pale,.75f);
                    if(y>=25&&y<=28&&x>12&&x<51&&x%9<6)return Alpha(s.cyan,.4f);break;
                case IceStyleModule.CrackA:case IceStyleModule.CrackB:case IceStyleModule.CrackC:
                    int branches=k-6;
                    if(Line(x,y,31,31,37,41)||Line(x,y,31,31,28,23)||Line(x,y,31,31,23,33))return s.white;
                    if(branches>0&&(Line(x,y,37,41,34,49)||Line(x,y,28,23,33,16)||Line(x,y,23,33,17,27)))return s.pale;
                    if(branches>1&&(Line(x,y,37,41,43,44)||Line(x,y,28,23,20,17)||Line(x,y,23,33,18,40)||Line(x,y,31,31,44,26)))return s.cyan;break;
                case IceStyleModule.ShardMedium:
                    if(Poly(x,y,V(26,9),V(22,37),V(35,55),V(39,25)))
                    {if(Line(x,y,26,10,35,53))return s.white;return x>30?s.cyan:s.deepBlue;}break;
                case IceStyleModule.ShardSmall:
                    if(Poly(x,y,V(16,21),V(42,48),V(46,19)))
                    {if(Line(x,y,17,21,42,48))return s.pale;return y>29?s.cyan:s.deepBlue;}break;
                case IceStyleModule.ShardLarge:case IceStyleModule.StatusCrystal:
                    if(Poly(x,y,V(23,13),V(16,35),V(38,53),V(45,28)))
                    {if(Line(x,y,23,14,38,51)||Line(x,y,38,51,44,28))return s.white;if(x>30)return s.cyan;return y>32?s.iceBlue:s.deepBlue;}
                    break;
                case IceStyleModule.Impact:
                    float d=Mathf.Abs(x-32)+Mathf.Abs(y-32);
                    if(d<5)return s.white;if(d<10&&((x+y)%3!=0))return s.pale;
                    if((Line(x,y,32,32,19,42)||Line(x,y,32,32,46,38)||Line(x,y,32,32,29,17))&&d<20)return s.cyan;break;
                case IceStyleModule.FrostRing:
                    float dx=(x-32)/25f,dy=(y-32)/19f,r=dx*dx+dy*dy;
                    if(r>.80f&&r<1.05f&&(x/5+y/4)%4!=0)return y<32?s.deepBlue:s.pale;break;
            }
            return Color.clear;
        }
    }
}
