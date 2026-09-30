using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DragonTower.Editor
{
    public sealed class UpperTowerArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UpperTowerArtSetup.Root)) return;
            var importer = (TextureImporter)assetImporter;
            bool sheet = Path.GetFileName(assetPath).StartsWith("enemies-");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = sheet ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = sheet;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = sheet ? 2048 : 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            if (!sheet) return;
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            var slices = new SpriteMetaData[5];
            for (int i = 0; i < 5; i++)
            {
                float left = (i % 3) / 3f, right = (i % 3 + 1) / 3f;
                // The ice knight's sword extends left of the nominal grid line.
                if (assetPath.EndsWith("enemies-51.png") && i == 0) right = 460f / 1536;
                if (assetPath.EndsWith("enemies-51.png") && i == 1) left = 460f / 1536;
                if (assetPath.EndsWith("enemies-31.png") && i == 4) right = 1060f / 1536;
                float sliceHeight = height / 2f;
                if (assetPath.EndsWith("enemies-41.png") && i == 4) sliceHeight = height * 534f / 1024;
                slices[i] = new SpriteMetaData {
                    name = "enemy-" + i,
                    rect = new Rect(left * width, i < 3 ? height / 2f : 0, (right-left) * width, sliceHeight),
                    alignment = (int)SpriteAlignment.Center, pivot = new Vector2(.5f,.5f)
                };
            }
#pragma warning disable 618
            importer.spritesheet = slices;
#pragma warning restore 618
        }
    }

    [InitializeOnLoad]
    public static class UpperTowerArtSetup
    {
        public const string Root = "Assets/Art/UpperTower/";
        static readonly string[][] Ids = {
            new[]{"4aabdc0e69f37b94caf0852110291a51","892b34e00dc5a4343bdbc79ccf9c2af2","1e8cb809eae50de4a905a611cb7c6517","9ebf8b68058577c4f8440cb0b8260734","93f027707baa1f040b938fe171ad640f"},
            new[]{"51715ef097bca064f802fc3b54a9a2d1","439c8f7a54dda0a42abfb710d4f81c61","8d70c1f9c98c8c5438479ca101665d1e","e208ba17cc9ede54e86151578a6a600e","9b956bed861f91543aff4b6fe0a127f9"},
            new[]{"9ee2d2e87852570438a463948878615a","7cda777449c43e1429003fe5f151c7bc","15b7c38b9dc257940886c0713f4dd129","2abff68d63785714eb18ffa9ed363376","ec7faf1d2df907a4fb0e3af01893fe1a"},
            new[]{"9a371ed99f820554ba6c65f6eedb6b17","9cd14504b1fa6d34097302c7c7cc18ed","a80d2cc2164bebe4fbf3620ce747e7ab","556906bc5d4476a4fb1f64e58b1c4f25","38434e60d305ca649beda241bfaa33aa"},
            new[]{"f6ed6779aaacdf94ca2cf7cbdfb7b12c","894b6a6aaf6beb04a9ca0936bc8b1c5e","080257ca1105bb446871b25e5b27a0d9","e5a823f91f7357f4faaa98bded2543ba","36064c57eb658944daa0e80b8b648b8e"}
        };
        static UpperTowerArtSetup() { EditorApplication.delayCall += EnsureReady; }
        static void EnsureReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += EnsureReady; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Root + "enemies-71.png")) return;
            var skin = Resources.Load<BattleSkin>("PixelBattleSkin");
            if (skin != null && (skin.upperTowerBackgrounds == null || skin.upperTowerBackgrounds.Length != 5 || skin.upperTowerBackgrounds.Any(s => s == null))) Install();
            else if (skin != null) Validate();
        }
        [MenuItem("Dragon Tower/Apply 31-80 floor artwork")]
        public static void Install()
        {
            var skin = Resources.Load<BattleSkin>("PixelBattleSkin");
            var database = ContentDatabase.Load();
            if (skin == null || database == null) throw new InvalidOperationException("Missing battle skin or content database.");
            var monsters = AssetDatabase.FindAssets("t:MonsterData", new[]{"Assets/Data/Monsters"})
                .Select(g => AssetDatabase.LoadAssetAtPath<MonsterData>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            var registered = (database.monsters ?? Array.Empty<MonsterData>()).Where(m => m != null).ToList();
            var backgrounds = new Sprite[5];
            for (int area = 0; area < 5; area++)
            {
                int start = 31 + area * 10;
                string backgroundPath = Root + "area-" + start + ".png", sheetPath = Root + "enemies-" + start + ".png";
                AssetDatabase.ImportAsset(backgroundPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset(sheetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                backgrounds[area] = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundPath);
                var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
                for (int slot = 0; slot < 5; slot++)
                {
                    var monster = monsters.Single(m => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m)) == Ids[area][slot]);
                    var sprite = sprites.Single(s => s.name == "enemy-" + slot);
                    monster.battleSprite = sprite;
                    EditorUtility.SetDirty(monster);
                    if (!registered.Contains(monster)) registered.Add(monster);
                }
            }
            skin.upperTowerBackgrounds = backgrounds;
            database.monsters = registered.ToArray();
            EditorUtility.SetDirty(skin); EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Validate();
        }
        [MenuItem("Dragon Tower/Validate 31-80 floors")]
        public static void Validate()
        {
            var database = ContentDatabase.Load(); var skin = Resources.Load<BattleSkin>("PixelBattleSkin");
            for (int floor = 31; floor <= 80; floor++)
            {
                bool boss = floor % 10 == 0;
                int area = (floor - 31) / 10;
                if (skin.BackgroundForFloor(floor) != skin.upperTowerBackgrounds[area]) throw new Exception("Background mismatch: " + floor);
                for (int roll = 0; roll < 100; roll++)
                {
                    var monster = database.PickMonster(floor, boss, roll);
                    if (monster == null || monster.battleSprite == null || !Ids[area].Contains(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(monster))) || monster.boss != boss)
                        throw new Exception("Encounter mismatch: " + floor + "/" + roll);
                }
            }
            File.WriteAllText("Library/upper-tower-validation.txt", "PASS: 31-80 backgrounds and 5000 encounter selections; 20 monsters and 5 bosses linked. " + DateTime.Now);
            Debug.Log("UPPER_TOWER_VALIDATION_OK");
        }
    }
}

