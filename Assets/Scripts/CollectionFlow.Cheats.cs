using UnityEngine;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  public void CheatBoss(int floor){
   if(Session?.Selected==null)return;
   StopTutorial();StopAllCoroutines();roomTransitionBusy=false;regionEntranceOwner=null;returningToLobby=false;
   controller.EndBattle();
   if(towerRun==null){var d=Session.Selected;towerRun=new TowerRun(d.maxHP,0,60,d.elementType,d.alternateSkillElement,d.StableId,d);towerRun.AttachInstance(Session.Profile.selectedInstanceId);}
   towerRun.CheatEnterBoss(floor,Random.Range(0,int.MaxValue));SaveProgress();StartCurrentBattleCore();
  }
 }
}
