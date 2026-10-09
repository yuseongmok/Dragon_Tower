using System;using System.Linq;using UnityEngine;
namespace DragonTower {
[Serializable] public sealed class SavedItemSlot {public string id;public int count;public bool legacyConsumable;}
[Serializable] public sealed class TowerRunSave {public int version=1;public string speciesId,skillId;public TowerRoomKind[] choices;public string[] augments,items;public SavedItemSlot[] slots;public BattleResumeData battle;
public ItemCooldownSave[] itemCooldowns;
public int inventoryVersion;public SavedItemSlot consumable;public SavedItemSlot[] pendingInventory;
public bool ExperiencePresentationPending;public int PreviousExperienceLevel,PreviousExperience;
public int Floor;
public int Level;
public int Experience;
public int LastExperienceGain;
public int CurrentHP;
public int MaxHP;
public int BaseMaxHP;
public int FlatMaxHPBonus;
public int AttackBonus;
public int MaxHPPercent;
public int AttackDamagePercent;
public int SkillDamagePercent;
public int CriticalChancePercent;
public int CriticalDamagePercent;
public bool SkillDisabled;
public bool SkillCooldownZero;
public int SkillCooldownPercent;
public int AttackCooldownPercent;
public int DodgeCooldownPercent;
public int DodgeDurationPercent;
public int DamageReductionPercent;
public int Gold;
public int PendingLevelAugments;
public int PendingEvolutionStage;
public int MonstersDefeated;
public int BossesDefeated;public RunProgressRecord progress;
public bool Active;
public bool RoomChosen;
public bool PassiveAvailable;
public TowerRoomKind Room;
public FloorPhase Phase;
public bool RewardClaimed;
public bool EncounterDecided;
public bool AdditionalBattle;
public int BattleExperiencePercent;
public bool BattleExperienceClaimed;
public int RewardSeed;public bool ChestOpened;public string[] ChestRewardIds;
public int LastBattleGold;public bool PostBattleShopPending;
public bool DropDecided;
public string PendingDropId;
public string InstanceId;
}
public sealed partial class TowerRun {
public TowerRunSave Capture(bool includeBattle=true)=>new TowerRunSave{ChestOpened=ChestOpened,ChestRewardIds=ChestRewardIds==null?null:(string[])ChestRewardIds.Clone(),itemCooldowns=ItemCooldowns.Select(x=>new ItemCooldownSave{itemId=x.itemId,slot=x.slot,remaining=x.remaining}).ToArray(),inventoryVersion=2,consumable=consumable==null?null:new SavedItemSlot{id=consumable.Item.StableId,count=consumable.Count},pendingInventory=pendingInventory.Select(x=>new SavedItemSlot{id=x.Item.StableId,count=x.Count,legacyConsumable=x.Item.kind==ItemKind.Consumable&&x.Item.hasLegacyConsumable}).ToArray(),LastBattleGold=LastBattleGold,PostBattleShopPending=PostBattleShopPending,BossesDefeated=BossesDefeated,progress=JsonUtility.FromJson<RunProgressRecord>(JsonUtility.ToJson(Progress)),ExperiencePresentationPending=ExperiencePresentationPending,PreviousExperienceLevel=PreviousExperienceLevel,PreviousExperience=PreviousExperience,Floor=Floor,Level=Level,Experience=Experience,LastExperienceGain=LastExperienceGain,CurrentHP=CurrentHP,MaxHP=MaxHP,BaseMaxHP=BaseMaxHP,FlatMaxHPBonus=FlatMaxHPBonus,AttackBonus=AttackBonus,MaxHPPercent=MaxHPPercent,AttackDamagePercent=AttackDamagePercent,SkillDamagePercent=SkillDamagePercent,CriticalChancePercent=CriticalChancePercent,CriticalDamagePercent=CriticalDamagePercent,SkillDisabled=SkillDisabled,SkillCooldownZero=SkillCooldownZero,SkillCooldownPercent=SkillCooldownPercent,AttackCooldownPercent=AttackCooldownPercent,DodgeCooldownPercent=DodgeCooldownPercent,DodgeDurationPercent=DodgeDurationPercent,DamageReductionPercent=DamageReductionPercent,Gold=Gold,PendingLevelAugments=PendingLevelAugments,PendingEvolutionStage=PendingEvolutionStage,MonstersDefeated=MonstersDefeated,Active=Active,RoomChosen=RoomChosen,PassiveAvailable=PassiveAvailable,Room=Room,Phase=Phase,RewardClaimed=RewardClaimed,EncounterDecided=EncounterDecided,AdditionalBattle=AdditionalBattle,BattleExperiencePercent=BattleExperiencePercent,BattleExperienceClaimed=BattleExperienceClaimed,RewardSeed=RewardSeed,DropDecided=DropDecided,PendingDropId=PendingDropId,InstanceId=InstanceId,speciesId=dragonId,skillId=replacedSkill==null?null:replacedSkill.StableId,choices=(TowerRoomKind[])choices.Clone(),augments=augments.ToArray(),items=items.ToArray(),slots=itemSlots.Select(x=>new SavedItemSlot{id=x.Item.StableId,count=x.Count}).ToArray(),battle=includeBattle?BattleResumeCodec.Pack(BattleResume):null};
public static TowerRun Restore(TowerRunSave s,ContentDatabase db,bool preserveLegacyInventory=false){
 if(s==null||s.version!=1||s.Floor<1||s.Level<1||s.choices==null||s.choices.Length==0)throw new InvalidOperationException("도전 저장을 읽을 수 없습니다.");
 var d=db.dragons.FirstOrDefault(x=>x.StableId==s.speciesId);if(d==null)throw new InvalidOperationException("저장된 드래곤을 찾을 수 없습니다.");
 var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);
r.ChestOpened=s.ChestOpened;r.ChestRewardIds=s.ChestRewardIds==null?null:(string[])s.ChestRewardIds.Clone();
r.ExperiencePresentationPending=s.ExperiencePresentationPending;r.PreviousExperienceLevel=s.PreviousExperienceLevel;r.PreviousExperience=s.PreviousExperience;
r.ItemCooldowns=s.itemCooldowns??Array.Empty<ItemCooldownSave>();
r.Floor=s.Floor;
r.Level=s.Level;
r.Experience=s.Experience;
r.LastExperienceGain=s.LastExperienceGain;
r.CurrentHP=s.CurrentHP;
r.MaxHP=s.MaxHP;
r.BaseMaxHP=s.BaseMaxHP;
r.FlatMaxHPBonus=s.FlatMaxHPBonus;
r.AttackBonus=s.AttackBonus;
r.MaxHPPercent=s.MaxHPPercent;
r.AttackDamagePercent=s.AttackDamagePercent;
r.SkillDamagePercent=s.SkillDamagePercent;
r.CriticalChancePercent=s.CriticalChancePercent;
r.CriticalDamagePercent=s.CriticalDamagePercent;
r.SkillDisabled=s.SkillDisabled;
r.SkillCooldownZero=s.SkillCooldownZero;
r.SkillCooldownPercent=s.SkillCooldownPercent;
r.AttackCooldownPercent=s.AttackCooldownPercent;
r.DodgeCooldownPercent=s.DodgeCooldownPercent;
r.DodgeDurationPercent=s.DodgeDurationPercent;
r.DamageReductionPercent=s.DamageReductionPercent;
r.Gold=s.Gold;
r.PendingLevelAugments=s.PendingLevelAugments;
r.PendingEvolutionStage=s.PendingEvolutionStage;
r.MonstersDefeated=s.MonstersDefeated;
r.BossesDefeated=s.BossesDefeated;
if(s.progress!=null&&!string.IsNullOrEmpty(s.progress.runId))r.Progress=JsonUtility.FromJson<RunProgressRecord>(JsonUtility.ToJson(s.progress));
else {
 // Older saves did not record scoring. Baseline historical counters without retroactive points.
 r.BossesDefeated=Math.Min(s.MonstersDefeated,(s.Floor-1)/10+(s.Room==TowerRoomKind.Boss&&s.BattleExperienceClaimed?1:0));
 r.Progress=new RunProgressRecord{runId=Guid.NewGuid().ToString("N"),normalKills=Math.Max(0,s.MonstersDefeated-r.BossesDefeated),bossKills=r.BossesDefeated,maxFloor=s.Floor,maxBond=s.Level};
}
r.Active=s.Active;
r.RoomChosen=s.RoomChosen;
r.PassiveAvailable=s.PassiveAvailable;
r.Room=s.Room;
r.Phase=s.Phase;
r.RewardClaimed=s.RewardClaimed;
r.EncounterDecided=s.EncounterDecided;
r.AdditionalBattle=s.AdditionalBattle;
r.BattleExperiencePercent=s.BattleExperiencePercent;
r.BattleExperienceClaimed=s.BattleExperienceClaimed;
r.RewardSeed=s.RewardSeed;
r.LastBattleGold=s.LastBattleGold;r.PostBattleShopPending=s.PostBattleShopPending;
r.DropDecided=s.DropDecided;
r.PendingDropId=s.PendingDropId;
r.InstanceId=s.InstanceId;
 r.choices=(TowerRoomKind[])s.choices.Clone();r.items.AddRange(s.items??Array.Empty<string>());
 foreach(var id in s.augments??Array.Empty<string>()){var a=db.augments.FirstOrDefault(x=>x.StableId==id);if(a==null)throw new InvalidOperationException("저장된 증강을 찾을 수 없습니다.");r.augments.Add(id);r.augmentData.Add(a);}
 foreach(var slot in s.slots??Array.Empty<SavedItemSlot>()){var item=db.items.FirstOrDefault(x=>x.StableId==slot.id);if(item==null||slot.count<1)throw new InvalidOperationException("저장된 아이템을 찾을 수 없습니다.");if(s.inventoryVersion<1&&item.hasLegacyConsumable)item=item.AsSavedConsumable();r.itemSlots.Add(new RunItemSlot{Item=item,Count=slot.count});}
 if(!preserveLegacyInventory){
  var legacy=r.itemSlots.ToArray();r.itemSlots.Clear();
  foreach(var slot in legacy){
   if(slot.Item.kind==ItemKind.Equipment&&!r.OwnsEquipment(slot.Item)&&r.itemSlots.Count<3){r.itemSlots.Add(new RunItemSlot{Item=slot.Item,Count=1});if(slot.Count>1)r.pendingInventory.Add(new RunItemSlot{Item=slot.Item,Count=slot.Count-1});}
   else if(slot.Item.kind==ItemKind.Consumable&&r.consumable==null){r.consumable=new RunItemSlot{Item=slot.Item,Count=Math.Min(3,slot.Count)};if(slot.Count>3)r.pendingInventory.Add(new RunItemSlot{Item=slot.Item,Count=slot.Count-3});}
   else r.pendingInventory.Add(slot);
  }
  if(s.consumable!=null&&!string.IsNullOrEmpty(s.consumable.id)){var item=db.items.FirstOrDefault(x=>x.StableId==s.consumable.id)?.AsSavedConsumable();if(item==null||item.kind!=ItemKind.Consumable||s.consumable.count<1||s.consumable.count>3)throw new InvalidOperationException("소모품 저장을 확인해주세요.");r.consumable=new RunItemSlot{Item=item,Count=s.consumable.count};}
  foreach(var slot in s.pendingInventory??Array.Empty<SavedItemSlot>()){var item=db.items.FirstOrDefault(x=>x.StableId==slot.id);if(item==null||slot.count<1)throw new InvalidOperationException("보관 아이템 저장을 확인해주세요.");if((s.inventoryVersion<2||slot.legacyConsumable)&&item.hasLegacyConsumable)item=item.AsSavedConsumable();r.pendingInventory.Add(new RunItemSlot{Item=item,Count=slot.count});}
  r.RefreshItemIds();if(s.inventoryVersion<1&&s.Phase!=FloorPhase.CombatActive)r.RecalculateMaxHP();
 }
 if(!string.IsNullOrEmpty(s.skillId)){r.replacedSkill=db.skills.FirstOrDefault(x=>x.StableId==s.skillId);if(r.replacedSkill==null)throw new InvalidOperationException("저장된 스킬을 찾을 수 없습니다.");}
 r.BattleResume=s.battle!=null&&!string.IsNullOrEmpty(s.battle.initialRunJson)?BattleResumeCodec.Unpack(s.battle):null;return r;
}
}
}
