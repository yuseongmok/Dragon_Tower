using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class ZephyrAttackSetup
    {
        [MenuItem("Dragon Tower/Zephyr/Install and validate Attack")]
        public static void Run()
        {
            const string sheet="Assets/Art/ZephyrAttack/zephyr-attack.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(sheet);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.isReadable=true;importer.maxTextureSize=2048;
            importer.GetSourceTextureWidthAndHeight(out int w,out int h);
            var slices=new SpriteMetaData[4];
            for(int i=0;i<4;i++)slices[i]=new SpriteMetaData{name="Zephyr_Attack_"+i,rect=new Rect(i%2*w/2,(1-i/2)*h/2,w/2,h/2),pivot=new Vector2(.5f,.5f),alignment=0};
            #pragma warning disable 618
            importer.spritesheet=slices;
            #pragma warning restore 618
            importer.SaveAndReimport();
            var frames=AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(frames.Length!=4)throw new System.Exception("Expected four attack frames");
            const string path="Assets/Art/ZephyrAttack/ZephyrAttack.asset";
            var clip=AssetDatabase.LoadAssetAtPath<DragonIdleFrames>(path);
            if(clip==null){clip=ScriptableObject.CreateInstance<DragonIdleFrames>();AssetDatabase.CreateAsset(clip,path);}
            clip.frames=frames;clip.durations=new[]{.06f,.06f,.06f,.06f};clip.offsets=new Vector2[4];
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");
            // Match the established idle root; foreclaw motion must not shift the entire sheet.
            var idle=dragon.idleFrames.frames[0];
            var anchor=ZephyrIdleSetup.FootAnchor(idle)/idle.rect.width;
            for(int i=0;i<4;i++)clip.offsets[i]=anchor-ZephyrIdleSetup.FootAnchor(frames[i])/frames[i].rect.width;
            dragon.attackFrames=clip;EditorUtility.SetDirty(clip);EditorUtility.SetDirty(dragon);AssetDatabase.SaveAssets();
            ZephyrIdleSetup.Run();
        }
    }
}
