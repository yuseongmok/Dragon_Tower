using System;using System.Linq;using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 [Serializable] public sealed class LeaderboardEntry {
  public string uid,runId,playerName,dragonId,dragonName,originalDragonName,verification;
  public int bestScore,bestFloor,bestBond;public long achievedAtMs;
 }
 public sealed partial class CollectionFlow {
  bool leaderboardSubmitting;string leaderboardSubmissionError="";float leaderboardNextRefresh;
  void SubmitPendingLeaderboard(Action completed=null){
   if(!OnlineFeatures.Enabled){completed?.Invoke();return;}
   EnsureAccounts();
   if(leaderboardSubmitting||account.Busy){completed?.Invoke();return;}
   if(identity==null||string.IsNullOrEmpty(identity.uid)||activeOwner!=AccountOwner(identity)){completed?.Invoke();return;}
   var record=Session.Profile.pendingLeaderboardRecords?.FirstOrDefault(p=>p.finalized);
   if(record==null){completed?.Invoke();return;}
   var owner=activeOwner;var session=Session;leaderboardSubmitting=true;
   account.Request(new AccountRequest{op="leaderboard-submit",uid=identity.uid,record=record},r=>{
    leaderboardSubmitting=false;if(owner!=activeOwner||session!=Session){completed?.Invoke();return;}
    if(r.ok&&r.accepted){try{session.AcknowledgeLeaderboard(record.runId);leaderboardSubmissionError="";}catch{leaderboardSubmissionError="전송 확인을 저장하지 못했습니다. 같은 기록으로 재시도합니다.";}}
    else leaderboardSubmissionError=LeaderboardError(r.code);
    completed?.Invoke();
   });
  }
  string LeaderboardError(string code){switch(code){
   case "leaderboard-not-configured":return "랭킹 서버 연결을 준비 중입니다.\n도전 기록은 기기에 보관됩니다.";
   case "editor-local":return "에디터에서는 온라인 조회가 연결되지 않습니다.";
   case "functions/resource-exhausted":case "busy":return "잠시 후 새로고침해주세요.";
   case "functions/invalid-argument":case "functions/already-exists":return "기록 검증을 통과하지 못했습니다.\n기기 기록은 그대로 보관됩니다.";
   default:return "랭킹에 연결하지 못했습니다.\n인터넷 연결을 확인하고 다시 시도해주세요.";
  }}
  public void ShowLeaderboard(){
   if(!OnlineFeatures.Enabled){ShowProgressRecords();return;}
   EnsureAccounts();AccountScreen("유대의 탑 랭킹");
   Label("플레이테스트 · 클라이언트 기록 기반",0,152,416,28,17,Muted);
   var status=Label("최신 기록을 불러오는 중…",0,208,414,65,20,Color.white);
   var mine=Label("내 최고 기록을 확인하고 있습니다",0,660,410,115,19,Color.white);mine.supportRichText=false;
   Button("새로고침",-103,758,192,54,()=>{if(Time.realtimeSinceStartup<leaderboardNextRefresh){status.text="잠시 후 다시 눌러주세요.";return;}ShowLeaderboard();});
   Button("로비로",103,758,192,54,ShowLobby);notice.text="점수 → 층 → 유대 → 서버 등록 시각 순으로 정렬합니다.";
   if(identity==null||string.IsNullOrEmpty(identity.uid)||activeOwner!=AccountOwner(identity)){status.text="로그인한 계정으로 랭킹을 이용할 수 있습니다.";mine.text="내 기록 · 로그인 필요";return;}
   var screen=body;var uid=identity.uid;leaderboardNextRefresh=Time.realtimeSinceStartup+5;
   Action read=()=>{
    if(body!=screen||identity?.uid!=uid)return;
    if(account.Busy){status.text="다른 요청을 처리 중입니다. 잠시 후 새로고침해주세요.";mine.text="내 기록 · 조회 대기";return;}
    account.Request(new AccountRequest{op="leaderboard-read",uid=uid},r=>{
     if(body!=screen||identity?.uid!=uid)return;
     if(!r.ok){status.text=LeaderboardError(r.code);mine.text="내 기록 · 조회하지 못했습니다";return;}
     var entries=r.entries??Array.Empty<LeaderboardEntry>();
     status.text=entries.Length==0?"아직 등록된 기록이 없습니다.\n도전을 마치고 첫 기록을 남겨보세요.":"상위 "+entries.Length+"명 · 아래로 스크롤";
     if(entries.Length>0)LeaderboardList(entries);
     var m=r.mine;mine.text=m==null||string.IsNullOrEmpty(m.uid)?"내 최고 기록 · 아직 등록된 기록이 없습니다":("내 순위  "+(r.myRank>0?r.myRank+"위":"집계 대기")+"  ·  "+m.playerName+"\n"+LeaderboardDragonName(m)+"\n"+m.bestScore.ToString("N0")+"점   ·   "+m.bestFloor+"층   ·   유대 "+m.bestBond);
     int pending=Session.Profile.pendingLeaderboardRecords?.Count??0;if(pending>0)notice.text="미전송 "+pending+"건 · "+(string.IsNullOrEmpty(leaderboardSubmissionError)?"새로고침하면 재시도합니다":leaderboardSubmissionError.Replace("\n"," "));
    });
   };
   SubmitPendingLeaderboard(read);
  }
  string LeaderboardDragonName(LeaderboardEntry e)=>string.IsNullOrEmpty(e.dragonName)?e.originalDragonName:e.dragonName+" ("+e.originalDragonName+")";
  void LeaderboardList(LeaderboardEntry[] entries){
   var viewport=Rect("Ranking viewport",body,0,420,416,350);var background=viewport.gameObject.AddComponent<Image>();background.color=AncientUi.Background;viewport.gameObject.AddComponent<RectMask2D>();
   var content=Rect("Ranking rows",viewport,0,0,404,entries.Length*91);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
   var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=36;
   for(int i=0;i<entries.Length;i++){
    var e=entries[i];var row=Panel("Rank "+(i+1),content,0,i*91+43,400,84,AncientUi.Stone,AncientUi.DarkGold);
    var medal=i==0?new Color(1,.78f,.35f):i==1?new Color(.8f,.85f,.91f):i==2?new Color(.76f,.47f,.29f):AncientUi.Muted;
    var rank=CardText(row.transform,"Rank",(i+1).ToString(),-173,42,40,48,23,medal,TextAnchor.MiddleCenter);rank.supportRichText=false;
    var d=Session.Find(e.dragonId);if(d!=null){var portrait=Rect("Dragon portrait",row.transform,-120,42,56,60);var image=portrait.gameObject.AddComponent<Image>();image.sprite=d.SpriteForStage(0);image.preserveAspect=true;image.raycastTarget=false;}
    var name=CardText(row.transform,"Nickname",e.playerName,-2,20,178,26,20,Color.white,TextAnchor.MiddleLeft);name.supportRichText=false;
    var score=CardText(row.transform,"Score",e.bestScore.ToString("N0")+"점",143,20,101,26,19,Gold,TextAnchor.MiddleRight);score.supportRichText=false;
    var dragon=CardText(row.transform,"Partner",LeaderboardDragonName(e),46,45,294,24,16,Muted,TextAnchor.MiddleLeft);dragon.supportRichText=false;
    CardText(row.transform,"Progress",e.bestFloor+"층   ·   유대 "+e.bestBond,46,66,294,22,16,Color.white,TextAnchor.MiddleLeft);
   }
  }
 }
}
