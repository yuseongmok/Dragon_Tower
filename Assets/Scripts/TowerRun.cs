using System;
using System.Collections.Generic;
using System.Linq;
namespace DragonTower
{
    public enum TowerRoomKind { Monster, Item, Augment, Recovery, Nest, Gold, Shop, Boss }

    // Everything in this class belongs to one tower attempt and is discarded on defeat.
    public sealed class TowerRun
    {
        readonly List<string> items=new List<string>();
        readonly List<string> augments=new List<string>();
        readonly List<AugmentData> augmentData=new List<AugmentData>();
        readonly ElementType dragonElement;
        SkillData replacedSkill;
        TowerRoomKind[] choices;
        public int Floor { get; private set; }=1;
        public int Level { get; private set; }=1;
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public int BaseMaxHP { get; private set; }
        public int FlatMaxHPBonus { get; private set; }
        public int AttackBonus { get; private set; }
        public int MaxHPPercent { get; private set; }
        public int AttackDamagePercent { get; private set; }
        public int SkillDamagePercent { get; private set; }
        public int CriticalChancePercent { get; private set; }
        public int CriticalDamagePercent { get; private set; }
        public bool SkillDisabled { get; private set; }
        public bool SkillCooldownZero { get; private set; }
        public int SkillCooldownPercent { get; private set; }
        public int AttackCooldownPercent { get; private set; }
        public int DodgeCooldownPercent { get; private set; }
        public int DodgeDurationPercent { get; private set; }
        public int Gold { get; private set; }
        public int PendingLevelAugments { get; private set; }
        public int PendingEvolutionStage { get; private set; }
        public int MonstersDefeated { get; private set; }
        public bool Active { get; private set; }=true;
        public bool RoomChosen { get; private set; }
        public TowerRoomKind Room { get; private set; }
        public IReadOnlyList<TowerRoomKind> Choices => choices;
        public IReadOnlyList<string> Items => items;
        public IReadOnlyList<string> Augments => augments;

