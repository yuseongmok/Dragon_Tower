using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Bake rotating faceted geometry into small pixel animation frames. No runtime 3D renderer.
    public static class IceBallDepthSetup
    {
        const int Cell=96,Cols=8,Rows=6;
        public static void Install()
        {
            const string folder="Assets/Resources/IceSkills/BallDepth";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var style=AssetDatabase.LoadAssetAtPath<IceVfxStyle>(IceStyleSetup.StylePath);var tex=new Texture2D(Cell*Cols,Cell*Rows,TextureFormat.RGBA32,false);var all=new Color[Cell*Cols*Cell*Rows];
            for(int row=0;row<Rows;row++)for(int f=0;f<8;f++){
                var tile=new Color[Cell*Cell];if(row==0||row==1)Sphere(tile,f,row==1,style);else for(int y=0;y<Cell;y++)for(int x=0;x<Cell;x++)tile[y*Cell+x]=Effect(row,f,x,y,style);
                for(int y=0;y<Cell;y++)for(int x=0;x<Cell;x++)all[(row*Cell+y)*Cell*Cols+f*Cell+x]=tile[y*Cell+x];
            }
            tex.SetPixels(all);tex.Apply();File.WriteAllBytes(folder+"/DepthFrames.png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.Refresh();
            var imp=(TextureImporter)AssetImporter.GetAtPath(folder+"/DepthFrames.png");imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.spritePixelsPerUnit=100;imp.isReadable=false;imp.maxTextureSize=1024;imp.npotScale=TextureImporterNPOTScale.None;
            var slices=new SpriteMetaData[48];for(int row=0;row<Rows;row++)for(int f=0;f<8;f++)slices[row*8+f]=new SpriteMetaData{name="Depth"+row+"_"+f,rect=new Rect(f*Cell,row*Cell,Cell,Cell),alignment=9,pivot=Vector2.one*.5f};
#pragma warning disable 618
            imp.spritesheet=slices;
#pragma warning restore 618
            imp.SaveAndReimport();var shader=Resources.Load<Shader>("VFX/WindClawHighlight");var mat=AssetDatabase.LoadAssetAtPath<Material>(folder+"/Highlight.mat");if(mat==null){mat=new Material(shader);AssetDatabase.CreateAsset(mat,folder+"/Highlight.mat");}AssetDatabase.SaveAssets();Debug.Log("ICE_BALL_DEPTH_INSTALLED 48 frames / shared highlight shader");
        }
        static void Sphere(Color[] tile,int frame,bool cracked,IceVfxStyle s)
        {
            float t=(1+Mathf.Sqrt(5))/2;var verts=new[]{new Vector3(-1,t,0),new Vector3(1,t,0),new Vector3(-1,-t,0),new Vector3(1,-t,0),new Vector3(0,-1,t),new Vector3(0,1,t),new Vector3(0,-1,-t),new Vector3(0,1,-t),new Vector3(t,0,-1),new Vector3(t,0,1),new Vector3(-t,0,-1),new Vector3(-t,0,1)};
            int[] faces={0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
            var depth=new float[Cell*Cell];for(int i=0;i<depth.Length;i++)depth[i]=-100;
            var rotation=Quaternion.Euler(24+frame*5,frame*15,18);for(int i=0;i<verts.Length;i++)verts[i]=verts[i].normalized;
            for(int i=0;i<faces.Length;i+=3){var a=verts[faces[i]];var b=verts[faces[i+1]];var c=verts[faces[i+2]];var ab=(a+b).normalized;var bc=(b+c).normalized;var ca=(c+a).normalized;
                Face(tile,depth,rotation*a,rotation*ab,rotation*ca,s,cracked,frame);Face(tile,depth,rotation*b,rotation*bc,rotation*ab,s,cracked,frame);Face(tile,depth,rotation*c,rotation*ca,rotation*bc,s,cracked,frame);Face(tile,depth,rotation*ab,rotation*bc,rotation*ca,s,cracked,frame);}
        }
        static void Face(Color[] tile,float[] depth,Vector3 a,Vector3 b,Vector3 c,IceVfxStyle s,bool cracked,int frame)
        {
            var n=Vector3.Cross(b-a,c-a).normalized;if(n.z<0)return;float light=Mathf.Clamp01(Vector3.Dot(n,new Vector3(-.35f,.6f,1).normalized));
            Color shade=light<.30f?s.deepNavy:light<.5f?s.deepBlue:light<.68f?s.iceBlue:light<.87f?s.cyan:s.pale;
            Vector2 Project(Vector3 v)=>new Vector2(48+v.x*37/(1-v.z*.12f),48+v.y*37/(1-v.z*.12f));var pa=Project(a);var pb=Project(b);var pc=Project(c);float den=(pb.y-pc.y)*(pa.x-pc.x)+(pc.x-pb.x)*(pa.y-pc.y);if(Mathf.Abs(den)<.001f)return;
            for(int y=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(pa.y,Mathf.Min(pb.y,pc.y))));y<=Mathf.Min(95,Mathf.CeilToInt(Mathf.Max(pa.y,Mathf.Max(pb.y,pc.y))));y++)for(int x=Mathf.Max(0,Mathf.FloorToInt(Mathf.Min(pa.x,Mathf.Min(pb.x,pc.x))));x<=Mathf.Min(95,Mathf.CeilToInt(Mathf.Max(pa.x,Mathf.Max(pb.x,pc.x))));x++){
                float u=((pb.y-pc.y)*(x-pc.x)+(pc.x-pb.x)*(y-pc.y))/den;float v=((pc.y-pa.y)*(x-pc.x)+(pa.x-pc.x)*(y-pc.y))/den;float w=1-u-v;if(u<0||v<0||w<0)continue;float z=u*a.z+v*b.z+w*c.z;int at=y*Cell+x;if(z<depth[at])continue;depth[at]=z;float edge=Mathf.Min(u,Mathf.Min(v,w));var color=shade;
                if(edge<.07f&&light>.67f)color=light>.86f?s.white:s.pale;
                // Broken surface facets widen across the eight crack frames.
                if(cracked&&edge<.035f+frame*.009f){color=frame<4?s.pale:s.white;if(frame>5&&(x+y)%7<2)color=s.deepNavy;}
                color.a=.94f;tile[at]=color;
            }
        }
        static Color Effect(int row,int f,int x,int y,IceVfxStyle s)
        {
            float px=(x-48)/44f,py=(y-48)/44f,r=Mathf.Sqrt(px*px+py*py),a=Mathf.Atan2(py,px),t=f/7f;
            if(row==2){ // A changing jagged bloom: white nucleus, pale fracture tongues, blue outer lobes.
                float boundary=(.3f+.63f*t)*(1+.17f*Mathf.Sin(a*9+f*.7f)+.10f*Mathf.Sin(a*17-f));float hole=Mathf.Max(0,t-.35f)*.85f;
                if(r<boundary&&r>hole){var c=r<boundary*.38f?s.white:r<boundary*.63f?s.pale:r<boundary*.83f?s.cyan:s.iceBlue;c.a=1-t*.8f;return c;}
            }
            if(row==3){ // Perspective ring with different front and rear brightness.
                float q=Mathf.Sqrt(px*px+py*py*2.8f),radius=.18f+t*.73f;
                if(Mathf.Abs(q-radius)<.065f*(1-t*.65f)){var c=py<0?s.pale:s.deepBlue;if(Mathf.Sin(a*13+f)>.6f)c=s.white;c.a=1-t*.8f;return c;}
            }
            if(row==4){ // Faceted frost crown, opening instead of scaling a complete ring.
                float peaks=.42f+.23f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(a*7+.3f)),5);float low=.1f+t*.48f;float high=peaks+Mathf.Sin(t*Mathf.PI)*.16f;
                if(r>low&&r<high){var c=r>high-.05f?s.pale:Mathf.Sin(a*7)>.3f?s.cyan:s.deepBlue;c.a=.9f-t*.7f;return c;}
            }
            if(row==5){float cross=Mathf.Min(Mathf.Abs(px),Mathf.Abs(py));float diagonal=Mathf.Abs(Mathf.Abs(px)-Mathf.Abs(py));if(r<.18f*(1-t*.6f))return s.white;if(r<.85f&&cross<.018f*(1-t*.4f)){var c=s.pale;c.a=(1-r)*(1-t);return c;}if(r<.5f&&diagonal<.025f){var c=s.cyan;c.a=1-t;return c;}}
            return Color.clear;
        }
    }
}
