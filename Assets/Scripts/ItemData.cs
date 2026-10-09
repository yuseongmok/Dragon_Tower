using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonTower
{
    public enum ItemGrade { Common, Rare, Epic, Unique, Legendary }
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
        public string id,displayName;
        public ItemAutoTrigger autoTrigger;
        public ItemMechanic mechanic;
        public float primaryValue,secondaryValue,duration;
        public int stacks=1;
    }
    [CreateAssetMenu(menuName="Dragon Tower/Item")]
    public sealed class ItemData : IdentifiedContent
    {
        public ItemAutoTrigger autoTrigger=new ItemAutoTrigger();
        // Non-attacking items have no offensive element, even when their art has an element palette.
        public ElementType DamageElement=>kind==ItemKind.Equipment&&autoTrigger!=null&&autoTrigger.enabled?autoTrigger.DamageElement:ElementType.Neutral;
        public bool hasLegacyConsumable;
        public ItemMechanic legacyMechanic;
        public float legacyPrimary,legacySecondary,legacyDuration;
        [NonSerialized] ItemData legacyConsumable;
        // Old charges remain usable; this clone is never registered in the loot database.
        public ItemData AsSavedConsumable(){
            if(kind==ItemKind.Consumable||!hasLegacyConsumable)return this;
            if(legacyConsumable==null){legacyConsumable=Instantiate(this);legacyConsumable.hideFlags=HideFlags.HideAndDontSave;legacyConsumable.kind=ItemKind.Consumable;legacyConsumable.mechanic=legacyMechanic;legacyConsumable.primaryValue=legacyPrimary;legacyConsumable.secondaryValue=legacySecondary;legacyConsumable.duration=legacyDuration;legacyConsumable.autoTrigger=new ItemAutoTrigger();legacyConsumable.effects=new List<ContentEffect>();}
            return legacyConsumable;
        }
        public ItemGrade grade;
        public ItemKind kind;
        public ItemMechanic mechanic;
        [Min(1)] public int maximumStacks=9;
        [Min(0)] public int price;
        public float primaryValue;
        public float secondaryValue;
        public float duration;
        public List<ContentEffect> effects=new List<ContentEffect>();
        public BattleItem Snapshot(int stacks=1)=>new BattleItem{id=StableId,autoTrigger=autoTrigger?.Copy(),displayName=displayName,mechanic=mechanic,
            primaryValue=primaryValue,secondaryValue=secondaryValue,duration=duration,stacks=Math.Max(1,stacks)};
    }
}
