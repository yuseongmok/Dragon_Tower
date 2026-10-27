using UnityEngine;
namespace DragonTower {
 public partial class BattleController {
  #if UNITY_WEBGL && !UNITY_EDITOR
  [System.Runtime.InteropServices.DllImport("__Internal")]static extern int DT_DesktopCheatKeys();
  static int desktopBrowser=-1;
#endif
  public static bool DesktopCheatsAvailable {
   get {
    if(Application.isMobilePlatform||SystemInfo.deviceType==DeviceType.Handheld)return false;
#if UNITY_WEBGL && !UNITY_EDITOR
    if(desktopBrowser<0)desktopBrowser=DT_DesktopCheatKeys();
    return desktopBrowser==1;
#else
    return true;
#endif
   }
  }
  bool cheatGod;string cheatNotice="";float cheatNoticeUntil;
  void TickDesktopCheats(){
   if(!focused||!DesktopCheatsAvailable||Flow?.Session?.Selected==null)return;
   if(Input.GetKeyDown(KeyCode.F1))ExecuteDesktopCheat(1);
   else if(Input.GetKeyDown(KeyCode.F2))ExecuteDesktopCheat(2);
   else if(Input.GetKeyDown(KeyCode.F3))ExecuteDesktopCheat(3);
   else if(Input.GetKeyDown(KeyCode.F4))ExecuteDesktopCheat(4);
  }
  public void ExecuteDesktopCheat(int key){
   if(!DesktopCheatsAvailable||Flow?.Session?.Selected==null)return;
   if(key==1||key==4){int floor=key==4?100:DesktopCheatRules.BossFloor(Flow.CurrentRun?.Floor??1);cheatGod=battle?.CheatInvincible??cheatGod;Flow.CheatBoss(floor);if(cheatGod&&!battle.CheatInvincible){battle.CheatInvincible=true;RecordBattleInput(7);}NotifyCheat(floor+"층 보스전");}
   else if(key==2){if(battle!=null&&battle.Result!=BattleResult.Fighting)return;cheatGod=!(battle?.CheatInvincible??cheatGod);if(battle!=null){battle.CheatInvincible=cheatGod;RecordBattleInput(7);}NotifyCheat("무적 "+(cheatGod?"ON":"OFF"));}
   else if(key==3){var d=Flow.Session.Selected;if(Flow.CurrentRun==null||d.signatureSkill==null)return;if(battle!=null&&(battle.Result!=BattleResult.Fighting||!battle.CheatCanChangeSkill)){NotifyCheat("현재 스킬이 끝난 후 다시 눌러주세요");return;}Flow.CurrentRun.CheatEquipSignature(d,true);if(battle!=null){CancelSignature();DesktopCheatRules.ApplySignature(battle,Flow.CurrentRun,d);RecordBattleInput(9);view.SetSkillPresentation(d.signatureSkill);view.Show(battle);}NotifyCheat(d.signatureSkill.displayName+" 장착 · 쿨타임 없음");}
   Flow.CheckpointBattle();
  }
  void NotifyCheat(string text){cheatNotice="치트 · "+text;cheatNoticeUntil=Time.unscaledTime+2.5f;}
  void OnGUI(){if(!DesktopCheatsAvailable)return;if(Time.unscaledTime<cheatNoticeUntil)GUI.Box(new Rect(10,10,280,28),cheatNotice);else if(battle?.CheatInvincible==true)GUI.Box(new Rect(10,10,100,25),"무적 ON · F2");}
 }
}
