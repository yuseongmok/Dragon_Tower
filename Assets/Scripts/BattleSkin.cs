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
        public string regionId,regionName;
        public int regionNumber;
        public bool playEntrance;
        public bool forgeEffects,replayDiscovered;
        public bool skyEffects;
        public bool thunderEffects;
        public string entranceMessage;
        public Color entranceTint=new Color(.16f,.78f,.79f,1);
        public Color particleTint=new Color(.45f,.92f,1,1);
        [Range(2,8)] public float entranceSeconds=4.2f;
        [Range(0,64)] public int particleCount=28;
        public Sprite Get(EnvironmentScreen screen)
        {
            switch(screen){case EnvironmentScreen.Item:return item;case EnvironmentScreen.Stairs:return stairs;case EnvironmentScreen.BossEntrance:return bossEntrance;case EnvironmentScreen.BossBattle:return bossBattle;default:return battle;}
        }
    }
    // One small, shared resource for this prototype battle. Future floors can load separate skins.
    public class BattleSkin : ScriptableObject
    {
        readonly System.Collections.Generic.HashSet<string> missingBackgrounds=new System.Collections.Generic.HashSet<string>();
        public Sprite babyDragon;
        public Sprite rockSlime;
        public Sprite[] floorMonsters;
        public Sprite ancientGolemBoss;
        public Sprite towerBackground;
        public Sprite floodedSewerBackground;
        public Sprite forgeDepthsBackground;
        public Sprite[] upperTowerBackgrounds;

        public EnvironmentBackgroundSet[] environmentBackgrounds;
        public EnvironmentBackgroundSet EntranceForFloor(int floor)
        {
            if(environmentBackgrounds!=null)foreach(var set in environmentBackgrounds)
                if(set!=null&&set.firstFloor==floor&&set.playEntrance&&!string.IsNullOrEmpty(set.regionId))return set;
            return null;
        }
        public Texture2D DoorAtlasForFloor(int floor)
        {
            if(environmentBackgrounds!=null)foreach(var set in environmentBackgrounds)
                if(set!=null&&floor>=set.firstFloor&&floor<=set.lastFloor&&set.doorAtlas!=null)return set.doorAtlas;
            return Resources.Load<Texture2D>("UI/tower-doors");
        }
        public Sprite BackgroundForScreen(int floor,EnvironmentScreen screen)
        {
            if(environmentBackgrounds!=null)foreach(var set in environmentBackgrounds)
                if(set!=null&&floor>=set.firstFloor&&floor<=set.lastFloor)
                {
                    var sprite=set.Get(screen);if(sprite!=null)return sprite;
                    string key=set.firstFloor+":"+screen;
                    if(missingBackgrounds.Add(key))Debug.LogWarning("Environment background missing: "+key+". Using legacy fallback.");
                }
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
