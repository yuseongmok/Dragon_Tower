using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace DragonTower.Editor
{
    public class PixelArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (assetPath.StartsWith("Assets/Art/Evolution/"))
            {
                var evolution=(TextureImporter)assetImporter;
                evolution.textureType=TextureImporterType.Default;
                evolution.filterMode=FilterMode.Point;evolution.wrapMode=TextureWrapMode.Clamp;
                evolution.mipmapEnabled=false;evolution.isReadable=false;evolution.alphaIsTransparency=true;
                evolution.npotScale=TextureImporterNPOTScale.None;evolution.maxTextureSize=512;
                evolution.textureCompression=TextureImporterCompression.Uncompressed;
                return;
            }
            if (!assetPath.StartsWith("Assets/Art/PixelBattle/")) return;
            Configure((TextureImporter)assetImporter, assetPath.EndsWith("tower-chamber.png"),assetPath.EndsWith("ancient-golem-boss.png"));
        }
        public static void Configure(TextureImporter importer, bool background,bool boss=false)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.alphaIsTransparency = !background;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = background ? 512 : boss ? 256 : 128;
            // Small RGBA32 textures avoid block-compression artifacts around dark pixel outlines.
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }

    [InitializeOnLoad]
    public static class PixelArtSetup
    {
        const string Root = "Assets/Art/PixelBattle/";
        const string SkinPath = "Assets/Resources/PixelBattleSkin.asset";
        static PixelArtSetup()
        {
            EditorApplication.delayCall += EnsureReady;
        }
        static void EnsureReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += EnsureReady; return; }
            if (!File.Exists(Root + "tower-chamber.png")) return;
            var skin = AssetDatabase.LoadAssetAtPath<BattleSkin>(SkinPath);
            bool missingDragon=false;
            string[] dragonFiles={"ember-baby.png","luna-baby.png","zephyr-baby.png","brandy-baby.png","volt-baby.png","okta-baby.png","nova-baby.png","dante-baby.png"};
            for(int i=0;i<dragonFiles.Length;i++){var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+i+".asset");if(File.Exists(Root+dragonFiles[i])&&dragon!=null&&dragon.battleSprite==null)missingDragon=true;}
            if (!File.Exists(SkinPath) || missingDragon ||
                (File.Exists(Root + "ancient-golem-boss.png") && (skin == null || skin.floorMonsters == null || skin.floorMonsters.Length < 5 || skin.ancientGolemBoss == null)))
                Install();
        }
        [MenuItem("Dragon Tower/Apply pixel art assets")]
        public static void Install()
        {
            string[] files = { "ember-baby.png", "luna-baby.png", "zephyr-baby.png", "brandy-baby.png", "volt-baby.png", "okta-baby.png", "nova-baby.png", "dante-baby.png", "rock-slime.png", "small-golem.png", "dungeon-zombie.png", "cave-bat.png", "armored-skeleton.png", "ancient-golem-boss.png", "tower-chamber.png" };
            foreach (var file in files)
            {
                if (!File.Exists(Root + file)) continue;
                AssetDatabase.ImportAsset(Root + file, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(Root + file);
                PixelArtImporter.Configure(importer, file == "tower-chamber.png",file == "ancient-golem-boss.png");
                importer.SaveAndReimport();
            }
            Directory.CreateDirectory("Assets/Resources");
            var skin = AssetDatabase.LoadAssetAtPath<BattleSkin>(SkinPath);
            if (skin == null) { skin = ScriptableObject.CreateInstance<BattleSkin>(); AssetDatabase.CreateAsset(skin, SkinPath); }
            skin.babyDragon = AssetDatabase.LoadAssetAtPath<Sprite>(Root + files[0]);
            skin.rockSlime = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "rock-slime.png");
            skin.floorMonsters = new[] {
                skin.rockSlime,
                AssetDatabase.LoadAssetAtPath<Sprite>(Root + "small-golem.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>(Root + "dungeon-zombie.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>(Root + "cave-bat.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>(Root + "armored-skeleton.png") };
            skin.ancientGolemBoss = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "ancient-golem-boss.png");
            skin.towerBackground = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "tower-chamber.png");
            string[] dragonFiles = { "ember-baby.png", "luna-baby.png", "zephyr-baby.png", "brandy-baby.png", "volt-baby.png", "okta-baby.png", "nova-baby.png", "dante-baby.png" };
            for (int i = 0; i < dragonFiles.Length; i++)
            {
                var dragon = AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon" + i + ".asset");
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + dragonFiles[i]);
                if (dragon != null && dragon.battleSprite == null && sprite != null)
                {
                    dragon.battleSprite = sprite;
                    EditorUtility.SetDirty(dragon);
                }
            }
            EditorUtility.SetDirty(skin);
            const string atlasPath = "Assets/Art/PixelBattle/Combat.spriteatlas";
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                atlas.SetPackingSettings(new SpriteAtlasPackingSettings { padding = 4, enableRotation = false, enableTightPacking = false });
                atlas.SetTextureSettings(new SpriteAtlasTextureSettings { readable = false, generateMipMaps = false, filterMode = FilterMode.Point, sRGB = true });
                atlas.SetPlatformSettings(new TextureImporterPlatformSettings { name = "DefaultTexturePlatform", maxTextureSize = 512, textureCompression = TextureImporterCompression.Uncompressed, format = TextureImporterFormat.RGBA32 });
                AssetDatabase.CreateAsset(atlas, atlasPath);
            }
            var packables = atlas.GetPackables();
            foreach (var file in new[] { "ember-baby.png", "luna-baby.png", "zephyr-baby.png", "brandy-baby.png", "volt-baby.png", "okta-baby.png", "nova-baby.png", "dante-baby.png", "rock-slime.png", "small-golem.png", "dungeon-zombie.png", "cave-bat.png", "armored-skeleton.png", "ancient-golem-boss.png" })
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + file);
                if (sprite == null) continue;
                bool found = false;
                foreach (var packed in packables) if (packed == sprite) { found = true; break; }
                if (!found) atlas.Add(new Object[] { sprite });
            }
            AssetDatabase.SaveAssets();
            Debug.Log("PIXEL_ART_READY: 128px sprites, <=512px background, point filtering, mipmaps/read-write off. Scene unchanged; press Play.");
        }
    }
}
