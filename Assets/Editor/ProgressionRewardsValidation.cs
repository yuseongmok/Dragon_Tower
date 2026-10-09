using System;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.Build.Reporting;
namespace DragonTower.Editor {
 public static class ProgressionRewardsValidation {
  static int checks;static void Check(bool value,string label){if(!value)throw new Exception("PROGRESSION_FAIL "+label);checks++;}
  public static void Run(){try{Verify();FloorProgressionValidation.Verify();Debug.Log("PROGRESSION_OK checks="+checks);EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  public static void Verify(){
   var db=ContentDatabase.Load();var c=FloorProgressionConfig.Load();const string key="DragonTower.Test.Progression20261009";PlayerPrefs.DeleteKey(key+".A");PlayerPrefs.DeleteKey(key+".B");
   var session=new CollectionSession(new ProfileStore(key),db.dragons);var d=session.Hatch(0);session.RenameDragon(session.Profile.selectedInstanceId,"테스트 친구",out var nameError);
   var r=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);r.AttachInstance(session.Profile.selectedInstanceId);int notices=0;session.AchievementUnlocked+=a=>notices++;session.SaveRun(r);Check(r.Progress.score==50,"first floor score");session.SaveRun(r);Check(r.Progress.score==50,"no duplicate floor");
   for(int floor=1;floor<=55;floor++){
    r.BeginFloorRoom(0,88);r.BeginFloorCombat(c,40);Check(r.CompleteFloorCombat(r.MaxHP,c),"victory "+floor);Check(!r.CompleteFloorCombat(r.MaxHP,c),"repeat victory "+floor);session.SaveRun(r);
    int score=r.Progress.score;session.SaveRun(r);Check(r.Progress.score==score,"repeat save "+floor);
    if(floor==1){Check(score==160&&notices==1,"first kill plus achievement");Check(r.Level==2&&r.Experience==0,"unchanged 100 EXP growth");}
    while(r.PendingLevelAugments>0)r.ConsumeEmptyAugmentReward(true);if(r.PendingEvolutionStage>0)r.ConsumeEvolution();r.CompleteBattleRewards();
    if(floor==12){var reloaded=new CollectionSession(new ProfileStore(key),db.dragons);r=TowerRun.Restore(reloaded.Profile.activeRun,db);r.CompleteBattleRewards();reloaded.SaveRun(r);Check(r.Progress.score==score,"disk load no duplicate");Check(r.Progress.customDragonName=="테스트 친구","custom name persists");}
    if(floor<55){r.MoveToNextFloor(0,1);session.SaveRun(r);}
   }
   Check(r.BossesDefeated==5&&r.Progress.normalKills==50,"boss normal split");Check(r.Progress.score==55*50+50*10+5*200+1300,"exact score all sources");Check(session.Profile.achievementProgress.unlocked.Count==5,"all five achievements");
   var before=r.Progress.score;r.End();session.FinishRun(r);session.FinishRun(r);var loaded=new CollectionSession(new ProfileStore(key),db.dragons);Check(!loaded.HasSavedRun&&loaded.Profile.lastRun.progress.score==before&&loaded.Profile.lastRun.progress.finalized,"death finalization persists");Check(loaded.Profile.lastRun.progress.maxBond==56&&loaded.Profile.lastRun.progress.dragonId==d.StableId,"ranking fields");
   var second=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);second.AttachInstance(session.Profile.selectedInstanceId);loaded.SaveRun(second);second.BeginFloorRoom(0,1);second.BeginFloorCombat(c,40);second.CompleteFloorCombat(second.MaxHP,c);loaded.SaveRun(second);Check(second.Progress.score==60&&second.Progress.achievements.Count==0,"career awards only once across runs");
   var old=second.Capture();old.progress=null;old.BossesDefeated=0;var migrated=TowerRun.Restore(old,db);loaded.SaveRun(migrated);Check(migrated.Progress.score==0&&migrated.Progress.normalKills==1,"legacy baseline no retroactive kills");
   var profile=JsonUtility.FromJson<PlayerProfile>("{\"version\":1,\"starterGiftClaimed\":true,\"eggs\":1,\"dragons\":[]}");var oldStore=new ProfileStore(key+"Old");oldStore.Save(profile);Check(new CollectionSession(new ProfileStore(key+"Old"),db.dragons).Profile.eggs==1,"legacy profile loads");
   PlayerPrefs.DeleteKey(key+".A");PlayerPrefs.DeleteKey(key+".B");PlayerPrefs.DeleteKey(key+"Old.A");PlayerPrefs.DeleteKey(key+"Old.B");PlayerPrefs.Save();
  }
 }
}

