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
    }
}
