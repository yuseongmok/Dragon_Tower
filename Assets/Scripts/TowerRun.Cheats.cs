using UnityEngine;
namespace DragonTower {
 public sealed partial class TowerRun {
  public void CheatEnterBoss(int floor,int seed){
   Floor=Mathf.Clamp(((floor+9)/10)*10,10,100);Active=true;Room=TowerRoomKind.Boss;RoomChosen=true;
   Phase=FloorPhase.CombatPending;choices=new[]{TowerRoomKind.Boss};BattleResume=null;
   RewardClaimed=EncounterDecided=AdditionalBattle=BattleExperienceClaimed=DropDecided=false;
   ChestOpened=false;ChestRewardIds=null;PendingDropId=null;PostBattleShopPending=false;
   LastBattleGold=0;BattleExperiencePercent=0;RewardSeed=seed;
   PendingLevelAugments=0;PendingEvolutionStage=0;ExperiencePresentationPending=false;
   if(CurrentHP<=0)CurrentHP=MaxHP;
  }
  public bool CheatSignatureNoCooldown {get;private set;}
  public void CheatEquipSignature(DragonData dragon,bool noCooldown=false){if(dragon?.signatureSkill!=null){replacedSkill=dragon.signatureSkill;CheatSignatureNoCooldown=noCooldown;}}
 }
 public static class DesktopCheatRules {
  public static int BossFloor(int floor)=>Mathf.Clamp(((floor+9)/10)*10,10,100);
  public static void ApplySignature(BattleModel model,TowerRun run,DragonData dragon){
   var stats=run.BuildBattleStats(dragon.Snapshot(run.Level),model.Dragon.legacyBalance,model.Dragon.combatRulesVersion,model.Dragon.passiveImprintLevel);
   model.Dragon.skill=stats.skill;model.Dragon.skillEffect=stats.skillEffect;model.Dragon.baseSkillCooldown=stats.baseSkillCooldown;model.SkillReady=model.Time;
  }
 }
}
