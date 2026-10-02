using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    // Repository-native pixel geometry authoring: no runtime mesh or extra shader required.
    public static class IceElementDepthSetup
    {
        const int Cell=96;
        public static void Validate()
        {
            Install();Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.IceDepthBatch.Validation");
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetInt("IceThrough",3);SessionState.SetBool("IceBatch",true);EditorApplication.isPlaying=true;
        }
        public static void Install()
        {
            const string folder="Assets/Resources/IceSkills/ElementDepth";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var style=AssetDatabase.LoadAssetAtPath<IceVfxStyle>(IceStyleSetup.StylePath);var tex=new Texture2D(768,288,TextureFormat.RGBA32,false);var pixels=new Color[768*288];
            for(int row=0;row<3;row++)for(int f=0;f<8;f++)for(int y=0;y<Cell;y++)for(int x=0;x<Cell;x++)pixels[(row*Cell+y)*768+f*Cell+x]=Pixel(row,f,x,y,style);
            tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(folder+"/Frames.png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.Refresh();
            var imp=(TextureImporter)AssetImporter.GetAtPath(folder+"/Frames.png");imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.spritePixelsPerUnit=100;imp.isReadable=false;imp.maxTextureSize=1024;imp.npotScale=TextureImporterNPOTScale.None;
            var slices=new SpriteMetaData[24];for(int r=0;r<3;r++)for(int f=0;f<8;f++)slices[r*8+f]=new SpriteMetaData{name="Element"+r+"_"+f,rect=new Rect(f*96,r*96,96,96),alignment=9,pivot=Vector2.one*.5f};
#pragma warning disable 618
            imp.spritesheet=slices;
#pragma warning restore 618
            imp.SaveAndReimport();AssetDatabase.SaveAssets();Debug.Log("ICE_ELEMENT_DEPTH_INSTALLED 24 frames, shared Ice palette/materials");
        }
        static Color Pixel(int row,int frame,int x,int y,IceVfxStyle s)
        {
            float px=(x-48)/44f,py=(y-48)/44f,t=frame/8f;
            if(row==1){
                // Irregular turbulent ribbon cross-section: advancing lobes, hollow dark folds, bright ridges.
                float bend=.16f*Mathf.Sin(py*5-t*6.283f),q=Mathf.Abs(px-bend);
                float edge=.58f+.15f*Mathf.Sin(py*9-t*6.283f)+.07f*Mathf.Sin(py*19+t*12.566f);
                if(Mathf.Abs(py)>.96f||q>edge)return Color.clear;
                float wave=(Mathf.Sin(py*6+px*8-t*6.283f)+Mathf.Sin(py*11-px*12+t*6.283f))*.5f;
                Color c=q>edge-.04f?s.deepBlue:wave>.65f?s.pale:wave>-.1f?s.cyan:s.iceBlue;
                if(q<.10f&&wave>.7f)c=s.white;c.a=Mathf.Abs(py)>.8f?.35f:.72f;return c;
            }
            // Two distinct pointed prisms: slender projectile and broad rooted growth crystal.
            float width=row==0?(py>.05f?(.98f-py)*.43f:(py+.98f)*.45f):py>.35f?(.98f-py)*.85f:.54f-(.35f-py)*.06f;
            if(py<-.93f||py>.98f||Mathf.Abs(px)>width)return Color.clear;
            float ridge=Mathf.Sin(t*6.283f)*width*.45f;float facet=(px-ridge)/Mathf.Max(.01f,width);
            Color shade=facet<-.55f?s.deepNavy:facet<0?s.iceBlue:facet<.55f?s.cyan:s.deepBlue;
            if(Mathf.Abs(px-ridge)<.026f)shade=s.white;
            if(Mathf.Abs(px)>width-.035f)shade=px<0?s.pale:s.deepNavy;
            if(row==2&&py<.35f&&py>-.1f&&px<ridge)shade=s.pale;
            if(row==0&&py>.3f&&px<ridge)shade=s.pale;
            return shade;
        }
    }
}
