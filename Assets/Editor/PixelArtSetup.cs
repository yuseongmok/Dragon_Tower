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
                evolution.npotScale=TextureImporterNPOTScale.None;evolution.maxTextureSize=2048;
                evolution.textureCompression=TextureImporterCompression.Uncompressed;
                return;
            }
            if (!assetPath.StartsWith("Assets/Art/PixelBattle/")) return;
            bool background=assetPath.EndsWith("tower-chamber.png")||assetPath.EndsWith("flooded-sewer.png")||assetPath.EndsWith("forge-depths.png");
            bool boss=assetPath.EndsWith("ancient-golem-boss.png")||assetPath.EndsWith("kraken-guardian-boss.png")||assetPath.EndsWith("forge-warden-vulcan.png");
            Configure((TextureImporter)assetImporter,background,boss);
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
            bool missingSecondArea=File.Exists(Root+"flooded-sewer.png")&&(skin==null||skin.floodedSewerBackground==null);
            bool missingThirdArea=File.Exists(Root+"forge-depths.png")&&(skin==null||skin.forgeDepthsBackground==null);
            bool missingNew=File.Exists("Assets/Art/Evolution/sol-evolution-sheet.png")&&(AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon8.asset")==null||AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon8.asset").evolutionSheet==null);
            if (!File.Exists(SkinPath) || missingDragon || missingSecondArea || missingThirdArea || missingNew ||
                (File.Exists(Root + "ancient-golem-boss.png") && (skin == null || skin.floorMonsters == null || skin.floorMonsters.Length < 5 || skin.ancientGolemBoss == null)))
                Install();
        }
        [MenuItem("Dragon Tower/Apply pixel art assets")]
        public static void Install()
        {
            string[] evolutionFiles={"sol","aurora","mir","titan","storm","sharkid","venom","chrono"};
            foreach(var id in evolutionFiles)if(File.Exists("Assets/Art/Evolution/"+id+"-evolution-sheet.png"))AssetDatabase.ImportAsset("Assets/Art/Evolution/"+id+"-evolution-sheet.png",ImportAssetOptions.ForceSynchronousImport);
            string[] files = { "ember-baby.png", "luna-baby.png", "zephyr-baby.png", "brandy-baby.png", "volt-baby.png", "okta-baby.png", "nova-baby.png", "dante-baby.png", "rock-slime.png", "small-golem.png", "dungeon-zombie.png", "cave-bat.png", "armored-skeleton.png", "ancient-golem-boss.png", "tower-chamber.png",
                "electric-jellyfish.png","mud-snail.png","water-gargoyle.png","volt-ray.png","kraken-guardian-boss.png","flooded-sewer.png",
                "gear-bat.png","magma-core.png","steam-sentinel.png","clockwork-duelist.png","forge-warden-vulcan.png","forge-depths.png" };
            foreach (var file in files)
            {
                if (!File.Exists(Root + file)) continue;
                AssetDatabase.ImportAsset(Root + file, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(Root + file);
                PixelArtImporter.Configure(importer, file == "tower-chamber.png"||file=="flooded-sewer.png"||file=="forge-depths.png",file == "ancient-golem-boss.png"||file=="kraken-guardian-boss.png"||file=="forge-warden-vulcan.png");
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
            skin.floodedSewerBackground = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "flooded-sewer.png");
            skin.forgeDepthsBackground = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "forge-depths.png");
            string[,] secondArea={
                {"Assets/Data/Monsters/monster_electric_jellyfish.asset","electric-jellyfish.png"},
                {"Assets/Data/Monsters/monster_mud_snail.asset","mud-snail.png"},
                {"Assets/Data/Monsters/monster_water_gargoyle.asset","water-gargoyle.png"},
                {"Assets/Data/Monsters/monster_volt_ray.asset","volt-ray.png"},
                {"Assets/Data/Monsters/boss_kraken_guardian.asset","kraken-guardian-boss.png"}};
            for(int i=0;i<secondArea.GetLength(0);i++)
            {
                var monster=AssetDatabase.LoadAssetAtPath<MonsterData>(secondArea[i,0]);var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+secondArea[i,1]);
                if(monster!=null&&sprite!=null){monster.battleSprite=sprite;EditorUtility.SetDirty(monster);}
            }
            string[,] thirdArea={
                {"Assets/Data/Monsters/monster_gear_bat.asset","gear-bat.png"},{"Assets/Data/Monsters/monster_magma_core.asset","magma-core.png"},
                {"Assets/Data/Monsters/monster_steam_sentinel.asset","steam-sentinel.png"},{"Assets/Data/Monsters/monster_clockwork_duelist.asset","clockwork-duelist.png"},
                {"Assets/Data/Monsters/boss_forge_warden_vulcan.asset","forge-warden-vulcan.png"}};
            for(int i=0;i<thirdArea.GetLength(0);i++){var monster=AssetDatabase.LoadAssetAtPath<MonsterData>(thirdArea[i,0]);var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+thirdArea[i,1]);if(monster!=null&&sprite!=null){monster.battleSprite=sprite;EditorUtility.SetDirty(monster);}}
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
            for(int i=0;i<evolutionFiles.Length;i++)
            {
                var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+(i+8)+".asset");
                if(dragon==null)continue;
                dragon.evolutionSheet=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Evolution/"+evolutionFiles[i]+"-evolution-sheet.png");
                string separated="Assets/Art/Evolution/Separated/"+evolutionFiles[i];
                dragon.battleSprite=AssetDatabase.LoadAssetAtPath<Sprite>(separated+"-base.png");
                dragon.intermediateEvolutionSprite=AssetDatabase.LoadAssetAtPath<Sprite>(separated+"-intermediate.png");
                dragon.finalEvolutionSprite=AssetDatabase.LoadAssetAtPath<Sprite>(separated+"-final.png");
                dragon.evolutionSheetIncludesBase=true;EditorUtility.SetDirty(dragon);
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
            foreach (var file in new[] { "ember-baby.png", "luna-baby.png", "zephyr-baby.png", "brandy-baby.png", "volt-baby.png", "okta-baby.png", "nova-baby.png", "dante-baby.png", "rock-slime.png", "small-golem.png", "dungeon-zombie.png", "cave-bat.png", "armored-skeleton.png", "ancient-golem-boss.png",
                "electric-jellyfish.png","mud-snail.png","water-gargoyle.png","volt-ray.png","kraken-guardian-boss.png","gear-bat.png","magma-core.png","steam-sentinel.png","clockwork-duelist.png","forge-warden-vulcan.png" })
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + file);
                if (sprite == null) continue;
                bool found = false;
                foreach (var packed in packables) if (packed == sprite) { found = true; break; }
                if (!found) atlas.Add(new Object[] { sprite });
            }
            AssetDatabase.SaveAssets();
            ContentDataSetup.Rebuild();
            Debug.Log("PIXEL_ART_READY: 128px sprites, <=512px background, point filtering, mipmaps/read-write off. Scene unchanged; press Play.");
        }
    }
}
