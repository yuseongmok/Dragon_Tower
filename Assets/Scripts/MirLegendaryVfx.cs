using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed class MirLegendaryVfx:MonoBehaviour {
  BattleView view;BattleModel bound;SkillData skill;WaterStylePrototype p;MirDragonLayer dragon,echo;Image actor;RawImage volume,blast;Material volumeMat,blastMat;bool hidden,wasEnabled;float age=100,hitAge=100;Vector2 source,target;int hits;
  public bool Configured{get;private set;}public bool DamageFromSkill{get;set;}public bool Clean=>age>=100&&!hidden;public bool ActorVisible=>actor!=null&&actor.enabled;public int HitsShown=>hits;public int Peak=>p==null?0:p.PeakQuads;public int Dropped=>p==null?0:p.DroppedLayers;public bool FinalFreeze=>age<100&&hitAge<(hits==1?.075f:.11f);
  static readonly Color deep=new Color(.015f,.13f,.17f),jade=new Color(.02f,.62f,.42f),cyan=new Color(.13f,.94f,.87f),pale=new Color(.78f,1,.92f),gold=new Color(1,.72f,.20f),whiteGold=new Color(1,.98f,.78f);
  static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));static float F(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));
  public void Initialize(BattleView v)=>view=v;public void BindBattle(BattleModel b)=>bound=b;
  public void Configure(SkillData s,Graphic player){Cancel();skill=s;actor=player as Image;Configured=s!=null&&s.StableId=="skill_hwaryong";if(Configured&&p==null){p=gameObject.AddComponent<WaterStylePrototype>();var old=view.frame.GetComponentsInChildren<LightningMeshLayer>(true);p.Initialize(view);int n=0;foreach(var l in view.frame.GetComponentsInChildren<LightningMeshLayer>(true))if(System.Array.IndexOf(old,l)<0){var cv=l.gameObject.AddComponent<Canvas>();cv.overrideSorting=true;cv.sortingOrder=n++==0?32701:32704;}echo=Layer("Divine dragon afterimage",32702);dragon=Layer("Divine dragon articulated body",32703);volume=Volume("Ascension cloud volume",out volumeMat,32701);blast=Volume("Divine pressure volume",out blastMat,32704);}}
  RawImage Volume(string name,out Material mat,int order){var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(RawImage));var im=go.GetComponent<RawImage>();im.rectTransform.SetParent(view.frame,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=im.rectTransform.pivot=Vector2.one*.5f;im.raycastTarget=false;var cv=go.GetComponent<Canvas>();cv.overrideSorting=true;cv.sortingOrder=order;mat=new Material(Resources.Load<Shader>("MirLegendary/WindVolume"));im.material=mat;go.SetActive(false);return im;}  MirDragonLayer Layer(string name,int order){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(MirDragonLayer));var d=go.GetComponent<MirDragonLayer>();d.Initialize(view,this,order);return d;}
  public void Cast(){if(!Configured||bound==null)return;Cancel();age=0;hitAge=100;hits=0;view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,view.frame,out source);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));Render();}
  public void Hit(){if(!Configured||!DamageFromSkill||age>=100)return;hits++;hitAge=0;age=(float)bound.SignatureCastElapsed;Render();}
  void Hide(){if(hidden||actor==null)return;wasEnabled=actor.enabled;actor.enabled=false;hidden=true;}void Restore(){if(hidden&&actor!=null)actor.enabled=wasEnabled;hidden=false;}
  public void Cancel(){Restore();age=100;hitAge=100;p?.Clear();if(volume!=null)volume.gameObject.SetActive(false);if(blast!=null)blast.gameObject.SetActive(false);dragon?.Clear();echo?.Clear();LegendaryScreenDimming.Set(this,Color.clear);}
  void OnDisable(){if(Configured)bound?.CancelProtectedSkill();Cancel();}void OnDestroy(){Cancel();if(dragon!=null)Destroy(dragon.gameObject);if(echo!=null)Destroy(echo.gameObject);if(p!=null)Destroy(p);if(volume!=null)Destroy(volume.gameObject);if(blast!=null)Destroy(blast.gameObject);if(volumeMat!=null)Destroy(volumeMat);if(blastMat!=null)Destroy(blastMat);}
  public void Step(BattleModel b,float dt){if(!Configured||age>=100)return;if(b==null||b.Result!=BattleResult.Fighting||(!b.SignatureCastLocked&&age<skill.castLockDuration-.06f)){b?.CancelProtectedSkill();Cancel();return;}age=(float)b.SignatureCastElapsed;hitAge+=dt;Render();if(age>=skill.castLockDuration){Restore();Cancel();}}
  // Spatial path is arc-length-like: body sections sample earlier distances, not rigid angles.
  public Vector2 BodyPoint(float time,float tail){
   if(time>=3.12f){float head= Mathf.Lerp(620,0,Mathf.Clamp01((time-3.12f)/(.48f)));if(time>3.6f)head=-(time-3.6f)*700;float y=head+tail*560;return target+new Vector2(Mathf.Sin(tail*8-time*4)*20*tail,y);}
   float travel;
   if(time<.72f)travel=-270;
   else if(time<.95f)travel=Mathf.Lerp(-270,0,(time-.72f)/.23f);
   else if(time<1.025f)travel=0;
   else if(time<1.18f)travel=(time-1.025f)*160;
   else if(time<1.62f)travel=Mathf.Lerp(24.8f,370,(time-1.18f)/.44f);
   else travel=370+(time-1.62f)*880;
   float distance=travel-tail*470;
   if(distance<115)return target+new Vector2(distance,Mathf.Sin(tail*9-time*5)*18*tail);
   float arc=(distance-115)/70;if(arc<Mathf.PI*.5f)return target+new Vector2(115+70*Mathf.Sin(arc),70*(1-Mathf.Cos(arc)));
   float rise=70+distance-115-70*Mathf.PI*.5f;return target+new Vector2(140+Mathf.Sin(rise*.018f)*48,rise);
  }
  void Render(){p.BeginLayers(age);volume.gameObject.SetActive(false);blast.gameObject.SetActive(false);float dim=Mathf.Min(1,age/.18f)*F(age,4.15f,4.7f);LegendaryScreenDimming.Set(this,new Color(.005f,.04f,.04f,.43f*dim));
   if(age<.60f)Gather(source,age/.6f,false);if(age>=.46f&&age<4.43f)Hide();else Restore();
   float visible=age<.54f?0:age<2.85f?Mathf.Min(1,(age-.54f)/.14f):age>=3.12f?F(age,3.72f,3.98f):0;float empowered=age>=3.12f?1:0;
   dragon.Show(age,visible,empowered);echo.Show(age,visible*.22f,empowered,.045f);
   if(visible>0){BodyWind(empowered,visible);if(age<1.8f)SpeedLines(age);}
   if(age>1.55f&&age<3.18f)Ascend();
   if(age>2.72f&&age<3.6f){float q=Mathf.Clamp01((age-2.72f)/.5f);var sky=target+Vector2.up*370;Gather(sky,q,true);if(age>3.12f){p.Line(sky+Vector2.up*120,BodyPoint(age,0),19,A(gold,.5f),1);p.Line(sky+Vector2.up*120,BodyPoint(age,0),4,A(whiteGold,.85f),2);}}
   if(hits>0&&hitAge<1.0f){Impact(hitAge,hits>=2);p.Impulse(hitAge,hits>=2?24:13,hits>=2?.31f:.18f);}
   if(age>3.96f){float u=Mathf.Clamp01((age-3.96f)/.60f);for(int i=0;i<34;i++){float t=Mathf.Clamp01(u-i*.004f);var q=Vector2.Lerp(target+D(i*2.4f)*150,source,t)+Vector2.up*Mathf.Sin(t*Mathf.PI)*80;Cloud(q,7+i%4*2,1-u,gold,2);}Gather(source,u,true);}
   p.EndLayers();
  }
  void Gather(Vector2 at,float u,bool divine){float f=Mathf.Sin(Mathf.Clamp01(u)*Mathf.PI);for(int j=0;j<7;j++)Ribbon(at,25+(1-u)*90+j*7,.45f,age*8+j,6,divine?gold:cyan,f,2,3.8f);for(int i=0;i<24;i++){var q=at+D(i*2.4f+age*4)*(20+(1-u)*90);Cloud(q,4+i%4,f,divine?whiteGold:pale,i%2==0?0:2);}p.Disc(at,5+u*23,.8f,divine?whiteGold:pale,f*.65f,2);}
  void Ribbon(Vector2 at,float r,float tilt,float angle,float width,Color color,float alpha,int depth,float span){for(int i=0;i<32;i++){float u=i/32f,v=(i+1)/32f,a=angle+u*span,b=angle+v*span;var q=at+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*tilt);var n=at+new Vector2(Mathf.Cos(b)*r,Mathf.Sin(b)*r*tilt);p.Line(q,n,width*Mathf.Sin(u*Mathf.PI),A(color,alpha),depth);if(i%4==0)p.Line(q,n,1.4f,A(pale,alpha*.8f),depth);}}
  void Cloud(Vector2 at,float r,float alpha,Color color,int depth){for(int i=0;i<3;i++)p.Disc(at+new Vector2((i-1)*r*.8f,Mathf.Sin(i*2)*r*.3f),r*(i==1?1:.7f),.53f,color,alpha*.35f,depth,12);}
  void BodyWind(float empowered,float alpha){for(int strand=0;strand<7;strand++){Vector2 prev=BodyPoint(age,0);for(int j=1;j<45;j++){float u=j/44f;var q=BodyPoint(age,u);var dir=(q-prev).normalized;var normal=new Vector2(-dir.y,dir.x);float offset=Mathf.Sin(u*15-age*12+strand)*(14+strand*7);var point=q+normal*offset;p.Line(prev,point,strand<2?8:2,A(strand%3==0&&empowered>0?gold:strand%2==0?jade:cyan,alpha*(1-u)*.45f),strand%2==0?0:2);prev=point;}}
   for(int i=0;i<30;i++){float u=Mathf.Repeat(age*.6f+i*.034f,1);var q=BodyPoint(age,u)+D(i*2.4f+age)*(30+i%4*9);Cloud(q,7+i%5*3,alpha*(1-u),empowered>0&&i%3==0?gold:pale,i%3==0?2:0);}}
  void SpeedLines(float t){float f=Mathf.Clamp01((t-.7f)*10)*F(t,1.4f,1.85f);for(int i=0;i<22;i++){float x=Mathf.Repeat(i*61+t*1250,780)-390;var q=target+new Vector2(x,(i-11)*15);p.Line(q-Vector2.right*(40+i%5*18),q,1+i%3,A(i%3==0?pale:cyan,f*.45f),i%2==0?0:2);}}
  void Ascend(){float f=Mathf.Min(1,(age-1.55f)*5)*F(age,2.62f,3.18f);var bottom=target+new Vector2(20,-90);volume.gameObject.SetActive(true);volume.rectTransform.anchoredPosition=bottom+Vector2.up*270;volume.rectTransform.sizeDelta=new Vector2(370,660);volumeMat.SetFloat("_Age",age);volumeMat.SetFloat("_Opacity",f*.85f);volumeMat.SetFloat("_Mode",0);
   for(int j=0;j<7;j++){float y=j*75;float r=120+Mathf.Sin(j*.9f)*24;Ribbon(bottom+Vector2.up*y,r,.35f,age*8-j*1.7f,6,j%2==0?jade:cyan,f*.42f,j%2==0?0:2,1.9f);}
   for(int i=0;i<46;i++){float u=Mathf.Repeat(age*.8f+i*.023f,1);Cloud(bottom+new Vector2(Mathf.Sin(i*2.4f+age*3)*155,u*550),12+i%4*7,f*(1-u),pale,i%3==0?2:0);}}  void Impact(float t,bool final){float scale=final?2.1f:1;float expand=1-Mathf.Exp(-t*14);float fade=F(t,.16f,final?1.0f:.65f);var hot=final?whiteGold:pale;blast.gameObject.SetActive(true);blast.rectTransform.anchoredPosition=target;blast.rectTransform.sizeDelta=new Vector2(440,390)*scale*(.35f+expand*.65f);blastMat.SetFloat("_Mode",1);blastMat.SetFloat("_Age",t);blastMat.SetFloat("_Gold",final?1:0);blastMat.SetFloat("_Opacity",fade*.9f);if(t<.09f){p.Disc(target,(8+expand*80)*scale,.8f,hot,F(t,.015f,.09f),2,32);}
   if(final)ContactExplosion(t);
   for(int ring=0;ring<3;ring++)Ribbon(target,scale*(25+expand*(95+ring*20)),.4f+ring*.1f,t*5+ring,5-ring*1.4f,final&&ring%2==0?gold:cyan,fade,2,6.15f);
   for(int i=0;i<(final?90:38);i++){var dir=D(i*2.39996f);var q=target+dir*(30+expand*(70+i%11*20))*scale+Vector2.down*t*t*35;p.Line(q-dir*(12+i%5*7)*fade,q,(2+i%3)*fade,A(final&&i%3==0?gold:pale,fade),i%4==0?0:2);if(i%3==0)Cloud(q,8+i%7*2,fade,final&&i%2==0?gold:pale,2);}
  }
  // A separate, target-centred detonation makes the vertical contact readable.
  // Driven only by the existing final damage cue; it never schedules damage.
  void ContactExplosion(float t){
   float open=1-Mathf.Exp(-t*22),core=F(t,.055f,.24f),burst=F(t,.10f,.52f);
   p.Disc(target,65+open*100,.85f,gold,core*.8f,2,40);
   p.Disc(target,42+open*68,.88f,whiteGold,core,2,40);
   for(int i=0;i<16;i++){
    float a=i*2.39996f;var dir=D(a);float reach=(105+i%5*27)*open;
    var start=target+dir*(16+open*20);var end=target+dir*reach;
    p.Line(start,end,(19+i%3*8)*burst,A(i%3==0?cyan:gold,burst*.85f),2);
    p.Line(start,end,5*burst,A(whiteGold,burst),2);
   }
   for(int i=0;i<12;i++){
    float delay=(i%3)*.025f,u=Mathf.Max(0,t-delay),f=F(u,.09f,.58f);
    if(t<delay)continue;var dir=D(i*Mathf.PI*2/12);
    var q=target+new Vector2(dir.x,dir.y*.72f)*(48+(1-Mathf.Exp(-u*12))*190);
    Cloud(q,24+u*36,f,i%3==0?gold:cyan,2);
   }
   float ring=1-Mathf.Exp(-t*11);
   Ribbon(target,70+ring*245,.55f,t*2,13*F(t,.08f,.42f),gold,F(t,.08f,.48f),2,6.27f);
   Ribbon(target,48+ring*205,.75f,-t*3,6*F(t,.06f,.35f),pale,F(t,.06f,.40f),2,6.27f);
  }
 }
}
