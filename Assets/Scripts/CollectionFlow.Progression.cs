using System;
using System.Linq;
using UnityEngine;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  FloorProgressionConfig progression;
  float nextRunSave;
  bool returningToLobby;
  void SaveProgress(){if(towerRun!=null&&towerRun.Active)Session.SaveRun(towerRun);}
  public void CheckpointBattle(){if(towerRun==null||!towerRun.Active)return;var b=controller.CurrentBattle;if(b!=null){towerRun.ItemCooldowns=b.CaptureItemCooldowns();if(b.Result!=BattleResult.Fighting){BattleSettled();return;}towerRun.SyncHealth(b.PlayerHP,b.PassiveConsumed);}SaveProgress();}
  public void BattleSettled(){
   if(towerRun==null||!towerRun.Active||controller.CurrentBattle==null)return;var b=controller.CurrentBattle;
   towerRun.ItemCooldowns=b.CaptureItemCooldowns();towerRun.RecordPassiveUse(b.PassiveConsumed);
   if(b.Result==BattleResult.Defeat){towerRun.End();Session.FinishRun(towerRun);return;}
   if(b.Result==BattleResult.Victory&&towerRun.CompleteFloorCombat(b.PlayerHP,progression,UnityEngine.Random.Range(0,100))){DecideSavedDrop();SaveProgress();}
  }
  void DecideSavedDrop(){
   if(towerRun.DropDecided)return;int chance=towerRun.Room==TowerRoomKind.Boss?25:12;string id=null;
   if(database.items!=null&&database.items.Length>0&&UnityEngine.Random.Range(0,100)<chance)id=database.PickItems(1,UnityEngine.Random.Range(0,int.MaxValue))[0].StableId;
   towerRun.DecideDrop(id);
  }
  public void ResumeRun(){
   if(!Session.HasSavedRun)return;
   try{towerRun=TowerRun.Restore(Session.Profile.activeRun,database);if(!Session.Select(towerRun.InstanceId))throw new InvalidOperationException("도전 드래곤을 찾을 수 없습니다.");ContinueRunPhase();}
   catch(Exception e){Screen("이어하기 확인 필요");Label(e.Message,0,380,420,120,20,Color.white);Button("로비로",0,610,360,60,ShowLobby);}
  }
  void ContinueRunPhase(){
   if(towerRun==null)return;
   if(towerRun.PendingInventory.Count>0&&towerRun.Phase!=FloorPhase.CombatActive){
    var item=towerRun.PendingInventory[0].Item;
    Action next=()=>{towerRun.ResolvePendingInventory();SaveProgress();ContinueRunPhase();};
    AcquireItem(item,next,next);return;
   }
   if(towerRun.ExperiencePresentationPending){ShowExperienceReward(towerRun.PreviousExperienceLevel,towerRun.PreviousExperience);return;}
   if(towerRun.Phase==FloorPhase.BattleRewards){if(TryShowMonsterDrop())return;}
   if(towerRun.PendingEvolutionStage>0){ShowEvolutionCutscene();return;}
   if(towerRun.PendingLevelAugments>0){ShowAugmentChoices(true);return;}
   if(towerRun.Phase==FloorPhase.BattleRewards){towerRun.CompleteBattleRewards();SaveProgress();}
   switch(towerRun.Phase){
    case FloorPhase.Choosing:ShowTowerChoices();break;
    case FloorPhase.RoomEvent:ShowChosenRoom();break;
    case FloorPhase.CombatPending:case FloorPhase.CombatActive:StartCurrentBattle();break;
    case FloorPhase.PostBattleShop:ShowShopRoom();break;
    case FloorPhase.Complete:AdvanceFloor();break;
   }
  }
  void FinishNestReward(DragonData dragon){towerRun.CompleteRoomEvent(progression,UnityEngine.Random.Range(0,100),UnityEngine.Random.Range(0,100));Session.SaveNestReward(towerRun,dragon);}
  void FinishRoomEvent(){if(towerRun.CompleteRoomEvent(progression,UnityEngine.Random.Range(0,100),UnityEngine.Random.Range(0,100)))SaveProgress();}
  void FinishLevelReward(string text){SaveProgress();Screen("성장 완료");Label(text,0,350,420,130,24,Gold);RoomButton=Button("계속",0,610,360,60,ContinueRunPhase);}
  void AutoContinueReward(){StartCoroutine(ContinueRewardAfterDelay());}
  System.Collections.IEnumerator ContinueRewardAfterDelay(){var owner=body;yield return new WaitForSeconds(.7f);if(!returningToLobby&&body==owner)ContinueRunPhase();}
  public void AutoSaveRun(){if(towerRun==null||!towerRun.Active||returningToLobby)return;if(Time.unscaledTime>=nextRunSave){nextRunSave=Time.unscaledTime+2;CheckpointBattle();}}
  void OnApplicationPause(bool paused){if(paused&&!returningToLobby)CheckpointBattle();}
  void OnApplicationQuit(){CheckpointBattle();}
 }
}
