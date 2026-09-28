using System;
using UnityEngine;
namespace DragonTower
{
    public enum EnemyBossPattern { None, EarthShatter, AbyssalRush, ForgeBarrier }
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
        [Header("Attack timing")]
        [Range(0,40)] public float timingVariancePercent;
        [Range(0,60)] public float quickAttackChancePercent;
        [Range(.35f,1)] public float quickAttackIntervalMultiplier=.6f;
        [Header("Boss pattern")]
        public EnemyBossPattern bossPattern;
        [Min(2)] public int patternEveryAttacks=4;
        [Range(.35f,2)] public float patternIntervalMultiplier=1;
        [Range(.25f,3)] public float patternDamageMultiplier=1;
        [Header("Forge barrier pattern")]
        [Min(0)] public int barrierHP;
        [Min(.1f)] public float barrierDuration=4.5f;
        [Range(0,100)] public float barrierFailureDamagePercent=35;
        public Sprite battleSprite;
        public BattleEnemyStats CreateBattleStats(int floor)
        {
            int steps=Math.Max(0,floor-minimumFloor);
            float hpScale=1f+steps*hpGrowthPerFloorPercent/100f,attackScale=1f+steps*attackGrowthPerFloorPercent/100f;
            return new BattleEnemyStats{displayName=displayName,elementType=elementType,maxHP=Math.Max(1,(int)Math.Round((baseHP+steps*hpGrowthPerFloor)*hpScale)),
                damage=Math.Max(0,(int)Math.Round((attackDamage+steps*attackGrowthPerFloor)*attackScale)),interval=Math.Max(.1f,attackInterval),
                timingVariancePercent=timingVariancePercent,quickAttackChancePercent=quickAttackChancePercent,
                quickAttackIntervalMultiplier=Math.Max(.35f,quickAttackIntervalMultiplier),bossPattern=bossPattern,
                patternEveryAttacks=Math.Max(2,patternEveryAttacks),patternIntervalMultiplier=Math.Max(.35f,patternIntervalMultiplier),
                patternDamageMultiplier=Math.Max(.25f,patternDamageMultiplier),barrierHP=Math.Max(0,barrierHP),
                barrierDuration=Math.Max(.1f,barrierDuration),barrierFailureDamagePercent=Math.Max(0,barrierFailureDamagePercent)};
        }
    }
}
