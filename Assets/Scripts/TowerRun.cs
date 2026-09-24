using System;
using System.Collections.Generic;
using System.Linq;
namespace DragonTower
{
    public enum TowerRoomKind { Monster, Item, Augment, Recovery, Nest, Gold, Shop, Boss }
    public sealed class RunItemSlot
    {
        public ItemData Item { get; internal set; }
        public int Count { get; internal set; }
        public string DisplayName=>Item==null?"빈 슬롯":Item.displayName;
    }

    // Everything in this class belongs to one tower attempt and is discarded on defeat.
    public sealed class TowerRun
    {
        readonly List<string> items=new List<string>();
        readonly List<RunItemSlot> itemSlots=new List<RunItemSlot>();
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
        public int DamageReductionPercent { get; private set; }
        public int Gold { get; private set; }
        public int PendingLevelAugments { get; private set; }
        public int PendingEvolutionStage { get; private set; }
        public int MonstersDefeated { get; private set; }
        public bool Active { get; private set; }=true;
        public bool RoomChosen { get; private set; }
        public TowerRoomKind Room { get; private set; }
        public IReadOnlyList<TowerRoomKind> Choices => choices;
        public IReadOnlyList<string> Items => items;
        public IReadOnlyList<RunItemSlot> ItemSlots => itemSlots;
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
            int itemAttack=ItemEffect(ContentEffectType.AttackDamage),itemAttackPercent=ItemEffect(ContentEffectType.AttackDamagePercent);
            int itemSkillPercent=ItemEffect(ContentEffectType.SkillDamagePercent),itemSkillCooldown=ItemEffect(ContentEffectType.SkillCooldownPercent);
            int itemAttackCooldown=ItemEffect(ContentEffectType.AttackCooldownPercent),itemDodgeCooldown=ItemEffect(ContentEffectType.DodgeCooldownPercent);
            int itemDodgeDuration=ItemEffect(ContentEffectType.DodgeDurationPercent),itemCrit=ItemEffect(ContentEffectType.CriticalChancePercent);
            int itemCritDamage=ItemEffect(ContentEffectType.CriticalDamagePercent),itemReduction=ItemEffect(ContentEffectType.DamageReductionPercent);
            return new BattleStats{displayName=source.displayName,element=source.element,elementType=source.elementType,skillEffect=replacedSkill==null?source.skillEffect:replacedSkill.effectKind,maxHP=MaxHP,attackDamage=Math.Max(1,(int)Math.Round((source.attackDamage+AttackBonus+itemAttack)*(1+(AttackDamagePercent+itemAttackPercent)/100f))),
                attackCooldown=ScaleTime(source.attackCooldown,AttackCooldownPercent+itemAttackCooldown,.08f),dodgeCooldown=ScaleTime(source.dodgeCooldown,DodgeCooldownPercent+itemDodgeCooldown,.2f),
                dodgeDuration=Math.Max(.05f,source.dodgeDuration*(1+(DodgeDurationPercent+itemDodgeDuration)/100f)),criticalChance=Math.Max(0,source.criticalChance+(CriticalChancePercent+itemCrit)/100f),criticalDamage=Math.Max(1,source.criticalDamage+(CriticalDamagePercent+itemCritDamage)/100f),damageReductionPercent=Math.Max(0,Math.Min(80,DamageReductionPercent+itemReduction)),skillDisabled=SkillDisabled,augments=augmentData.Select(x=>x.Snapshot()).ToArray(),items=itemSlots.Where(x=>x.Item!=null&&x.Item.kind==ItemKind.Equipment).Select(x=>x.Item.Snapshot(x.Count)).ToArray(),
                skill=new SkillStats{displayName=selected.displayName,elementType=selected.elementType,damage=Math.Max(1,(int)Math.Round(selected.damage*(1+(SkillDamagePercent+itemSkillPercent)/100f))),
                    cooldown=SkillCooldownZero?0:ScaleTime(selected.cooldown,SkillCooldownPercent+itemSkillCooldown,.2f),hitCount=Math.Max(1,selected.hitCount),hitInterval=Math.Max(.03f,selected.hitInterval),
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
        public bool CanAddItem(ItemData item)
        {
            if(item==null)return false;
            var same=itemSlots.FirstOrDefault(x=>x.Item!=null&&x.Item.StableId==item.StableId);
            return (same!=null&&same.Count<Math.Max(1,item.maximumStacks))||itemSlots.Count<2;
        }
        public void AddItem(ItemData item,int replaceIndex=-1)
        {
            if(item==null)throw new ArgumentNullException(nameof(item));
            var same=itemSlots.FirstOrDefault(x=>x.Item!=null&&x.Item.StableId==item.StableId);
            if(same!=null&&same.Count<Math.Max(1,item.maximumStacks)){same.Count++;RecalculateMaxHP();return;}
            if(itemSlots.Count<2){itemSlots.Add(new RunItemSlot{Item=item,Count=1});items.Add(item.StableId);RecalculateMaxHP();return;}
            if(replaceIndex<0||replaceIndex>=itemSlots.Count)throw new InvalidOperationException("아이템 슬롯이 가득 찼습니다. 교체할 아이템을 선택하세요.");
            itemSlots[replaceIndex]=new RunItemSlot{Item=item,Count=1};items[replaceIndex]=item.StableId;RecalculateMaxHP();
        }
        public ItemData ItemAt(int index)=>index>=0&&index<itemSlots.Count?itemSlots[index].Item:null;
        public int ItemCountAt(int index)=>index>=0&&index<itemSlots.Count?itemSlots[index].Count:0;
        public void ConsumeItem(int index)
        {
            if(index<0||index>=itemSlots.Count)throw new ArgumentOutOfRangeException(nameof(index));
            var slot=itemSlots[index];if(slot.Item==null||slot.Item.kind!=ItemKind.Consumable)throw new InvalidOperationException("소모품만 사용할 수 있습니다.");
            slot.Count--;if(slot.Count<=0){itemSlots.RemoveAt(index);items.RemoveAt(index);}RecalculateMaxHP();
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
                    case ContentEffectType.DamageReductionPercent:DamageReductionPercent+=value;break;
                }
            }
        }
        int ItemEffect(ContentEffectType type)
        {
            float total=0;
            foreach(var slot in itemSlots)
            {
                if(slot.Item==null||slot.Item.kind!=ItemKind.Equipment||slot.Item.effects==null)continue;
                foreach(var effect in slot.Item.effects)if(effect!=null&&effect.type==type)total+=effect.value*Math.Max(1,slot.Count);
            }
            return (int)Math.Round(total,MidpointRounding.AwayFromZero);
        }
        void RecalculateMaxHP()
        {
            int previous=MaxHP;int itemFlat=ItemEffect(ContentEffectType.MaxHP),itemPercent=ItemEffect(ContentEffectType.MaxHPPercent);
            MaxHP=Math.Max(1,(int)Math.Round((BaseMaxHP+FlatMaxHPBonus+itemFlat)*(1+(MaxHPPercent+itemPercent)/100f)));
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
