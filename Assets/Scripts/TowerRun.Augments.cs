using System;
using System.Linq;

namespace DragonTower
{
    public sealed partial class TowerRun
    {
        public void ConsumeEmptyAugmentReward(bool levelReward){if(levelReward&&PendingLevelAugments>0)PendingLevelAugments--;}
        bool HasAugment(AugmentMechanic mechanic)=>augmentData.Any(a=>a.mechanic==mechanic);
        public bool HasLegendaryAugment=>augmentData.Any(a=>a.grade==AugmentGrade.Legendary);
        public bool AugmentCompatible(AugmentData candidate)
        {
            if(candidate.grade==AugmentGrade.Legendary&&(HasLegendaryAugment||Level<20||PendingEvolutionStage>0))return false;
            foreach(var id in candidate.prerequisites??Array.Empty<string>())if(!augments.Contains(id))return false;
            foreach(var owned in augmentData)
                if((candidate.exclusions??Array.Empty<string>()).Contains(owned.StableId)||(owned.exclusions??Array.Empty<string>()).Contains(candidate.StableId))return false;
            var r=candidate.requirements;
            bool seal=HasAugment(AugmentMechanic.Seal)||(candidate.effects??new System.Collections.Generic.List<ContentEffect>()).Any(e=>e.type==ContentEffectType.SkillDisabled&&e.value!=0);
            if(seal&&(r&AugmentRequirement.CommonSkill)!=0)return false;
            if(seal&&augmentData.Any(a=>(a.requirements&AugmentRequirement.CommonSkill)!=0))return false;
            var skill=CurrentSkill(definition==null?null:definition.SkillAtLevel(Level));
            var passive=definition==null?DragonPassiveMechanic.None:definition.passiveMechanic;
            bool usable=skill!=null&&(!seal||skill.IsSignatureSkill);
            bool burn=passive==DragonPassiveMechanic.FlameBreath||HasAugment(AugmentMechanic.FlameRemnant)||(usable&&skill.statusEffect==CombatStatusEffect.Burn);
            bool poison=passive==DragonPassiveMechanic.HydraVenom||HasAugment(AugmentMechanic.VenomFang)||(usable&&skill.statusEffect==CombatStatusEffect.Poison);
            bool slow=passive==DragonPassiveMechanic.FreezingGaze||HasAugment(AugmentMechanic.IceCream)||(usable&&skill.statusEffect==CombatStatusEffect.Slow);
            bool status=burn||poison||slow||passive==DragonPassiveMechanic.ElectricShock||passive==DragonPassiveMechanic.TimeRift||HasAugment(AugmentMechanic.Tingly)||(usable&&skill.statusEffect!=CombatStatusEffect.None);
            bool shield=passive==DragonPassiveMechanic.IronArmor||HasAugment(AugmentMechanic.CastingWard)||HasAugment(AugmentMechanic.FrostBarrier)||HasAugment(AugmentMechanic.LivingFortress)||itemSlots.Any(i=>i.Item!=null&&i.Item.mechanic==ItemMechanic.BarrierStone);
            bool hpCost=passive==DragonPassiveMechanic.VoidAccelerator||HasAugment(AugmentMechanic.ManaRampage)||HasAugment(AugmentMechanic.Vampire);
            bool crit=CriticalChancePercent+ItemEffect(ContentEffectType.CriticalChancePercent)>0||HasAugment(AugmentMechanic.EvasiveCounter)||HasAugment(AugmentMechanic.CriticalDestiny)||HasAugment(AugmentMechanic.PatientAim)||(usable&&skill.targetCriticalBonus>0);
            return ((r&AugmentRequirement.Burn)==0||burn)&&((r&AugmentRequirement.Poison)==0||poison)&&((r&AugmentRequirement.Slow)==0||slow)&&((r&AugmentRequirement.Shield)==0||shield)&&((r&AugmentRequirement.HpCost)==0||hpCost)&&((r&AugmentRequirement.Critical)==0||crit)&&((r&AugmentRequirement.AnyStatus)==0||status);
        }
        float AugmentValue(AugmentMechanic mechanic)=>augmentData.Where(a=>a.mechanic==mechanic).Sum(a=>a.primaryValue);
        SkillStats BuildAugmentedSkill(SkillStats selected,int itemSkillPercent,int affinityPassiveBonus,int itemSkillCooldown)
        {
            var skill=selected.Copy();
            float damageFactor=Math.Max(0,1+(SkillDamagePercent+itemSkillPercent+affinityPassiveBonus)/100f);
            skill.damage=Math.Max(1,(int)Math.Round(skill.damage*damageFactor));
            skill.attackEmpowerDamage=Math.Max(0,(int)Math.Round(skill.attackEmpowerDamage*damageFactor));
            skill.celestialExtraDamage=Math.Max(0,(int)Math.Round(skill.celestialExtraDamage*damageFactor));
            int reduction=SkillCooldownPercent+itemSkillCooldown;
            if(!skill.isSignature&&string.IsNullOrEmpty(skill.exclusiveDragonId))
            {
                if(HasAugment(AugmentMechanic.DoubleCasting))reduction-=(int)augmentData.First(a=>a.mechanic==AugmentMechanic.DoubleCasting).secondaryValue;
                if(HasAugment(AugmentMechanic.Vampire))reduction+=(int)augmentData.First(a=>a.mechanic==AugmentMechanic.Vampire).duration;
            }
            skill.cooldown=SkillCooldownZero?0:ScaleTime(selected.cooldown,reduction,.2f);
            return skill;
        }
    }
}
