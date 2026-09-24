using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonTower
{
    public enum ItemGrade { Common, Rare, Epic, Unique }
    public enum ItemKind { Consumable, Equipment }
    public enum ItemMechanic
    {
        None,TimeShard,BurstCore,LastStand,GiantRoar,BloodTome,PurificationRod,
        FrostWitchTear,InfernoBreath,WeaknessLens,EmergencyAccelerator,StunGun,
        HealingPotion,GreaterHealingPotion,ExecutionerMark,PoisonFang,MeteorFragment,
        InfightingGlove,GuardianBrooch,PhoenixFeather,BloodPotion,BarrierStone,EchoCrystal
    }
    [Serializable]
    public sealed class BattleItem
    {
        public string displayName;
        public ItemMechanic mechanic;
        public float primaryValue,secondaryValue,duration;
        public int stacks=1;
    }
    [CreateAssetMenu(menuName="Dragon Tower/Item")]
    public sealed class ItemData : IdentifiedContent
    {
        public ItemGrade grade;
        public ItemKind kind;
        public ItemMechanic mechanic;
        [Min(1)] public int maximumStacks=9;
        [Min(0)] public int price;
        public float primaryValue;
        public float secondaryValue;
        public float duration;
        public List<ContentEffect> effects=new List<ContentEffect>();
        public BattleItem Snapshot(int stacks=1)=>new BattleItem{displayName=displayName,mechanic=mechanic,
            primaryValue=primaryValue,secondaryValue=secondaryValue,duration=duration,stacks=Math.Max(1,stacks)};
    }
}