        public TowerRun(int maxHP,int firstRoll,int secondRoll,ElementType element=ElementType.Neutral)
        {
            if(maxHP<=0)throw new ArgumentOutOfRangeException(nameof(maxHP));
            dragonElement=element;BaseMaxHP=MaxHP=CurrentHP=maxHP;PrepareChoices(firstRoll,secondRoll);
        }
        public SkillData CurrentSkill(SkillData fallback)=>replacedSkill??fallback;
        public string CurrentSkillId(SkillData fallback){var skill=CurrentSkill(fallback);return skill==null?string.Empty:skill.StableId;}
        public void ReplaceSkill(SkillData skill,bool levelReward)
        {
            if(skill==null)throw new ArgumentNullException(nameof(skill));
            if(skill.elementType!=dragonElement)throw new InvalidOperationException("현재 드래곤과 같은 속성의 스킬만 배울 수 있습니다.");
            replacedSkill=skill;if(levelReward&&PendingLevelAugments>0)PendingLevelAugments--;
        }
        public void ChooseRoom(int index)
        {
            if(!Active||RoomChosen||index<0||index>=choices.Length)throw new InvalidOperationException("Room cannot be selected.");
            Room=choices[index];RoomChosen=true;
        }
        public void AdvanceFloor(int firstRoll,int secondRoll)
        {
            if(!Active||!RoomChosen)throw new InvalidOperationException("Current room is not complete.");
            Floor++;RoomChosen=false;PrepareChoices(firstRoll,secondRoll);
        }
        void PrepareChoices(int firstRoll,int secondRoll)
        {
            if(Floor%10==0){choices=new[]{TowerRoomKind.Boss};return;}
            var first=Pick(Floor,firstRoll);var second=Pick(Floor,secondRoll);
            if(second==first)second=Pick(Floor,(secondRoll+37)%100);
            if(second==first)second=first==TowerRoomKind.Monster?TowerRoomKind.Item:TowerRoomKind.Monster;
            choices=new[]{first,second};
        }
        public static TowerRoomKind Pick(int floor,int roll)
        {
            if(floor<1)throw new ArgumentOutOfRangeException(nameof(floor));
            if(floor%10==0)return TowerRoomKind.Boss;
            if(roll<0||roll>99)throw new ArgumentOutOfRangeException(nameof(roll));
            // Provisional weights, kept here so later balancing changes one table only.
            if(roll<45)return TowerRoomKind.Monster;
            if(roll<57)return TowerRoomKind.Item;
            if(roll<67)return TowerRoomKind.Augment;
            if(roll<77)return TowerRoomKind.Recovery;
            if(roll<80)return TowerRoomKind.Nest;
            if(roll<92)return TowerRoomKind.Gold;
            return TowerRoomKind.Shop;
        }
        public BattleStats BuildBattleStats(BattleStats source)
        {
            var selected=replacedSkill==null?source.skill:replacedSkill.Snapshot();
            return new BattleStats{displayName=source.displayName,element=source.element,elementType=source.elementType,skillEffect=replacedSkill==null?source.skillEffect:replacedSkill.effectKind,maxHP=MaxHP,attackDamage=Math.Max(1,(int)Math.Round((source.attackDamage+AttackBonus)*(1+AttackDamagePercent/100f))),
                attackCooldown=ScaleTime(source.attackCooldown,AttackCooldownPercent,.08f),dodgeCooldown=ScaleTime(source.dodgeCooldown,DodgeCooldownPercent,.2f),
                dodgeDuration=Math.Max(.05f,source.dodgeDuration*(1+DodgeDurationPercent/100f)),criticalChance=Math.Max(0,source.criticalChance+CriticalChancePercent/100f),criticalDamage=Math.Max(1,source.criticalDamage+CriticalDamagePercent/100f),skillDisabled=SkillDisabled,augments=augmentData.Select(x=>x.Snapshot()).ToArray(),
                skill=new SkillStats{displayName=selected.displayName,elementType=selected.elementType,damage=Math.Max(1,(int)Math.Round(selected.damage*(1+SkillDamagePercent/100f))),
                    cooldown=SkillCooldownZero?0:ScaleTime(selected.cooldown,SkillCooldownPercent,.2f),hitCount=Math.Max(1,selected.hitCount),hitInterval=Math.Max(.03f,selected.hitInterval),
                    statusEffect=selected.statusEffect,statusChancePercent=selected.statusChancePercent,statusDuration=selected.statusDuration,statusPower=selected.statusPower}};
        }
        static float ScaleTime(float value,int reductionPercent,float minimum)=>Math.Max(minimum,value*(1-reductionPercent/100f));
        public void RecordBattleVictory(int remainingHP)
        {
            int previousStage=DragonData.EvolutionStage(Level);
            CurrentHP=Math.Max(0,Math.Min(MaxHP,remainingHP));MonstersDefeated++;Level++;
            int currentStage=DragonData.EvolutionStage(Level);
            if(currentStage>previousStage)PendingEvolutionStage=currentStage;
            if(Level%5==0)PendingLevelAugments++;
        }
        public void ConsumeEvolution()
        {
            if(PendingEvolutionStage<=0)throw new InvalidOperationException("No evolution is pending.");
            PendingEvolutionStage=0;
        }
        public int Heal(int amount)
        {
            if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));
            int before=CurrentHP;CurrentHP=Math.Min(MaxHP,CurrentHP+amount);return CurrentHP-before;
        }
        public int HealFull()=>Heal(MaxHP);
        public void AddGold(int amount){if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));Gold+=amount;}
        public bool SpendGold(int amount){if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));if(Gold<amount)return false;Gold-=amount;return true;}
        public void AddItem(string id)
        {
            if(string.IsNullOrEmpty(id))throw new ArgumentException(nameof(id));items.Add(id);
            if(id=="healing-potion")Heal(35);
            else if(id=="iron-scale"){FlatMaxHPBonus+=15;RecalculateMaxHP();}
            else if(id=="sharp-claw")AttackBonus+=2;
        }
        public void AddItem(ItemData item)
        {
            if(item==null)throw new ArgumentNullException(nameof(item));items.Add(item.StableId);ApplyEffects(item.effects);
        }
        public void AddAugment(AugmentData augment,bool levelReward)
        {
            if(augment==null)throw new ArgumentNullException(nameof(augment));
            if(!CanTakeAugment(augment))throw new InvalidOperationException("이미 최대 중첩에 도달한 증강입니다.");
            augments.Add(augment.StableId);augmentData.Add(augment);ApplyEffects(augment.effects);if(levelReward&&PendingLevelAugments>0)PendingLevelAugments--;
        }
        public bool CanTakeAugment(AugmentData augment)
        {
            if(augment==null)return false;
            int stacks=0;foreach(var id in augments)if(id==augment.StableId)stacks++;
            return stacks<Math.Max(1,augment.maximumStacks);
        }
        void ApplyEffects(IReadOnlyList<ContentEffect> effects)
        {
            if(effects==null)return;
            foreach(var effect in effects)
            {
                if(effect==null)continue;int value=(int)Math.Round(effect.value);
                switch(effect.type)
                {
                    case ContentEffectType.Heal:Heal(Math.Max(0,value));break;
                    case ContentEffectType.MaxHP:FlatMaxHPBonus+=Math.Max(0,value);RecalculateMaxHP();break;
                    case ContentEffectType.AttackDamage:AttackBonus+=value;break;
                    case ContentEffectType.SkillDamagePercent:SkillDamagePercent+=value;break;
                    case ContentEffectType.SkillCooldownPercent:SkillCooldownPercent+=value;break;
                    case ContentEffectType.AttackCooldownPercent:AttackCooldownPercent+=value;break;
                    case ContentEffectType.DodgeCooldownPercent:DodgeCooldownPercent+=value;break;
                    case ContentEffectType.DodgeDurationPercent:DodgeDurationPercent+=value;break;
                    case ContentEffectType.Gold:AddGold(Math.Max(0,value));break;
                    case ContentEffectType.MaxHPPercent:MaxHPPercent+=value;RecalculateMaxHP();break;
                    case ContentEffectType.AttackDamagePercent:AttackDamagePercent+=value;break;
                    case ContentEffectType.CriticalChancePercent:CriticalChancePercent+=value;break;
                    case ContentEffectType.CriticalDamagePercent:CriticalDamagePercent+=value;break;
                    case ContentEffectType.SkillDisabled:SkillDisabled=value!=0;break;
                    case ContentEffectType.SkillCooldownSetZero:SkillCooldownZero=value!=0;break;
                }
            }
        }
        void RecalculateMaxHP()
        {
            int previous=MaxHP;MaxHP=Math.Max(1,(int)Math.Round((BaseMaxHP+FlatMaxHPBonus)*(1+MaxHPPercent/100f)));
            if(MaxHP>previous)CurrentHP+=MaxHP-previous;else CurrentHP=Math.Min(CurrentHP,MaxHP);
        }
        public void AddAugment(string id,bool levelReward)
        {
            if(string.IsNullOrEmpty(id))throw new ArgumentException(nameof(id));augments.Add(id);
            if(levelReward&&PendingLevelAugments>0)PendingLevelAugments--;
        }
        public void End(){Active=false;CurrentHP=0;}
    }
}
