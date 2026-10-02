using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class ZephyrDeathSetup
    {
        [MenuItem("Dragon Tower/Zephyr/Install and validate Death")]
        public static void Run()
        {
            const string sheet="Assets/Art/ZephyrDeath/zephyr-death.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(sheet);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.isReadable=true;importer.maxTextureSize=2048;
            importer.GetSourceTextureWidthAndHeight(out int w,out int h);
            var slices=new SpriteMetaData[4];
            // Four equal cells; border checks below reject clipped limbs or wings.
            if(w!=1254||h!=1254)throw new System.Exception("Review slicing for changed death source dimensions");
            for(int i=0;i<4;i++)slices[i]=new SpriteMetaData{name="Zephyr_Death_"+i,rect=new Rect(i%2*w/2,(1-i/2)*h/2,w/2,h/2),pivot=new Vector2(.5f,.5f),alignment=0};
            #pragma warning disable 618
            importer.spritesheet=slices;
            #pragma warning restore 618
            importer.SaveAndReimport();
            var frames=AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(frames.Length!=4)throw new System.Exception("Expected four death frames");
            foreach(var frame in frames)CheckBorder(frame);
            const string path="Assets/Art/ZephyrDeath/ZephyrDeath.asset";
            var clip=AssetDatabase.LoadAssetAtPath<DragonIdleFrames>(path);
            if(clip==null){clip=ScriptableObject.CreateInstance<DragonIdleFrames>();AssetDatabase.CreateAsset(clip,path);}
            clip.frames=frames;clip.durations=new[]{.10f,.14f,.16f,.20f};clip.offsets=new Vector2[4];
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");
            // Use the upright first frame to preserve scale as the body collapses.
            var idle=dragon.idleFrames.frames[0];
            clip.displayScale=VisibleHeight(idle)/idle.rect.height/(VisibleHeight(frames[0])/frames[0].rect.height);
            var center=new Vector2(.5f,.5f);
            var anchor=ZephyrIdleSetup.FootAnchor(idle)/idle.rect.width-center;
            for(int i=0;i<4;i++)clip.offsets[i]=anchor-(ZephyrIdleSetup.FootAnchor(frames[i])/frames[i].rect.width-center)*clip.displayScale;
            dragon.deathFrames=clip;EditorUtility.SetDirty(clip);EditorUtility.SetDirty(dragon);AssetDatabase.SaveAssets();
            ZephyrIdleSetup.Run();
        }
        static float VisibleHeight(Sprite sprite)
        {
            var r=sprite.rect;var pixels=sprite.texture.GetPixels((int)r.x,(int)r.y,(int)r.width,(int)r.height);
            int min=(int)r.height,max=0,width=(int)r.width;
            for(int y=0;y<(int)r.height;y++)for(int x=0;x<width;x++)if(pixels[y*width+x].a>.5f){min=Mathf.Min(min,y);max=Mathf.Max(max,y);}
            return Mathf.Max(1,max-min+1);
        }
        static void CheckBorder(Sprite sprite)
        {
            var r=sprite.rect;var p=sprite.texture.GetPixels((int)r.x,(int)r.y,(int)r.width,(int)r.height);int width=(int)r.width,height=(int)r.height;
            for(int x=0;x<width;x++)if(p[x].a>.1f||p[(height-1)*width+x].a>.1f)throw new System.Exception("Death sprite touches horizontal crop border");
            for(int y=0;y<height;y++)if(p[y*width].a>.1f||p[y*width+width-1].a>.1f)throw new System.Exception("Death sprite touches vertical crop border");
        }
    }
}




