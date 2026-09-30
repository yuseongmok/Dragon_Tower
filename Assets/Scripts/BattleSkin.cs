using UnityEngine;
namespace DragonTower
{
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

        public Sprite BackgroundForFloor(int floor)
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
