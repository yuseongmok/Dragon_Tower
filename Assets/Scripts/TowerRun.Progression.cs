using System;
using System.Collections.Generic;
using System.Linq;
namespace DragonTower {
 public enum FloorPhase { Choosing, RoomEvent, CombatPending, CombatActive, BattleRewards, Complete, Ended, PostBattleShop }
 [Serializable] public sealed class BattleInputRecord { public int action;public float delta;public string itemId; }
 [Serializable] public sealed class BattleResumeData {public string initialRunJson;public int seed;public BattleEnemyStats enemy;public string monsterId;public string packedInputs;public List<BattleInputRecord> inputs=new List<BattleInputRecord>();}
 [Serializable] public sealed class RunResultRecord { public string instanceId;public int floor,level,monsters;public bool cleared;public RunProgressRecord progress; }
 public sealed partial class TowerRun {
  public FloorPhase Phase {get;private set;}
  public bool ChestOpened {get;private set;}
  public string[] ChestRewardIds {get;private set;}
  public void OpenChest(string[] ids){if(Phase!=FloorPhase.RoomEvent||Room!=TowerRoomKind.Item||ChestOpened)return;ChestOpened=true;ChestRewardIds=ids??Array.Empty<string>();}
  public bool ExperiencePresentationPending {get;private set;}
  public int PreviousExperienceLevel {get;private set;}
  public int PreviousExperience {get;private set;}
  public void AcknowledgeExperience(){ExperiencePresentationPending=false;}
  public bool RewardClaimed {get;private set;}
  public bool EncounterDecided {get;private set;}
  public bool AdditionalBattle {get;private set;}
  public int BattleExperiencePercent {get;private set;}
  public bool BattleExperienceClaimed {get;private set;}
  public int RewardSeed {get;private set;}
  public int LastBattleGold {get;private set;}
  public bool PostBattleShopPending {get;private set;}
  public bool DropDecided {get;private set;}
  public string PendingDropId {get;private set;}
  public string InstanceId {get;private set;}
  public BattleResumeData BattleResume {get;set;}
  public void AttachInstance(string id){InstanceId=id;}
  public void BeginFloorRoom(int index,int seed){ChooseRoom(index);RewardSeed=seed;Phase=Room==TowerRoomKind.Monster||Room==TowerRoomKind.Boss?FloorPhase.CombatPending:FloorPhase.RoomEvent;}
  public bool CompleteRoomEvent(FloorProgressionConfig config,int chanceRoll,int expRoll){
   if(Phase!=FloorPhase.RoomEvent||RewardClaimed)return false;
   RewardClaimed=true;GrantExperience(config.RoomExperience(Room));EncounterDecided=true;
   AdditionalBattle=chanceRoll<config.BattleChance(Room);BattleExperiencePercent=AdditionalBattle?config.PickExperienceMultiplier(expRoll):0;
   Phase=AdditionalBattle?FloorPhase.CombatPending:FloorPhase.Complete;return true;
  }
  public void BeginFloorCombat(FloorProgressionConfig config,int expRoll){
   if(Phase==FloorPhase.CombatActive)return;if(Phase!=FloorPhase.CombatPending)throw new InvalidOperationException("Battle is not pending");
   if(!EncounterDecided){EncounterDecided=true;BattleExperiencePercent=config.PickExperienceMultiplier(expRoll);}Phase=FloorPhase.CombatActive;
  }
  public bool CompleteFloorCombat(int hp,FloorProgressionConfig config,int shopRoll=100){
   if(Phase!=FloorPhase.CombatActive||BattleExperienceClaimed)return false;
   BattleExperienceClaimed=true;LastBattleGold=Math.Max(0,Room==TowerRoomKind.Boss?config.bossKillGold:config.normalKillGold);AddGold(LastBattleGold);PostBattleShopPending=Room==TowerRoomKind.Monster&&shopRoll<config.monsterShopChance;if(Room==TowerRoomKind.Boss)BossesDefeated++;RecordBattleVictory(hp,(int)Math.Round(config.baseBattleExperience*BattleExperiencePercent/100f));RecalculateMaxHP();Phase=FloorPhase.BattleRewards;BattleResume=null;return true;
  }
  public void DecideDrop(string id){if(DropDecided)return;DropDecided=true;PendingDropId=id;}
  public void ConsumeDrop(){PendingDropId=null;}
  public void CompleteBattleRewards(){if(Phase==FloorPhase.BattleRewards)Phase=PostBattleShopPending?FloorPhase.PostBattleShop:FloorPhase.Complete;}
  public void CompletePostBattleShop(){if(Phase!=FloorPhase.PostBattleShop)return;PostBattleShopPending=false;Phase=FloorPhase.Complete;}
  public void SyncHealth(int hp,bool consumed){CurrentHP=Math.Max(0,Math.Min(MaxHP,hp));RecordPassiveUse(consumed);}
  public void MoveToNextFloor(int firstRoll,int secondRoll){if(Phase!=FloorPhase.Complete)throw new InvalidOperationException("Unfinished floor");AdvanceFloor(firstRoll,secondRoll);Phase=FloorPhase.Choosing;ChestOpened=false;ChestRewardIds=null;RewardClaimed=EncounterDecided=AdditionalBattle=BattleExperienceClaimed=DropDecided=false;BattleExperiencePercent=0;LastBattleGold=0;PostBattleShopPending=false;PendingDropId=null;BattleResume=null;}
  void GrantExperience(int gain){if(gain>0){PreviousExperienceLevel=Level;PreviousExperience=Experience;ExperiencePresentationPending=true;}int oldLevel=Level,oldStage=DragonData.EvolutionStage(Level);LastExperienceGain=Math.Max(0,gain);Experience+=LastExperienceGain;while(Experience>=ExperienceToNext){Experience-=ExperienceToNext;Level++;}PendingLevelAugments+=Level/5-oldLevel/5;if(DragonData.EvolutionStage(Level)>oldStage){PendingEvolutionStage=DragonData.EvolutionStage(Level);RecalculateMaxHP();}}
 }
}
