using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Signature presentation only; immunity and armor duration are owned by BattleModel.
 public sealed class GenesisVfx:MonoBehaviour {
  BattleView view;BattleModel model;SkillData skill;EarthStylePrototype earth;BattleVfxHudVisibility materials;
  RectTransform rear,front,window;public RectTransform Numbers{get;private set;}Image[] pool;Image copy,flash;Graphic actor,enemy;Sprite[] atlas=new Sprite[16];
  readonly Vector3[] corners=new Vector3[4];Vector2 foot,target,ground;float age=100,contact=100,armorHit=100;bool hidden,actorWasEnabled,shifted;Vector2 savedEnemy,heldEnemy;int used;
  public bool Configured=>skill!=null&&skill.genesisPresentation;
  public bool DamageFromSkill{get;set;}public int HitsShown{get;private set;}
  public bool Clean=>used==0&&!hidden&&!shifted&&(earth==null||earth.ActiveCount==0);
  public bool Hidden=>hidden;public int PeakImages{get;private set;}public int DroppedLayers=>earth==null?0:earth.DroppedLayers;
  public double ArmorRemaining=>model==null?0:model.CompletionDefenseRemaining;
  public Vector2 LastGround=>foot;
  float Return=>model==null?skill.protectedCastDuration:(float)model.SignatureReturnOffset;
  bool Active=>Configured&&age<Return+1.0f;
  public void Initialize(BattleView owner){view=owner;}
  public void BindBattle(BattleModel b){model=b;}
  public void Configure(SkillData data,Graphic player,Graphic foe){Cancel();skill=data;actor=player;enemy=foe;if(Configured){Ensure();flash.rectTransform.SetParent(enemy.transform,false);}}
  RectTransform Root(string name,bool back){var go=new GameObject(name,typeof(RectTransform),typeof(RectMask2D),typeof(Canvas));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(0,95);r.sizeDelta=new Vector2(480,670);if(back)r.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());return r;}
  void Ensure(){if(pool!=null)return;materials=new BattleVfxHudVisibility();var all=Resources.LoadAll<Sprite>("VFX/Genesis/GenesisAtlas");for(int row=0;row<4;row++)for(int f=0;f<4;f++)foreach(var s in all)if(s.name=="Genesis"+row+"_"+f)atlas[row*4+f]=s;
   rear=Root("Genesis rear geology",true);front=Root("Genesis foreground geology",false);pool=new Image[160];for(int i=0;i<pool.Length;i++){var go=new GameObject("Genesis pooled "+i,typeof(RectTransform),typeof(Image));pool[i]=go.GetComponent<Image>();pool[i].raycastTarget=false;pool[i].rectTransform.SetParent(front,false);go.SetActive(false);}
   earth=gameObject.AddComponent<EarthStylePrototype>();earth.style=Resources.Load<EarthSkillLibrary>("EarthSkills/Library").style;earth.Initialize(view);
   var w=new GameObject("Burrow ground aperture",typeof(RectTransform),typeof(RectMask2D));window=w.GetComponent<RectTransform>();window.SetParent(view.frame,false);window.anchorMin=window.anchorMax=new Vector2(.5f,.5f);window.pivot=new Vector2(.5f,0);window.sizeDelta=new Vector2(480,650);window.SetSiblingIndex(front.GetSiblingIndex());
   var c=new GameObject("Burrow actor copy",typeof(RectTransform),typeof(Image));copy=c.GetComponent<Image>();copy.rectTransform.SetParent(window,false);copy.raycastTarget=false;copy.preserveAspect=true;window.gameObject.SetActive(false);
   var fl=new GameObject("Genesis enemy silhouette",typeof(RectTransform),typeof(Image));flash=fl.GetComponent<Image>();flash.raycastTarget=false;flash.material=Resources.Load<Material>("VFX/Wisp/Silhouette");fl.SetActive(false);
   var numberRoot=new GameObject("Genesis damage overlay",typeof(RectTransform),typeof(Canvas));Numbers=numberRoot.GetComponent<RectTransform>();Numbers.SetParent(view.frame,false);Numbers.anchorMin=Numbers.anchorMax=Numbers.pivot=new Vector2(.5f,1);Numbers.anchoredPosition=Vector2.zero;Numbers.sizeDelta=new Vector2(480,850);
   BattleVfxHudVisibility.BringHudForward(view);
  }
  public void Cast(){if(!Configured||model==null||!model.SignatureCastLocked)return;Cancel();view.TryGetCharacterAnchor(DragonVisualAnchor.GroundPosition,view.frame,out foot);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));ground=target-new Vector2(0,70);age=0;contact=armorHit=100;HitsShown=PeakImages=0;earth.Play(EarthSample.HeavyFinalImpact,foot,ground);Render();}
  public void Hit(){if(!Configured||!DamageFromSkill)return;if(age>50)Cast();HitsShown++;contact=0;age=(float)model.SignatureCastElapsed;heldEnemy=view.enemyArt.rectTransform.anchoredPosition;Render();}
  public void ArmorHit(){if(ArmorRemaining>0)armorHit=0;}
  public void PrepareFrame(){if(shifted){view.enemyArt.rectTransform.anchoredPosition=savedEnemy;shifted=false;}}
  public void Step(BattleModel b,float dt){
   PrepareFrame();if(!Configured)return;
   if(b==null||b.Result==BattleResult.Defeat||(b.Result==BattleResult.Victory&&HitsShown==0)){Cancel();return;}
   if(age>50)return;
   if(b.Result==BattleResult.Fighting){if(!b.SignatureCastLocked&&age<Return-.06f&&!b.CompletionDefenseActive){Cancel();return;}age=(float)b.SignatureCastElapsed;}else age+=dt;
   contact+=dt;armorHit+=dt;Render();
   if(b.Result==BattleResult.Fighting&&age<Return)Burrow();else RestoreActor();
   if(contact<.075f&&HitsShown>0){savedEnemy=view.enemyArt.rectTransform.anchoredPosition;view.enemyArt.rectTransform.anchoredPosition=heldEnemy;shifted=true;}
   if(flash!=null){bool show=HitsShown>0&&contact<.09f;flash.gameObject.SetActive(show);if(show){flash.rectTransform.SetParent(enemy.transform,false);flash.rectTransform.anchorMin=Vector2.zero;flash.rectTransform.anchorMax=Vector2.one;flash.rectTransform.offsetMin=flash.rectTransform.offsetMax=Vector2.zero;flash.sprite=(enemy as Image)?.sprite;flash.preserveAspect=true;flash.color=new Color(1,.91f,.69f,1-contact/.09f);}}
  }
  void Burrow(){var im=actor as Image;if(im==null)return;if(!hidden){actorWasEnabled=actor.enabled;actor.enabled=false;hidden=true;}window.gameObject.SetActive(true);window.anchoredPosition=new Vector2(0,foot.y-5);
   float enter=Mathf.Pow(Mathf.Clamp01((age-.05f)/Mathf.Max(.01f,skill.protectedCastDelay-.05f)),2.6f);
   float emerge=Mathf.Clamp01((age-(Return-.32f))/.25f);float depth=age<Return-.32f?enter:1-Mathf.SmoothStep(0,1,emerge);
   im.rectTransform.GetWorldCorners(corners);Vector2 lo=view.frame.InverseTransformPoint(corners[0]),hi=view.frame.InverseTransformPoint(corners[2]);
   copy.sprite=im.sprite;copy.color=im.color;copy.preserveAspect=im.preserveAspect;
   var r=copy.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,0);r.pivot=Vector2.one*.5f;r.sizeDelta=hi-lo;r.anchoredPosition=(lo+hi)*.5f-new Vector2(0,foot.y-5)-new Vector2(0,(hi.y-foot.y+35)*depth);r.localRotation=im.rectTransform.rotation;
  }
  void RestoreActor(){if(hidden&&actor!=null)actor.enabled=actorWasEnabled;hidden=false;if(window!=null)window.gameObject.SetActive(false);}
  public void Cancel(){PrepareFrame();RestoreActor();age=contact=armorHit=100;HitsShown=0;if(pool!=null)foreach(var im in pool)if(im!=null)im.gameObject.SetActive(false);used=0;if(flash!=null)flash.gameObject.SetActive(false);if(earth!=null)earth.Clear();}
  void OnDisable(){if(Configured)model?.CancelProtectedSkill();Cancel();}
  void OnDestroy(){Cancel();materials?.Dispose();if(rear!=null)Destroy(rear.gameObject);if(front!=null)Destroy(front.gameObject);if(window!=null)Destroy(window.gameObject);if(Numbers!=null)Destroy(Numbers.gameObject);if(flash!=null)Destroy(flash.gameObject);if(earth!=null)Destroy(earth);}
  static float Fade(float t,float from,float to)=>Mathf.Clamp01((to-t)/(to-from));
  void Draw(Sprite sprite,Vector2 p,Vector2 size,float alpha=1,float angle=0,bool back=false,float reveal=1,float shade=1){if(alpha<.003f||reveal<=0)return;if(used>=pool.Length)return;var im=pool[used++];im.sprite=sprite;im.material=materials.alpha;im.color=new Color(shade,shade,shade,Mathf.Clamp01(alpha));im.type=reveal<1?Image.Type.Filled:Image.Type.Simple;im.fillMethod=Image.FillMethod.Vertical;im.fillOrigin=0;im.fillAmount=reveal;var r=im.rectTransform;var parent=back?rear:front;if(r.parent!=parent)r.SetParent(parent,false);r.SetAsLastSibling();r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y-95)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);}
  void Render(){if(pool==null)return;int previous=used;used=0;materials.Update(view);earth.BeginLayers(age,true);
   if(Active){Entry();Travel();Formation();ReturnBurst();if(age<.30f)earth.Impulse(age,6,.16f);else if(contact<.28f)earth.Impulse(contact,19,.28f);else if(age>.35f&&age<.91f)earth.Impulse(Mathf.Repeat(age-.35f,.11f),1.8f,.11f);}
   if(ArmorRemaining>0)Armor();earth.EndLayers();for(int i=used;i<previous;i++)pool[i].gameObject.SetActive(false);PeakImages=Mathf.Max(PeakImages,used+earth.ActiveCount);
  }
  void Entry(){if(age>1.05f)return;earth.Crack(foot,age,1.4f);earth.Impact(foot,age-.04f,.93f,31);for(int i=0;i<8;i++){float t=age-.07f-i*.014f;if(t<0||t>.6f)continue;earth.Dust(foot+new Vector2((i-3.5f)*27,t*45),new Vector2(76+t*60,45+t*90),t/.6f,Fade(t,.22f,.6f),2);}}
  void Travel(){if(age<.3f||age>1.2f)return;float walk=Mathf.Clamp01((age-.3f)/.53f);for(int i=0;i<9;i++){float u=(i+1)/9f;if(u>walk)continue;float t=age-(.3f+u*.53f),hush=age>.91f?Mathf.Min(t,.10f):t;var p=Vector2.Lerp(foot,ground,u);earth.Crack(p,hush,.65f+u*.30f);earth.Layer(EarthModule.GroundSlab,.2f,p+new Vector2(0,Mathf.Sin(Mathf.Clamp01(t/.3f)*Mathf.PI)*13),new Vector2(122,33),Fade(t,.17f,.47f),1);earth.Dust(p+new Vector2(0,17),new Vector2(92,43),hush/.6f,Fade(t,.18f,.65f)*.66f,1);earth.Debris(p,t,.35f,i,false);}
   if(age>.83f){float q=Mathf.Clamp01((age-.83f)/.15f);earth.Crack(ground,.15f,2.2f,q);earth.Layer(EarthModule.Impact,0,ground,new Vector2(270,54),q*.75f,0);for(int j=0;j<12;j++){float a=j*2.399f;earth.Rock(ground+new Vector2(Mathf.Cos(a)*135,15+Mathf.Sin(a)*25),new Vector2(19,13),j*.13f,.85f,1,j*31);}}}
  void Formation(){float due=skill.initialHitDelay;float t=age-due;if(t<-.13f)return;
   // The strongest pose lands on the real damage cue. A short local hold gives
   // impact weight without pausing combat timers or changing global time scale.
   float h=HitsShown>0?Mathf.Max(0,contact-.075f):0;float fade=Fade(t,.62f,1.45f);
   for(int layer=0;layer<3;layer++)for(int j=0;j<3;j++){
    float delay=layer==0?-.13f:layer==1?-.10f:-.065f;
    float grow=Mathf.Clamp01((t-delay)/(-delay));float collapse=Mathf.Max(0,h-.55f);float width=layer==0?175:layer==1?205:185;
    float height=(layer==0?275:layer==1?355:190)+(j==1?40:j==0?-15:-45);
    var p=ground+new Vector2((j-1)*(layer==1?108:145)+(layer-1)*13,layer==0?25:layer==2?-28:0);
    float drop=collapse*collapse*(60+j*25);int frame=Mathf.Clamp((int)(collapse*7),0,3);
    Draw(atlas[(layer==1?0:1)*4+frame],p+new Vector2((j-1)*collapse*22,height*.45f-drop),new Vector2(width,height),fade, (j-1)*(layer==1?8:14)+collapse*(j-1)*11,layer==0,grow,layer==0?.58f:layer==1?.91f:1);
   }
   if(HitsShown==0)return;
   if(contact<.10f)earth.Layer(EarthModule.Impact,contact/.10f,target,new Vector2(310,180),1-contact/.10f,2);
   earth.Crack(ground,h,2.6f);earth.Impact(ground,h,2.1f,44);earth.Shockwave(ground,h-.025f,2.35f,17);
   for(int i=0;i<3;i++)earth.Debris(ground+new Vector2((i-1)*100,40+i%2*45),h-i*.02f,1.25f+i*.18f,51+i,true);
   for(int i=0;i<14;i++){float q=h-.04f-i%4*.025f;if(q<0||q>1.25f)continue;float a=i*2.399f;var p=ground+new Vector2(Mathf.Cos(a)*(80+q*180),40+q*(40+i%3*40));earth.Dust(p,new Vector2(130+q*100,80+q*120),q/1.25f,Fade(q,.3f,1.25f)*.85f,i%3==0?0:2);}
   for(int i=0;i<12;i++){float q=h-.35f-i*.024f;if(q<0||q>.85f)continue;var p=ground+new Vector2(Mathf.Sin(i*2.4f)*(90+q*180),270-q*q*380);earth.Rock(p,Vector2.one*(27+i%4*14)*(1+q*.35f),q+i*.12f,Fade(q,.55f,.85f),i%3==0?2:1,i*27+q*70);}
  }
  void ReturnBurst(){float t=age-(Return-.32f);if(t<0||t>.95f)return;earth.Crack(foot,t,1.25f);earth.Impact(foot,t,.90f,93);earth.Shockwave(foot,t,.78f,4);}
  void Armor(){float remain=(float)ArmorRemaining,fade=Mathf.Clamp01(remain/.65f);Vector2 center;view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,view.frame,out center);Vector2 floor;view.TryGetCharacterAnchor(DragonVisualAnchor.GroundPosition,view.frame,out floor);
   earth.Layer(EarthModule.Pressure,0,floor,new Vector2(215,45),fade*.50f,0);
   for(int i=0;i<6;i++){float a=age*.65f+i*Mathf.PI/3;bool back=Mathf.Sin(a)>0;var p=center+new Vector2(Mathf.Cos(a)*105,Mathf.Sin(a)*27-12);Draw(atlas[8],p,new Vector2(43,60)*(back?.8f:1),fade*(back?.65f:.95f),Mathf.Cos(a)*13,back);earth.Layer(EarthModule.Grain,0,p+new Vector2(-3,9),new Vector2(5,24),fade*.75f,back?0:2,8);}
   for(int i=0;i<9;i++){float u=Mathf.Repeat(age*.45f+i*.113f,1);earth.Layer(EarthModule.Grain,0,floor+new Vector2(Mathf.Sin(i*2.4f)*93,u*95),Vector2.one*(4+i%3),fade*(1-u),1,i*21);}
   if(armorHit<.17f){earth.Layer(EarthModule.Impact,armorHit/.17f,center+new Vector2(58,7),new Vector2(83,66),1-armorHit/.17f,2);earth.Debris(center,armorHit,.32f,2,false);}
  }
 }
}
