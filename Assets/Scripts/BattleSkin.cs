using UnityEngine;
namespace DragonTower
{
    public enum EnvironmentScreen { Battle, Item, Stairs, BossEntrance, BossBattle }
    [System.Serializable]
    public sealed class EnvironmentBackgroundSet
    {
        public int firstFloor=1,lastFloor=10;
        public Sprite battle,item,stairs,bossEntrance,bossBattle;
        public Texture2D doorAtlas;
        public Sprite Get(EnvironmentScreen screen)
        {
            switch(screen){case EnvironmentScreen.Item:return item;case EnvironmentScreen.Stairs:return stairs;case EnvironmentScreen.BossEntrance:return bossEntrance;case EnvironmentScreen.BossBattle:return bossBattle;default:return battle;}
        }
    }
    // One small, shared resource for this prototype battle. Future floors can load separate skins.
    public class BattleSkin : ScriptableObject
    {
        public Sprite babyDragon;
        public Sprite rockSlime;
        public Sprite[] floorMonsters;
        public Sprite ancientGolemBoss;
        public Sprite towerBackground;
        public Sprite floodedSewerBackground;
        public Sprite forgeDepthsBackground;
        public Sprite[] upperTowerBackgrounds;

        public EnvironmentBackgroundSet[] environmentBackgrounds;
        public Texture2D DoorAtlasForFloor(int floor)
        {
            if(environmentBackgrounds!=null)foreach(var set in environmentBackgrounds)
                if(set!=null&&floor>=set.firstFloor&&floor<=set.lastFloor&&set.doorAtlas!=null)return set.doorAtlas;
            return Resources.Load<Texture2D>("UI/tower-doors");
        }
        public Sprite BackgroundForScreen(int floor,EnvironmentScreen screen)
        {
            if(environmentBackgrounds!=null)foreach(var set in environmentBackgrounds)
                if(set!=null&&floor>=set.firstFloor&&floor<=set.lastFloor){var sprite=set.Get(screen);if(sprite!=null)return sprite;}
            switch(screen)
            {
                case EnvironmentScreen.Item:return Resources.Load<Sprite>("UI/treasure-chamber");
                case EnvironmentScreen.Stairs:return Resources.Load<Sprite>("UI/tower-stairways");
                case EnvironmentScreen.BossEntrance:return Resources.Load<Sprite>("UI/boss-stairway");
                default:return LegacyBackgroundForFloor(floor);
            }
        }
        public Sprite BackgroundForFloor(int floor)=>BackgroundForScreen(floor,EnvironmentScreen.Battle);
        Sprite LegacyBackgroundForFloor(int floor)
        {
            if (floor >= 31 && floor <= 80)
            {
                int region = (floor - 31) / 10;
                if (upperTowerBackgrounds != null && region < upperTowerBackgrounds.Length && upperTowerBackgrounds[region] != null)
                    return upperTowerBackgrounds[region];
            }
            if (floor >= 21 && forgeDepthsBackground != null) return forgeDepthsBackground;
            if (floor >= 11 && floodedSewerBackground != null) return floodedSewerBackground;
            return towerBackground;
        }
    }
}
