using System;
using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName="Dragon Tower/Monster")]
    public sealed class MonsterData : IdentifiedContent
    {
        public ElementType elementType;
        [Min(1)] public int baseHP=100;
        [Min(0)] public int attackDamage=10;
        [Min(.1f)] public float attackInterval=2.4f;
        [Min(0)] public int hpGrowthPerFloor;
        [Min(0)] public float attackGrowthPerFloor;
        [Min(0)] public float hpGrowthPerFloorPercent;
        [Min(0)] public float attackGrowthPerFloorPercent;
        [Min(1)] public int minimumFloor=1;
        [Min(1)] public int maximumFloor=9;
        [Min(0)] public int spawnWeight=10;
        public bool boss;
        public Sprite battleSprite;
        public BattleEnemyStats CreateBattleStats(int floor)
        {
            int steps=Math.Max(0,floor-minimumFloor);
            float hpScale=1f+steps*hpGrowthPerFloorPercent/100f,attackScale=1f+steps*attackGrowthPerFloorPercent/100f;
            return new BattleEnemyStats{displayName=displayName,elementType=elementType,maxHP=Math.Max(1,(int)Math.Round((baseHP+steps*hpGrowthPerFloor)*hpScale)),
                damage=Math.Max(0,(int)Math.Round((attackDamage+steps*attackGrowthPerFloor)*attackScale)),interval=Math.Max(.1f,attackInterval)};
        }
    }
}
