using System;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class VisualRenewalSetup
    {
        const string Root="Assets/Art/ZephyrBattleRenewal/";
        static Sprite Import(string file,bool alpha)
        {
            AssetDatabase.Refresh();string path=Root+file;
            var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.filterMode=FilterMode.Point;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.alphaIsTransparency=alpha;t.maxTextureSize=2048;t.spritePixelsPerUnit=100;t.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        public static void VerifyPersistence()
        {
            ContentDataSetup.InstallForAutomation();
            var monster=AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/Data/Monsters/monster_rock_slime.asset");
            if(AssetDatabase.GetAssetPath(monster.battleSprite)!=Root+"rock-slime-v2.png")throw new Exception("Monster art reset by initializer");
            if(AssetDatabase.GetAssetPath(Resources.Load<BattleSkin>("PixelBattleSkin").towerBackground)!=Root+"tower-chamber-v2.png")throw new Exception("Background art reset");
            Debug.Log("RENEWAL_RELOAD_PERSISTENCE_OK");ZephyrIdleSetup.Run();
        }
        public static void Monster()
        {
            var sprite=Import("rock-slime-v2.png",true);if(sprite==null)throw new Exception("Monster sprite missing");
            var skin=Resources.Load<BattleSkin>("PixelBattleSkin");skin.rockSlime=sprite;skin.floorMonsters[0]=sprite;EditorUtility.SetDirty(skin);
            var monster=AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/Data/Monsters/monster_rock_slime.asset");monster.battleSprite=sprite;EditorUtility.SetDirty(monster);AssetDatabase.SaveAssets();
            Debug.Log("RENEWAL_MONSTER_LINK_OK");ZephyrIdleSetup.Run();
        }
        public static void Background()
        {
            var sprite=Import("tower-chamber-v2.png",false);if(sprite==null)throw new Exception("Background sprite missing");
            var skin=Resources.Load<BattleSkin>("PixelBattleSkin");skin.towerBackground=sprite;EditorUtility.SetDirty(skin);AssetDatabase.SaveAssets();
            Debug.Log("RENEWAL_BACKGROUND_LINK_OK");ZephyrIdleSetup.Run();
        }
    }
}
