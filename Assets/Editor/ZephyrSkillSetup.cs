using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class ZephyrSkillSetup
    {
        [MenuItem("Dragon Tower/Zephyr/Install and validate Skill")]
        public static void Run()
        {
            const string sheet="Assets/Art/ZephyrSkill/zephyr-skill-v2.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(sheet);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.isReadable=true;importer.maxTextureSize=2048;
            importer.GetSourceTextureWidthAndHeight(out int w,out int h);
            var slices=new SpriteMetaData[4];
            // Equal-size windows retain the third frame's wing beyond the nominal grid line.
            // Windows contain transparent borders and never include adjacent characters.
            if(w!=1254||h!=1254)throw new System.Exception("Review slicing for changed skill source dimensions");
            for(int i=0;i<4;i++)slices[i]=new SpriteMetaData{name="Zephyr_Skill_"+i,rect=new Rect(i%2==0?40:647,i<2?632:14,600,600),pivot=new Vector2(.5f,.5f),alignment=0};
            #pragma warning disable 618
            importer.spritesheet=slices;
            #pragma warning restore 618
            importer.SaveAndReimport();
            var frames=AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(frames.Length!=4)throw new System.Exception("Expected four skill frames");
            foreach(var frame in frames)CheckBorder(frame);
            const string path="Assets/Art/ZephyrSkill/ZephyrSkill.asset";
            var clip=AssetDatabase.LoadAssetAtPath<DragonIdleFrames>(path);
            if(clip==null){clip=ScriptableObject.CreateInstance<DragonIdleFrames>();AssetDatabase.CreateAsset(clip,path);}
            clip.frames=frames;clip.durations=new[]{.08f,.10f,.10f,.12f};clip.offsets=new Vector2[4];
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");
            // Match the established idle root; foreclaw motion must not shift the entire sheet.
            var idle=dragon.idleFrames.frames[0];
            clip.displayScale=VisibleHeight(idle)/idle.rect.height/(VisibleHeight(frames[3])/frames[3].rect.height);
            var center=new Vector2(.5f,.5f);
            var anchor=ZephyrIdleSetup.FootAnchor(idle)/idle.rect.width-center;
            for(int i=0;i<4;i++)clip.offsets[i]=anchor-(ZephyrIdleSetup.FootAnchor(frames[i])/frames[i].rect.width-center)*clip.displayScale;
            dragon.skillFrames=clip;EditorUtility.SetDirty(clip);EditorUtility.SetDirty(dragon);AssetDatabase.SaveAssets();
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
            for(int x=0;x<width;x++)if(p[x].a>.1f||p[(height-1)*width+x].a>.1f)throw new System.Exception("Skill sprite touches horizontal crop border");
            for(int y=0;y<height;y++)if(p[y*width].a>.1f||p[y*width+width-1].a>.1f)throw new System.Exception("Skill sprite touches vertical crop border");
        }
    }
}

