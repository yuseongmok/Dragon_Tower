using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace DragonTower {
 public partial class BattleView {
  BattleResultPresentation resultPresentation;
  TowerRun resultRun;
  int resultStartScore;
  public void PrepareResultPresentation(TowerRun run){resultRun=run;resultStartScore=run?.Progress?.score??0;HideResultPresentation();}
  public void HideResultPresentation(){if(resultPresentation!=null)resultPresentation.ResetPresentation();if(resultPanel!=null)resultPanel.SetActive(false);}
  void ShowResultPresentation(BattleModel battle,bool deathFinished){
   if(battle.Result==BattleResult.Fighting){HideResultPresentation();return;}
   if(!deathFinished)return;
   if(resultPresentation==null){resultPresentation=resultPanel.AddComponent<BattleResultPresentation>();resultPresentation.Build(this);}
   if(!resultPanel.activeSelf){resultPanel.transform.SetAsLastSibling();resultPanel.SetActive(true);resultPresentation.Present(battle,resultRun,resultStartScore);CancelGesture();}
   if(lastAudioResult!=battle.Result){lastAudioResult=battle.Result;DragonTowerAudio.PlayResult(battle.Result==BattleResult.Victory);}
  }
 }
 // Presentation only: this component never awards rewards or writes a save.
 public sealed class BattleResultPresentation:MonoBehaviour,IPointerClickHandler {
  BattleView view;Image backdrop;CanvasGroup cardGroup,titleGroup,infoGroup,actionGroup;RectTransform card;
  Text title,subtitle,partner,hint;Text[] labels=new Text[5],values=new Text[5];GameObject[] rows=new GameObject[5];
  ResultEmblem emblem;ResultMotes motes;float age;bool showing,victory;public bool Ready=>showing&&age>=.5f;
  RectTransform Rect(string name,Transform parent,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
  Image Panel(string name,Transform parent,float x,float y,float w,float h){var im=Rect(name,parent,x,y,w,h).gameObject.AddComponent<Image>();im.raycastTarget=false;AncientUi.Frame(im);return im;}
  Text Text(string name,Transform parent,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleCenter){var t=Rect(name,parent,x,y,w,h).gameObject.AddComponent<Text>();t.font=view.font;t.text=name;t.fontSize=size;t.color=color;t.alignment=align;t.supportRichText=false;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=14;t.resizeTextMaxSize=size;return t;}
  public void Build(BattleView owner){
   view=owner;backdrop=GetComponent<Image>();backdrop.raycastTarget=true;
   // Keep the original result button and its existing progression callback.
   view.restartButton.transform.SetParent(view.frame,false);
   foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   var canvas=gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=200;gameObject.AddComponent<GraphicRaycaster>();
   card=Panel("Result stone frame",transform,0,438,432,640).rectTransform;cardGroup=card.gameObject.AddComponent<CanvasGroup>();
   var inset=Panel("Recessed record",card,0,418,380,245);AncientUi.Frame(inset,AncientSurfaceKind.Information);
   motes=Rect("Gold and ash",transform,0,425,480,850).gameObject.AddComponent<ResultMotes>();motes.raycastTarget=false;
   emblem=Rect("Victory laurel / broken stone",card,0,88,180,130).gameObject.AddComponent<ResultEmblem>();emblem.raycastTarget=false;
   var titleRoot=Rect("Result heading",card,0,221,400,130);titleGroup=titleRoot.gameObject.AddComponent<CanvasGroup>();
   title=Text("",titleRoot,0,28,380,58,43,AncientUi.Gold);subtitle=Text("",titleRoot,0,77,380,28,19,AncientUi.Ivory);
   partner=Text("",titleRoot,0,109,366,30,18,AncientUi.Muted);
   infoGroup=inset.gameObject.AddComponent<CanvasGroup>();
   for(int i=0;i<5;i++){var row=Rect("Record row",inset.transform,0,28+i*46,340,40);rows[i]=row.gameObject;labels[i]=Text("",row,-64,20,212,32,19,AncientUi.Muted,TextAnchor.MiddleLeft);values[i]=Text("",row,108,20,128,32,22,AncientUi.Ivory,TextAnchor.MiddleRight);}
   view.restartButton.transform.SetParent(card,false);var br=(RectTransform)view.restartButton.transform;br.anchorMin=br.anchorMax=new Vector2(.5f,1);br.pivot=new Vector2(.5f,.5f);br.anchoredPosition=new Vector2(0,-587);br.sizeDelta=new Vector2(366,64);
   actionGroup=br.gameObject.AddComponent<CanvasGroup>();var bt=view.restartButton.GetComponentInChildren<Text>();bt.rectTransform.anchorMin=Vector2.zero;bt.rectTransform.anchorMax=Vector2.one;bt.rectTransform.offsetMin=new Vector2(10,5);bt.rectTransform.offsetMax=new Vector2(-10,-5);bt.fontSize=25;
   hint=Text("터치하면 연출을 빠르게 마칩니다",transform,0,793,440,30,15,AncientUi.Muted);
   view.resultTitle=title;view.resultDetail=subtitle;
  }
  void Row(int i,string label,string value){labels[i].text=label;values[i].text=value;}
  public void Present(BattleModel battle,TowerRun run,int startingScore){
   showing=true;victory=battle.Result==BattleResult.Victory;age=0;emblem.defeat=motes.defeat=!victory;
   var p=run?.Progress;title.text=victory?"승리!":"도전 종료";title.color=victory?AncientUi.Gold:AncientUi.Ivory;
   subtitle.text=victory?(run==null?"전투 승리":run.Floor+"층 정복 완료"):"드래곤과 함께한 여정의 기록";
   string name=string.IsNullOrEmpty(p?.customDragonName)?battle.Dragon.displayName:p.customDragonName;
   partner.text=string.IsNullOrEmpty(p?.originalDragonName)||name==p.originalDragonName?name:name+"  ·  "+p.originalDragonName;
   for(int i=0;i<5;i++)rows[i].SetActive(true);
   if(victory){Row(0,"남은 체력",battle.PlayerHP+" / "+battle.Dragon.maxHP);Row(1,"유대 경험치",run==null?"—":"+"+run.LastExperienceGain.ToString("N0"));Row(2,"획득 점수",p==null?"—":"+"+Mathf.Max(0,p.score-startingScore).ToString("N0"));Row(3,"획득 골드",run==null?"—":"+"+run.LastBattleGold.ToString("N0"));rows[4].SetActive(false);}
   else{Row(0,"최대 도달 층",(p?.maxFloor??0)+"층");Row(1,"최대 유대",(p?.maxBond??0).ToString());Row(2,"몬스터 처치",(run?.MonstersDefeated??0).ToString("N0"));Row(3,"최종 점수",(p?.score??0).ToString("N0"));Row(4,"획득 업적",(p?.achievements?.Count??0)+"개");}
   view.restartButton.GetComponentInChildren<Text>().text=victory?"다음 층으로":"도전 결과 확인";AncientUi.Button(view.restartButton,victory);Animate();
  }
  public void ResetPresentation(){showing=false;age=0;if(view!=null)view.restartButton.interactable=false;}
  public void OnPointerClick(PointerEventData e){if(showing&&!Ready){age=.5f;Animate();}}
  void Update(){if(showing){age+=Time.unscaledDeltaTime;Animate();}}
  void Animate(){
   float t=Mathf.Clamp01(age/.5f);backdrop.color=new Color(.055f,.06f,.075f,Mathf.Lerp(0,victory?.76f:.86f,Mathf.Clamp01(t*3)));
   cardGroup.alpha=Mathf.Clamp01(t*3);card.anchoredPosition=new Vector2(0,-438-12*(1-Mathf.Clamp01(t*3)));
   titleGroup.alpha=Mathf.Clamp01((t-.22f)*4);infoGroup.alpha=Mathf.Clamp01((t-.45f)*3);actionGroup.alpha=Mathf.Clamp01((t-.7f)*4);
   view.restartButton.interactable=Ready;actionGroup.blocksRaycasts=Ready;hint.text=Ready?(victory?"남은 보상 절차를 마친 뒤 다음 층으로 이동합니다":"함께한 여정은 도전 기록에 남습니다"):"터치하면 연출을 빠르게 마칩니다";
   emblem.age=motes.age=age;emblem.SetVerticesDirty();motes.SetVerticesDirty();
  }
 }
 // Pixel geometry follows the existing charcoal UI icon system, without new textures/materials.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ResultEmblem:MaskableGraphic {
  public bool defeat;public float age;
  static void Box(VertexHelper v,float x,float y,float w,float h,Color c){int n=v.currentVertCount;v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();Color gold=defeat?Color.Lerp(AncientUi.Gold,AncientUi.Muted,Mathf.Clamp01(age*2)):AncientUi.Gold;Color edge=defeat?AncientUi.Hex(0x55565A):AncientUi.DarkGold;
   for(int side=-1;side<=1;side+=2)for(int i=0;i<8;i++){float a=(-70+i*20)*Mathf.Deg2Rad;float x=side*Mathf.Round((48+14*Mathf.Cos(a))/2)*2,y=Mathf.Round(48*Mathf.Sin(a)/2)*2;Box(v,x-3,y-3,6,12,edge);Box(v,x-8,y+2,16,6,gold);Box(v,x-5,y+8,10,3,AncientUi.Ivory);}
   Box(v,-28,-38,56,70,edge);Box(v,-24,-34,48,64,AncientUi.Background);Box(v,-18,-30,36,50,gold);Box(v,-23,20,46,12,gold);for(int i=-1;i<=1;i++)Box(v,i*18-5,29,10,11,gold);Box(v,-5,-30,10,20,AncientUi.Background);Box(v,-7,1,14,13,AncientUi.Ivory);Box(v,-22,-45,44,5,edge);
   if(defeat){for(int i=0;i<13;i++){float x=(i<5?i*2:20-i*2)-8;Box(v,x,33-i*6,5,8,AncientUi.Background);}Box(v,-27,-2,19,3,AncientUi.Hex(0x734A49));Box(v,12,-20,15,3,AncientUi.Hex(0x734A49));}
  }
 }
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ResultMotes:MaskableGraphic {
  public bool defeat;public float age;
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();if(defeat&&age>.2f){for(int side=-1;side<=1;side+=2)for(int j=0;j<7;j++){float x=side*(207-(j%3)*3),y=285-j*5;int n=v.currentVertCount;Color crack=AncientUi.Hex(0x17181D);v.AddVert(new Vector3(x,y),crack,Vector2.zero);v.AddVert(new Vector3(x,y+6),crack,Vector2.zero);v.AddVert(new Vector3(x+2,y+6),crack,Vector2.zero);v.AddVert(new Vector3(x+2,y),crack,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}}for(int i=0;i<28;i++){float cycle=Mathf.Repeat(age*.18f+i*.137f,1);float x=Mathf.Round((Mathf.Sin(i*5.72f)*210)/2)*2;float y=(cycle-.5f)*680*(defeat?-1:1);float size=i%5==0?4:2;Color c=defeat?(i%3==0?AncientUi.Hex(0x824A49):AncientUi.Muted):AncientUi.Gold;c.a=Mathf.Sin(cycle*Mathf.PI)*.65f*Mathf.Clamp01(age*5);int n=v.currentVertCount;v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x,y+size),c,Vector2.zero);v.AddVert(new Vector3(x+size,y+size),c,Vector2.zero);v.AddVert(new Vector3(x+size,y),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}}
 }
}
