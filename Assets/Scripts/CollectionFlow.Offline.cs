using System;using System.Runtime.InteropServices;using UnityEngine;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  string offlineNotice="",offlinePrefix;bool offlineIdentityPending,offlineChoiceCommitted;float offlineDeadline;
  [Serializable] sealed class CachedIdentity {public string owner,error;}
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]static extern void DT_ReadOfflineIdentity(string receiver);
#endif
  void BeginOffline(){
   offlinePrefix=localProfilePrefix;
#if UNITY_EDITOR
   string test=Environment.GetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE");if(!string.IsNullOrEmpty(test))offlinePrefix=localProfilePrefix=test;
#endif
   string chosen=PlayerPrefs.GetString(offlinePrefix+".OfflineStore","");
   if(!string.IsNullOrEmpty(chosen)){if(chosen!=offlinePrefix&&!chosen.StartsWith(offlinePrefix+".Offline.",StringComparison.Ordinal)){OfflineProblem("저장 위치 정보를 확인할 수 없습니다.");return;}OpenOfflineStore(chosen);return;}
   Screen("기기 기록 확인");Label("이 기기의 모험을 불러오는 중…",0,380,410,90,24,Gold);
#if UNITY_WEBGL && !UNITY_EDITOR
   offlineIdentityPending=true;offlineDeadline=Time.realtimeSinceStartup+8;
   try{DT_ReadOfflineIdentity(gameObject.name);}catch{OnOfflineIdentity("{\"error\":\"local-unreadable\"}");}
#else
   OnOfflineIdentity("{}");
#endif
  }
  [UnityEngine.Scripting.Preserve] public void OnOfflineIdentity(string json){
   if(offlineChoiceCommitted)return;
   offlineIdentityPending=false;
   try{
    var cached=JsonUtility.FromJson<CachedIdentity>(json)??new CachedIdentity();
    if(!string.IsNullOrEmpty(cached.error)){offlineNotice="이전 계정의 기기 정보를 확인하지 못했습니다. 원본은 보존됩니다.";OfflineProblem(offlineNotice);return;}
    string imported=string.IsNullOrEmpty(cached.owner)?null:OfflineRecords.CopyAccount(offlinePrefix,cached.owner);
    if(imported!=null&&OfflineRecords.Meaningful(Session.Profile)){
     Screen("기록 선택");Label("기존 로컬 기록과 이전 계정 기록이 모두 있습니다.\n원본을 보존했으며 자동으로 합치지 않았습니다.\n이번 버전에서 사용할 기록을 선택해주세요.",0,280,412,128,21,Color.white);
     var p=new ProfileStore(imported).Load();var t=Label("이전 계정 · "+(p.playerName??"닉네임 없음")+"\n드래곤 "+p.dragons.Count+"마리 · "+(p.hasActiveRun?"진행 중인 도전 있음":"진행 중인 도전 없음"),0,422,405,82,20,Gold);t.supportRichText=false;
     Button("이전 계정 기록으로 플레이",0,555,390,60,()=>OpenOfflineStore(imported));Button("기존 로컬 기록으로 플레이",0,638,390,60,()=>OpenOfflineStore(offlinePrefix));notice.text="어느 쪽을 선택해도 다른 원본 기록은 삭제되지 않습니다.";return;
    }
    OpenOfflineStore(imported??offlinePrefix);
   }catch(Exception){OfflineProblem("이전 기록을 안전하게 읽지 못했습니다.\n원본은 덮어쓰지 않고 보존했습니다.");}
  }
  void OfflineProblem(string message){Screen("저장 확인 필요");Label(message,0,340,412,130,23,Color.white);Button("기존 로컬 기록으로 계속",0,560,390,62,()=>{offlineNotice=message;OpenOfflineStore(offlinePrefix);});notice.text="서버 접속이나 저장 초기화는 진행하지 않았습니다.";}
  void OpenOfflineStore(string prefix){
   try{
    if(prefix!=offlinePrefix&&!OfflineRecords.Exists(prefix))throw new InvalidOperationException("Missing selected save");
    foreach(var suffix in new[]{".A",".B"})if(PlayerPrefs.HasKey(prefix+suffix)&&!PlayerPrefs.HasKey(prefix+".BeforeOffline"+suffix))PlayerPrefs.SetString(prefix+".BeforeOffline"+suffix,PlayerPrefs.GetString(prefix+suffix));
    PlayerPrefs.Save();
    var next=new CollectionSession(new ProfileStore(prefix),catalog);next.PrepareOfflineRecords();
    if(string.IsNullOrEmpty(next.Profile.playerName)&&prefix==offlinePrefix){var cached=new LocalTrainerSave(offlinePrefix).Load("offline");if(cached?.profile!=null)next.SetTrainerName(cached.profile.displayName);}
    if(Session!=null){Session.AchievementUnlocked-=achievementToast.Show;Session.BondPointsEarned-=achievementToast.ShowBond;}Session=next;Session.AchievementUnlocked+=achievementToast.Show;Session.BondPointsEarned+=achievementToast.ShowBond;
    PlayerPrefs.SetString(offlinePrefix+".OfflineStore",prefix);PlayerPrefs.Save();offlineChoiceCommitted=true;identity=null;activeOwner="offline";if(!TryBeginIntroduction()){if(Session.Selected==null&&Session.Profile.eggs>0)ShowEgg();else ShowLobby();}if(!string.IsNullOrEmpty(offlineNotice))notice.text=offlineNotice;
   }catch(Exception){Screen("저장 확인 필요");Label("저장을 읽지 못했습니다.\n기존 기록은 보존되며 자동 초기화하지 않습니다.",0,350,410,140,23,Color.white);}
  }
  void AddLocalNickname(){var b=Button(string.IsNullOrEmpty(Session.Profile.playerName)?"닉네임 설정":"플레이어 · "+Session.Profile.playerName,0,205,300,42,ShowLocalPlayer);b.GetComponentInChildren<UnityEngine.UI.Text>().supportRichText=false;}
  void ShowOfflineStarterLobby(){Screen("드래곤 로비");AddLocalNickname();var art=Rect("첫 드래곤의 알",body,0,389,180,180);art.gameObject.AddComponent<EggGraphic>().raycastTarget=false;Label("첫 드래곤을 만나보세요",0,493,390,38,26,Gold);AddPersonalRecords();Button("첫 드래곤 부화",0,650,400,66,ShowEgg);Button("드래곤 도감",0,719,380,54,ShowCodex);Button("업적 · 도전 기록",0,771,380,40,ShowProgressRecords);notice.text="로그인 없이 시작 · 알을 부화하면 탑에 도전할 수 있습니다.";ApplyCozyLobby();}
  public void ShowLocalPlayer(){CheckpointBattle();controller.EndBattle();towerRun=null;AccountScreen("플레이어 닉네임");var input=AccountInput("이 기기에서 사용할 닉네임",330,Session.Profile.playerName??"");input.characterLimit=12;var feedback=Label("한글·영문·숫자 1~12자",0,425,390,70,22,Color.white);Button("저장",0,575,380,60,()=>{var n=input.text.Trim();if(!TrainerProfile.ValidName(n)){feedback.text="닉네임을 확인해주세요. 공백만 사용할 수 없습니다.";return;}try{Session.SetTrainerName(n);ShowLobby();}catch{feedback.text="저장하지 못했습니다. 기존 이름은 유지됩니다.";}});Button("돌아가기",0,660,380,55,ShowLobby);notice.text="닉네임과 진행 기록은 이 브라우저에 저장됩니다.";}
  string BestPartner(){var r=Session.Profile.bestScoreRun;if(r==null||string.IsNullOrEmpty(r.runId)||r.score!=Session.Profile.bestScore)return Session.Profile.bestScore>0?"이전 기록 · 파트너 정보 없음":"아직 완료한 도전이 없습니다";return string.IsNullOrEmpty(r.customDragonName)?r.originalDragonName:r.customDragonName+" ("+r.originalDragonName+")";}
  void AddPersonalRecords(){Panel("나의 탑 기록",body,0,560,400,98,AncientUi.Stone,AncientUi.Gold);Label("나의 탑 기록",0,527,380,24,18,Gold);var p=Session.Profile;Label(p.bestScore.ToString("N0")+"점   ·   최고 "+p.bestFloor+"층   ·   유대 "+p.bestBond,0,551,382,29,19,Color.white);var t=Label(BestPartner(),0,583,382,25,16,Muted);t.supportRichText=false;}
 }
}
