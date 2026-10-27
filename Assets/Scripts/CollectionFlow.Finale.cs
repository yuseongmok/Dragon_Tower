using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionSession {
  // Presentation checkpoints are independent of combat replay and reward receipts.
  public void SaveFinalScene(string runId,int kind,int page,bool done=false){
   var n=ProfileStore.Clone(Profile);
   if(kind<2&&n.finalSceneRunId!=runId){n.finalSceneRunId=runId;n.finalEntryPage=n.finalVictoryPage=n.endingPage=0;}
   if(kind==0){n.finalEntryPage=page;if(done)n.finalEntryRunId=runId;}
   else if(kind==1){n.finalVictoryPage=page;if(done)n.finalVictoryRunId=runId;}
   else {n.endingPage=page;if(done){n.endingPending=false;if(!string.IsNullOrEmpty(runId))n.finalEndingRunId=runId;}}
   store.Save(n);Profile=n;
  }
 }
 public sealed partial class CollectionFlow {
  StoryTowerCinema finalCinema;float finalAge;int finalKind,finalPage;bool finalActive,finalReplay;
  static readonly string[][] FinalLines={
   new[]{"낡은 망토 곁에서,\n최초의 계약자가 아직 제단을 지키고 있었다.\n\n“이곳은…… 우리가 맺은 약속의 자리다.”","“다가오지 마라.\n이번에는…… 빼앗기지 않겠다.”"},
   new[]{"공허가 걷히자, 드래곤은 고개를 낮췄다.\n\n“너희는…… 함께 싸우고 있었구나.”","“지켜야 했던 것은…… 이 자리가 아니라,\n우리가 함께할 수 있다는 믿음이었구나.”"},
   new[]{"낡은 망토가 바람에 움직였다.\n오랜 기다림 끝에, 제단에 빛이 돌아왔다.","한 사람의 시간은 끝났지만,\n그 약속은 새로운 유대로 이어졌다."}
  };
  bool TryFinalEntry(){
   if(towerRun==null||towerRun.Floor!=100||towerRun.Room!=TowerRoomKind.Boss||towerRun.BattleResume!=null||towerRun.Phase==FloorPhase.CombatActive||Session.Profile.finalEntryRunId==towerRun.Progress.runId)return false;
   SaveProgress();ShowFinalStory(0,Session.Profile.finalSceneRunId==towerRun.Progress.runId?Session.Profile.finalEntryPage:0);return true;
  }
  bool TryFinalVictory(){
   if(towerRun==null||towerRun.Floor!=100||towerRun.Room!=TowerRoomKind.Boss||towerRun.Phase!=FloorPhase.BattleRewards )return false;
   string id=towerRun.Progress.runId;
   if(Session.Profile.finalVictoryRunId!=id){ShowFinalStory(1,Session.Profile.finalSceneRunId==id?Session.Profile.finalVictoryPage:0);return true;}
   if(Session.Profile.finalEndingRunId!=id){ShowFinalStory(2,Session.Profile.endingPage);return true;}
   return false;
  }
  public void ShowFinalStory(int kind,int page,bool replay=false){
   kind=Mathf.Clamp(kind,0,2);page=Mathf.Clamp(page,0,1);
   if(!replay)Session.SaveFinalScene(towerRun?.Progress.runId,kind,page);
   Screen(kind==0?"남겨진 계약자":kind==1?"알아본 유대":"이어지는 약속");
   finalKind=kind;finalPage=page;finalReplay=replay;finalAge=0;finalActive=true;
   foreach(Transform child in body){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   body.GetComponent<Image>().color=Color.black;
   finalCinema=body.gameObject.AddComponent<StoryTowerCinema>();
   string art=kind==0?"promise":kind==1?"final-liberation":"final-dawn";
   finalCinema.Build(body,art,kind==0?3:8,kind==0?3:page==1?3:-1);
   for(int i=0;i<16;i++){var shade=Rect("Final subtitle shade",body,0,555+i*19,480,20).gameObject.AddComponent<Image>();shade.color=new Color(.015f,.02f,.035f,.04f+i*.043f);shade.raycastTarget=false;}
   var caption=Label(FinalLines[kind][page],0,694,414,160,22,Color.white);caption.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1,-2);
   notice=Label("화면을 터치하여 계속",0,803,350,28,15,new Color(.8f,.83f,.87f));
   var tap=Rect("Final tap",body,0,425,480,850).gameObject.AddComponent<Image>();tap.color=Color.clear;
   var b=tap.gameObject.AddComponent<Button>();b.targetGraphic=tap;b.transition=Selectable.Transition.None;b.onClick.AddListener(()=>AdvanceFinalStory(false));
   var corner=Rect("Final skip",body,174,42,112,48).gameObject.AddComponent<Image>();corner.color=new Color(0,0,0,.16f);
   var skip=corner.gameObject.AddComponent<Button>();skip.targetGraphic=corner;skip.transition=Selectable.Transition.None;skip.onClick.AddListener(()=>AdvanceFinalStory(true));
   CardText(corner.transform,"Skip text","건너뛰기 ›",0,24,108,40,15,Color.white,TextAnchor.MiddleCenter);
  }
  void TickFinalStory(){if(finalActive&&finalCinema!=null){finalAge+=Time.unscaledDeltaTime;finalCinema.Render(finalAge);}}
  void ClearFinalStory(){finalActive=false;if(finalCinema!=null){Destroy(finalCinema);finalCinema=null;}}
  public void AdvanceFinalStory(bool skip){
   if(!finalActive||finalAge<.25f)return;
   if(!skip&&finalPage==0&&finalAge<finalCinema.Duration){finalAge=finalCinema.Duration;finalCinema.Render(finalAge);return;}
   int kind=finalKind,page=finalPage;bool replay=finalReplay;
   if(!skip&&page==0){ShowFinalStory(kind,1,replay);return;}
   if(!replay)Session.SaveFinalScene(towerRun?.Progress.runId,kind,page,true);
   ClearFinalStory();
   if(replay){ShowStoryArchive();return;}
   if(kind==0){StartCurrentBattleCore();return;}
   if(kind==1||(kind==2&&towerRun!=null&&towerRun.Active&&!towerRun.Progress.finalized)){ContinueRunPhase();return;}
   ShowFinalCompletion();
  }
  void ShowFinalCompletion(){
   Screen("타워 완주");Label("100층을 돌파했습니다!\n최종 점수 "+(Session.Profile.lastRun?.progress?.score??0)+"점",0,350,420,80,28,Gold);Button("로비로",0,610,360,60,ShowLobby);
  }
 }
}
