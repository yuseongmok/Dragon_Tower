using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionSession {
  public void SaveIntroduction(int page,int step,bool completed){var n=ProfileStore.Clone(Profile);n.onboardingVersion=1;n.introPage=Mathf.Clamp(page,0,8);n.tutorialStep=Mathf.Clamp(step,0,2);n.tutorialCompleted=completed;store.Save(n);Profile=n;}
 }
 public sealed partial class CollectionFlow {
  bool storyReplay,tutorialActive,dodgeSent;int lesson;float lessonEnd=-1;GameObject tutorialUI;Text tutorialText;RectTransform storyArt;float storyAge;StoryTowerCinema storyCinema;
  public bool TutorialActive=>tutorialActive;public int TutorialLesson=>lesson;
  static readonly string[] StoryTitles={"끝나지 않던 전쟁","최초의 유대","새로운 만남","","평화의 탑","최초의 계약","탑의 오염","남겨진 두 계약자","끝나지 않은 약속"};
  static readonly string[] StoryLines={"먼 옛날, 인간과 드래곤은 긴 전쟁을 벌였다.\n서로를 두려워하며, 끝없이 상처를 남겼다.","서로를 살려준 인간과 드래곤.\n두 존재의 유대는 전쟁을 멈췄다.\n서로를 믿을 수 있다는 것을 모두에게 보여주었다.","오랜 시간이 흐른 뒤,\n사람들이 사라진 폐허 속에서\n당신은 아직 식지 않은 알을 발견한다.","","인간은 돌을 쌓고, 드래곤은 하늘길을 열었다.\n서로 다른 삶이 층층이 이어져\n평화를 기리는 하나의 탑이 되었다.","탑의 가장 높은 제단에서, 두 존재는 약속했다.\n서로를 지배하지 않고, 함께 살아가겠다고.\n그 유대가 탑의 심장에 새겨졌다.","그러나 어느 날, 탑의 빛에 균열이 생겼다.\n공허가 스며들고, 금빛 문양이 꺼져 갔다.\n그 시작을 아는 이는 없었다.","사람들이 떠난 뒤에도,\n최초의 계약자들은 제단을 떠나지 않았다.\n함께 맺은 약속을 지키기 위해.","인간의 시간은 끝났지만,\n드래곤은 그 자리에 남았다.\n그는 떠났지만, 약속은 남았다."};
  public bool TryBeginIntroduction()=>false;
  public void ShowStoryArchive(){CheckpointBattle();controller.EndBattle();towerRun=null;Screen("이야기 · 조작 연습");Label("평화의 탑에 남은 첫 번째 기억",0,290,400,80,25,Gold);Button("오프닝 다시 보기",0,450,380,60,()=>{storyReplay=true;ShowStoryPage(0);});Button("전투 조작 연습",0,530,380,60,()=>BeginTutorial(true));Button("로비로",0,710,380,55,ShowLobby);notice.text="다시 보기는 기존 도전과 보상을 변경하지 않습니다.";}
  public void ShowStoryPage(int page){
   page=Mathf.Clamp(page,0,8);if(page==3)page=2;if(!storyReplay)Session.SaveIntroduction(page,0,false);
   if(storyCinema!=null){Destroy(storyCinema);storyCinema=null;}Screen(StoryTitles[page]);storyAge=0;
   // Replace the menu chrome with an edge-to-edge story stage.
   foreach(Transform child in body){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   body.GetComponent<Image>().color=Color.black;
   storyCinema=null;
   if(page>=4){storyCinema=body.gameObject.AddComponent<StoryTowerCinema>();storyCinema.Build(body,page);storyArt=body;}
   if(page==2){var eggArt=Resources.Load<Texture2D>("Story/ruined-tower-egg");if(eggArt!=null){var full=Rect("Ruined peace tower",body,0,425,480,850).gameObject.AddComponent<Image>();full.sprite=Sprite.Create(eggArt,new Rect(0,0,eggArt.width,eggArt.height),new Vector2(.5f,.5f),100);full.raycastTarget=false;storyArt=full.rectTransform;}}
   var tex=page<2?Resources.Load<Texture2D>("Story/opening"):null;
   if(tex!=null){tex.filterMode=FilterMode.Point;float w=tex.width/3f;var im=Rect("Fullscreen story",body,0,425,866,866).gameObject.AddComponent<Image>();im.sprite=Sprite.Create(tex,new Rect(page*w+8,0,w-16,tex.height),new Vector2(.5f,.5f),100);im.preserveAspect=false;im.raycastTarget=false;storyArt=im.rectTransform;}
   // A stepped transparent fade supports subtitles without a card frame.
   for(int i=0;i<16;i++){var shade=Rect("Subtitle shade",body,0,555+i*19,480,20).gameObject.AddComponent<Image>();shade.color=new Color(.015f,.02f,.035f,.04f+i*.043f);shade.raycastTarget=false;}
   var caption=Label(StoryLines[page],0,694,414,150,22,Color.white);caption.gameObject.AddComponent<Shadow>().effectDistance=new Vector2(1,-2);
   var hint=Label("화면을 터치하여 계속",0,803,350,28,15,new Color(.8f,.83f,.87f));
   var tap=Rect("Story tap anywhere",body,0,425,480,850).gameObject.AddComponent<Image>();tap.color=Color.clear;tap.raycastTarget=true;var next=tap.gameObject.AddComponent<Button>();next.targetGraphic=tap;next.transition=Selectable.Transition.None;next.onClick.AddListener(()=>{if(storyAge<.25f)return;if(storyCinema!=null&&storyAge<storyCinema.Duration){storyAge=storyCinema.Duration;storyCinema.Render(storyAge);return;}if(page==0)ShowStoryPage(1);else if(page==1)ShowStoryPage(4);else if(page==4)ShowStoryPage(5);else if(page==5)ShowStoryPage(6);else if(page==6)ShowStoryPage(7);else if(page==7)ShowStoryPage(8);else if(page==8)ShowStoryPage(2);else FinishOpening();});
   var corner=Rect("Story skip",body,174,42,112,48).gameObject.AddComponent<Image>();corner.color=new Color(0,0,0,.16f);corner.raycastTarget=true;var skip=corner.gameObject.AddComponent<Button>();skip.targetGraphic=corner;skip.transition=Selectable.Transition.None;skip.onClick.AddListener(()=>{if(storyAge>=.25f)FinishOpening();});CardText(corner.transform,"Skip text","건너뛰기 ›",0,24,108,40,15,Color.white,TextAnchor.MiddleCenter);
   notice=hint;
  }

  void FinishOpening(){if(storyCinema!=null){Destroy(storyCinema);storyCinema=null;}storyArt=null;if(storyReplay){ShowStoryArchive();return;}Session.SaveIntroduction(3,0,false);if(Session.Selected==null)ShowEgg();else BeginTutorial(false);}
  public void BeginTutorial(bool replay){
   if(Session.Selected==null){ShowEgg();return;}
   CheckpointBattle();controller.EndBattle();towerRun=null;storyReplay=replay;lesson=replay?0:Session.Profile.tutorialStep;lessonEnd=-1;dodgeSent=false;tutorialActive=true;
   root.gameObject.SetActive(false);ScreenName="첫 유대 · 조작 연습";
   var d=Session.Selected;var enemy=BattleEnemyStats.Normal();enemy.displayName="깨어난 작은 수호자 · 연습";enemy.maxHP=100000;enemy.damage=0;
   controller.BeginBattle(d,d.Snapshot(),enemy,1,false,d.maxHP);
   tutorialUI=new GameObject("First bond tutorial",typeof(RectTransform));var r=tutorialUI.GetComponent<RectTransform>();r.SetParent(battleView.frame,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
   Panel("Training instruction",r,0,162,416,68,new Color(.035f,.06f,.09f,.92f),Gold);
   tutorialText=CardText(r,"Training text","",0,162,394,60,18,Color.white,TextAnchor.MiddleCenter);
   var skip=Rect("Skip training",r,158,620,124,34).gameObject.AddComponent<Image>();skip.color=new Color(.06f,.08f,.13f,.9f);skip.raycastTarget=true;var b=skip.gameObject.AddComponent<Button>();b.targetGraphic=skip;b.onClick.AddListener(()=>FinishTutorial());CardText(skip.transform,"Skip label","연습 건너뛰기",0,17,120,30,14,Color.white,TextAnchor.MiddleCenter);
   UpdateTutorialText();
  }
  void UpdateTutorialText(){if(tutorialText==null)return;tutorialText.text=lesson==0?"탑이 우리의 만남에 반응했다.\n전투장을 짧게 터치해 일반공격하세요.":lesson==1?"공격 게이지가 끝에 가까워지면\n전투장에서 좌우로 빠르게 밀어 회피하세요.":lesson==2?"서로의 힘을 믿어보세요.\n오른쪽 아래 스킬 버튼을 누르세요.":"첫 호흡이 맞았습니다!";}
  public bool TutorialAllows(int action){if(!tutorialActive)return true;return lesson==0?action==1:lesson==1?action==3&&!dodgeSent&&controller.CurrentBattle.NextEnemyStrike-controller.CurrentBattle.Time<=.201:lesson==2&&action==2;}
  public void TutorialAction(int action){if(!tutorialActive)return;if(lesson==0&&action==1)SetLesson(1);else if(lesson==1&&action==3)dodgeSent=true;else if(lesson==2&&action==2){lesson=3;lessonEnd=Time.unscaledTime+1.1f;UpdateTutorialText();}}
  public void TutorialCue(CombatCue cue){if(tutorialActive&&lesson==1&&dodgeSent&&cue==CombatCue.EnemyMiss)SetLesson(2);}
  void SetLesson(int n){if(!storyReplay)Session.SaveIntroduction(3,n,false);lesson=n;dodgeSent=false;UpdateTutorialText();}
  public float TutorialDelta(float dt){if(!tutorialActive)return dt;if(lesson==0||lesson==2)return 0;if(lesson==1&&!dodgeSent)return Mathf.Min(dt,Mathf.Max(0,(float)(controller.CurrentBattle.NextEnemyStrike-controller.CurrentBattle.Time-.2)));return dt;}
  void TickTutorial(){if(storyArt!=null&&storyArt.gameObject.activeInHierarchy){storyAge+=Time.unscaledDeltaTime;if(storyCinema!=null)storyCinema.Render(storyAge);else storyArt.localScale=Vector3.one*(1+.012f*Mathf.Sin(Mathf.Min(storyAge,8)/8*Mathf.PI));}if(tutorialActive&&lessonEnd>=0&&Time.unscaledTime>=lessonEnd)FinishTutorial();}
  void StopTutorial(){if(tutorialActive)controller.EndBattle();tutorialActive=false;lessonEnd=-1;storyArt=null;if(tutorialUI!=null){tutorialUI.SetActive(false);Destroy(tutorialUI);tutorialUI=null;}}
  public void FinishTutorial(){if(!tutorialActive)return;if(!storyReplay)Session.SaveIntroduction(3,2,true);StopTutorial();controller.EndBattle();Screen("첫 번째 유대");Portrait(Session.Selected);Label("아직 서툴지만, 서로의 뜻을 알 것 같다.\n탑 꼭대기에서 희미한 목소리가 들려온다.\n\n“……누군가, 왔는가.”",0,590,400,140,21,Color.white);Button("모험을 시작하자",0,730,380,60,ShowLobby);notice.text="연습 완료 · 층 진행과 보상은 지급되지 않습니다.";}
 }
}
