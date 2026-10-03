using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class FireStyleSetup
    {
        public const string Folder="Assets/Art/FireStylePrototype",StylePath=Folder+"/FireStyle.asset",ScenePath="Assets/Scenes/Prototypes/FireStylePreview.unity";
        [MenuItem("Dragon Tower/Fire Style/Install isolated prototype")]
        public static void Install()
        {
            Directory.CreateDirectory(Folder);Directory.CreateDirectory("Assets/Prefabs/Prototypes");AssetDatabase.Refresh();
            var style=AssetDatabase.LoadAssetAtPath<FireVfxStyle>(StylePath);if(style==null){style=ScriptableObject.CreateInstance<FireVfxStyle>();AssetDatabase.CreateAsset(style,StylePath);}
            const int tile=96,w=768,h=960;var texture=new Texture2D(w,h,TextureFormat.RGBA32,false);var p=new Color[w*h];
            for(int m=0;m<10;m++)for(int f=0;f<8;f++)for(int y=0;y<tile;y++)for(int x=0;x<tile;x++)p[(m*tile+y)*w+f*tile+x]=Pixel(m,f,x,y,style);
            texture.SetPixels(p);texture.Apply();File.WriteAllBytes(Folder+"/FireAtlas.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.Refresh();
            var im=(TextureImporter)AssetImporter.GetAtPath(Folder+"/FireAtlas.png");im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Multiple;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.alphaIsTransparency=true;im.textureCompression=TextureImporterCompression.Uncompressed;im.npotScale=TextureImporterNPOTScale.None;im.spritePixelsPerUnit=100;im.maxTextureSize=1024;im.isReadable=false;
            var slices=new SpriteMetaData[80];for(int m=0;m<10;m++)for(int f=0;f<8;f++)slices[m*8+f]=new SpriteMetaData{name=((FireStyleModule)m)+"_"+f,rect=new Rect(f*96,m*96,96,96),pivot=Vector2.one*.5f,alignment=0};
            #pragma warning disable 618
            im.spritesheet=slices;
            #pragma warning restore 618
            im.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(Folder+"/FireAtlas.png").OfType<Sprite>().ToArray();style.frames=slices.Select(s=>sprites.First(p=>p.name==s.name)).ToArray();
            // Generic UI additive shader already used by approved VFX; no legacy Fire art input.
            var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/FireHighlight.mat");if(material==null){material=new Material(Shader.Find("DragonTower/Wind Claw Highlight"));AssetDatabase.CreateAsset(material,Folder+"/FireHighlight.mat");}style.highlight=material;EditorUtility.SetDirty(style);AssetDatabase.SaveAssets();
            var go=new GameObject("Fire style prototype (not a skill)");go.AddComponent<FireStylePrototype>().style=style;PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Prototypes/FireStylePrototype.prefab");UnityEngine.Object.DestroyImmediate(go);
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");var driver=new GameObject("Fire style review only").AddComponent<FireStylePreview>();driver.style=style;driver.dragons=new[]{AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon8.asset"),AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset")};EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);AssetDatabase.SaveAssets();Debug.Log("FIRE_STYLE_INSTALLED");
        }
        static Color A(Color c,float a){c.a=a;return c;}
        static Color Pixel(int m,int f,int x,int y,FireVfxStyle s)
        {
            float px=(x-47.5f)/44f,py=(y-47.5f)/44f,t=f/8f,phase=t*Mathf.PI*2;
            if(m<4||m==9){
                float yy=(y-5)/86f;if(yy<0||yy>1)return Color.clear;float depth=-1;int count=m==3?2:m==9?2:4;
                for(int j=0;j<count;j++){
                    float root=(j-(count-1)*.5f)*(m==3?.18f:.26f),height=(j==1?.96f:.64f+.18f*Mathf.Sin(j*2.1f+phase+m*.8f));if(m==3)height=.90f-j*.24f;if(m==9)height=j==0?.97f:.66f;
                    float u=yy/height;if(u>=1)continue;
                    float curl=Mathf.Sin(phase+j*1.7f+m*.9f);float line=root+curl*.48f*Mathf.Sin(u*4.4f)+.32f*Mathf.Sin(phase+j)*u*u;
                    float width=(m==3?.26f:m==9?.30f:.40f)*Mathf.Pow(1-u,1.05f)*(0.42f+.85f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(u*2.2f)))*(1+.24f*Mathf.Sin(u*11-phase-j));
                    depth=Mathf.Max(depth,1-Mathf.Abs(px-line)/Mathf.Max(.006f,width));
                }
                if(depth<=0)return Color.clear;
                if(m==0)return depth<.22f?s.deepCrimson:depth<.55f?s.crimson:s.redOrange;
                if(m==1)return depth<.15f?s.crimson:depth<.42f?s.redOrange:depth<.78f?s.orange:s.gold;
                if(m==2)return depth<.18f?s.orange:depth<.57f?s.gold:s.pale;
                if(m==3)return depth<.2f?s.pale:s.whiteHot;
                return depth<.28f?s.redOrange:depth<.65f?s.orange:s.gold;
            }
            float r=Mathf.Sqrt(px*px+py*py),a=Mathf.Atan2(py,px);
            if(m==4){float k=f/7f,edge=(.24f+.67f*k)*(1+.16f*Mathf.Sin(a*7+k*3)+.055f*Mathf.Sin(a*17-k*6));float hole=Mathf.Max(0,k-.52f)*1.3f;
                if(r>edge||r<hole||r>.98f)return Color.clear;float z=(r-hole)/Mathf.Max(.01f,edge-hole);Color c=z>.87f?s.crimson:z>.66f?s.redOrange:z>.35f?s.orange:z>.12f?s.gold:s.pale;if(k<.25f&&r<edge*.6f)c=s.whiteHot;return A(c,1-k*.35f);}
            if(m==5){float edge=.61f+.10f*Mathf.Sin(a*5+phase)+.08f*Mathf.Sin(a*9-phase);float rr=Mathf.Sqrt(px*px+py*py*.8f);if(rr>edge)return Color.clear;float fold=Mathf.Sin(px*8+py*6+phase)*.12f;return rr>edge-.11f?s.smoke:py+fold>.1f?new Color32(107,56,48,255):new Color32(72,39,42,255);}
            if(m==6){float band=Mathf.Abs(px-(Mathf.Sin(py*7-phase)*.12f+.13f));bool visible=Mathf.Abs(py)<.85f&&((y/5+f)%5!=0);if(visible&&(band<.018f||Mathf.Abs(px+Mathf.Sin(py*5+phase)*.10f+.26f)<.018f))return A(s.pale,.55f);return Color.clear;}
            if(m==7){float shape=Mathf.Abs(px)*1.5f+Mathf.Abs(py)*.70f;if(shape>.36f)return Color.clear;return shape<.09f?s.whiteHot:shape<.18f?s.gold:shape<.26f?s.orange:s.redOrange;}
            if(m==8){float rr=r+.025f*Mathf.Sin(a*12+phase);float radius=.43f+f/7f*.43f;if(Mathf.Abs(rr-radius)<.045f&&Mathf.Sin(a*11+phase)>.0f)return s.gold;if(Mathf.Abs(rr-radius)<.09f)return A(s.redOrange,.65f);return Color.clear;}
            return Color.clear;
        }
    }
}
