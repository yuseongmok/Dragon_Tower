using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class WindClawHierarchySetup
    {
        [MenuItem("Dragon Tower/VFX/Install Wind Claw Hierarchy")]
        public static void Install()
        {
            const string path="Assets/Resources/VFX/WindClawHierarchy.png";if(File.Exists(path))return;
            const int w=96,h=128;var tex=new Texture2D(w*8,h*2,TextureFormat.RGBA32,false);var pixels=new Color[w*8*h*2];
            var metadata=new SpriteMetaData[16];
            for(int frame=0;frame<8;frame++)
            {
                float progress=(frame+1)/8f;
                for(int layer=0;layer<2;layer++)
                {
                    int oy=layer==0?h:0;
                    metadata[frame+layer*8]=new SpriteMetaData{name=(layer==0?"ClawBody":"ClawCore")+frame,rect=new Rect(frame*w,oy,w,h),alignment=0,pivot=new Vector2(.5f,.5f)};
                    for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                    {
                        Color result=Color.clear;
                        for(int claw=0;claw<3;claw++)
                        {
                            float best=10000,bestT=0,signed=0;
                            for(int step=0;step<=80;step++)
                            {
                                float t=step/80f;if(t>progress)break;Vector2 p=Point(claw,t);Vector2 tangent=(Point(claw,Mathf.Min(1,t+.01f))-Point(claw,Mathf.Max(0,t-.01f))).normalized;
                                Vector2 normal=new Vector2(-tangent.y,tangent.x);
                                Vector2 delta=new Vector2(x+.5f,y+.5f)-p;float d=delta.sqrMagnitude;
                                if(d<best){best=d;bestT=t;signed=Vector2.Dot(delta,normal);}
                            }
                            float thickness=(claw==1?6.0f:claw==0?4.4f:3.5f)*Mathf.Pow(1-bestT,.95f)*Mathf.Min(1,.5f+bestT*9);
                            thickness*=Mathf.Clamp01((progress-bestT)/.09f);
                            
                            float distance=Mathf.Sqrt(best);if(distance>thickness||thickness<.1f)continue;
                            if(layer==1){if(thickness>1.5f&&(signed<thickness*.06f||signed>thickness*.40f))continue;result=new Color(.92f,1,.96f);}
                            else
                            {
                                float edge=signed/Mathf.Max(.1f,thickness);
                                result=distance>thickness-.65f?new Color(.025f,.20f,.32f):edge<-.5f?new Color(.06f,.53f,.74f):edge<-.08f?new Color(.14f,.90f,.91f):edge<.45f?new Color(.5f,.97f,.88f):new Color(.35f,.98f,.72f);
                            }
                        }
                        pixels[(oy+y)*w*8+frame*w+x]=result;
                    }
                }
            }
            tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 618
            importer.spritesheet=metadata;
#pragma warning restore 618
            importer.SaveAndReimport();
        }
        static Vector2 Point(int claw,float t)
        {
            float x=claw*20;Vector2 a=new Vector2(8+x,claw==1?123:claw==0?111:103),b=new Vector2(-2+x,65),c=new Vector2(16+x,-13),d=new Vector2(49+x,claw==1?18:claw==0?25:32);
            float u=1-t;return a*u*u*u+b*3*u*u*t+c*3*u*t*t+d*t*t*t;
        }
    }
}

