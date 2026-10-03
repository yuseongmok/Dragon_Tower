using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Presentation-only: all impact and recovery cues originate in BattleModel.
 public sealed class SolarPierceVfx:MonoBehaviour {
  const int Capacity=480;BattleModel model;public void BindBattle(BattleModel battle){model=battle;}
  BattleView view;SkillData skill;FireVfxStyle fire;BattleVfxHudVisibility materials;Image[] pool;RectTransform rear,front;Sprite[] atlas=new Sprite[48];Image silhouette,arena;Graphic enemyGraphic;Vector2 arenaHome;bool shifted;Color enemyColor;float age=100,finalAge=-1,healAge=100,contactAge=100;Vector2 source,target,playerCenter;int used;
  public bool Configured=>skill!=null&&skill.solarPresentation;public bool DamageFromSkill{get;set;}public int ScheduledHits{get;set;}public int HitCount=>Mathf.Max(skill==null?1:skill.hitCount,ScheduledHits);public int HitsShown{get;private set;}public int PeakImages{get;private set;}public int DroppedLayers{get;private set;}public bool Clean=>used==0&&!shifted;public bool Active=>Configured&&age<End+1.25f;float End=>skill==null?0:skill.initialHitDelay+(HitCount-1)*skill.hitInterval;
  public void Initialize(BattleView owner){view=owner;}
  public void Configure(SkillData data,Graphic graphic){Cancel();skill=data;enemyGraphic=graphic;if(!Configured)return;Ensure();silhouette.rectTransform.SetParent(graphic.transform,false);silhouette.rectTransform.anchorMin=Vector2.zero;silhouette.rectTransform.anchorMax=Vector2.one;silhouette.rectTransform.offsetMin=silhouette.rectTransform.offsetMax=Vector2.zero;var image=graphic as Image;silhouette.sprite=image==null?null:image.sprite;silhouette.preserveAspect=image!=null&&image.preserveAspect;}
  RectTransform Root(string name){var go=new GameObject(name,typeof(RectTransform),typeof(RectMask2D),typeof(Canvas));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(0,95);r.sizeDelta=new Vector2(480,660);return r;}
  void Ensure(){if(pool!=null)return;materials=new BattleVfxHudVisibility();fire=Resources.Load<FireSkillLibrary>("FireSkills/Library").style;var sprites=Resources.LoadAll<Sprite>("VFX/SolarPierce/SolarAtlas");for(int row=0;row<6;row++)for(int f=0;f<8;f++)foreach(var s in sprites)if(s.name=="Solar"+row+"_"+f)atlas[row*8+f]=s;rear=Root("Solar rear");rear.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());front=Root("Solar foreground");BattleVfxHudVisibility.BringHudForward(view);pool=new Image[Capacity];for(int i=0;i<Capacity;i++){var go=new GameObject("Solar pooled "+i,typeof(RectTransform),typeof(Image));var im=go.GetComponent<Image>();im.rectTransform.SetParent(front,false);im.raycastTarget=false;go.SetActive(false);pool[i]=im;}var flash=new GameObject("Solar silhouette",typeof(RectTransform),typeof(Image));silhouette=flash.GetComponent<Image>();silhouette.raycastTarget=false;silhouette.material=Resources.Load<Material>("VFX/Wisp/Silhouette");flash.SetActive(false);arena=view.frame.Find("Arena").GetComponent<Image>();}
  public void Cast(){if(!Configured)return;Cancel();view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out source);view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,view.frame,out playerCenter);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));age=0;HitsShown=PeakImages=DroppedLayers=0;Render();}
  public void Hit(){if(!Active)Cast();HitsShown++;contactAge=0;if(HitsShown>=HitCount)finalAge=0;Render();}
  public void Heal(){if(Active){healAge=0;Render();}}
  public void Step(BattleModel battle,float dt){Restore();if(!Configured||battle==null||battle.Result==BattleResult.Defeat||(battle.Result==BattleResult.Victory&&finalAge<0)){Cancel();return;}age+=dt;contactAge+=dt;healAge+=dt;if(finalAge>=0)finalAge+=dt;Render();if(finalAge>=0&&finalAge<.18f){arenaHome=arena.rectTransform.anchoredPosition;arena.rectTransform.anchoredPosition=arenaHome+new Vector2(Mathf.Sin(finalAge*110)*7,Mathf.Cos(finalAge*91)*4)*(1-finalAge/.18f);shifted=true;}if(silhouette!=null){bool flash=contactAge<.045f;silhouette.gameObject.SetActive(flash);silhouette.color=new Color(1,.94f,.65f,flash?1:0);}}
  void Restore(){if(shifted&&arena!=null)arena.rectTransform.anchoredPosition=arenaHome;shifted=false;}
  public void Cancel(){Restore();age=100;finalAge=-1;healAge=contactAge=100;HitsShown=0;if(pool!=null)foreach(var im in pool)if(im!=null)im.gameObject.SetActive(false);used=0;if(silhouette!=null)silhouette.gameObject.SetActive(false);}
  void OnDisable(){model?.CancelCompletionHeal();Cancel();}void OnDestroy(){Cancel();materials?.Dispose();if(front!=null)Destroy(front.gameObject);if(rear!=null)Destroy(rear.gameObject);if(silhouette!=null)Destroy(silhouette.gameObject);}
  Sprite S(int row,float time)=>atlas[row*8+Mathf.FloorToInt(Mathf.Repeat(time*14,8))];
  void Draw(Sprite sprite,Vector2 p,Vector2 size,Color c,float angle=0,bool back=false,bool glow=false){if(c.a<=.002f)return;if(used>=Capacity){DroppedLayers++;return;}var im=pool[used++];im.sprite=sprite;im.material=glow?materials.additive:materials.alpha;im.color=c;var r=im.rectTransform;var parent=back?rear:front;if(r.parent!=parent)r.SetParent(parent,false);r.SetAsLastSibling();r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y-95)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);}
  static Color White(float a)=>new Color(1,1,1,Mathf.Clamp01(a));
  void F(FireStyleModule module,Vector2 p,Vector2 size,float a=1,float angle=0,bool back=false,bool glow=false,float phase=-1){Draw(phase<0?fire.Get(module,age):fire.Frame(module,phase),p,size,White(a),angle,back,glow);}
  void Feather(Vector2 p,float length,float angle,float a,bool back=false,bool bright=false){Draw(S(1,age),p,new Vector2(length*.65f,length),White(a),angle,back);if(bright)Draw(S(1,age+.2f),p,new Vector2(length*.20f,length*.9f),new Color(1,1,.84f,a*.75f),angle,back,true);}
  void Render(){if(pool==null)return;int previous=used;used=0;materials.Update(view);if(Active){float final=finalAge;float dim=Mathf.Clamp01(age/.12f)*Mathf.Clamp01((End+1.0f-age)/.6f);Draw(null,new Vector2(0,95),new Vector2(480,660),new Color(.14f,.035f,.025f,dim*.37f),0,true);
    if(age<.32f)for(int i=0;i<24;i++){float t=Mathf.Clamp01(age/.32f),a=i*2.4f;var p=Vector2.Lerp(source+new Vector2(Mathf.Cos(a)*110,Mathf.Sin(a)*80),target,t*t);Feather(p,15+20*t,-a*Mathf.Rad2Deg,1-t*.4f,i%3==0,true);}
    if(final<0)Sun();if(final<0&&age>Mathf.Max(.35f,End-.94f))PhoenixFlight();if(contactAge<.14f&&final<0){F(FireStyleModule.Burst,target,Vector2.one*(85+contactAge*180),1-contactAge/.14f,0,false,true,contactAge/.14f);F(FireStyleModule.Shockwave,target,Vector2.one*(85+contactAge*500),1-contactAge/.14f,0,false,true,contactAge/.14f);}
    if(final>=0)Explosion(final);if(healAge<.5f)Healing(healAge);
   }for(int i=used;i<previous;i++)pool[i].gameObject.SetActive(false);PeakImages=Mathf.Max(PeakImages,used);}
  void Sun(){float grow=Mathf.SmoothStep(.12f,1,Mathf.Clamp01(age/.28f));float collapse=Mathf.Clamp01((age-(End-.075f))/.075f);float scale=grow*(1-collapse*.42f);var c=target+new Vector2(-12,10);float diameter=390*scale;
   for(int i=0;i<15;i++){float a=i*2.399f+age*(i%2==0?.35f:-.18f);var p=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*diameter*.46f;F(FireStyleModule.Tongue,p,new Vector2(35,90)*scale,.8f,a*Mathf.Rad2Deg-90,true);}
   Draw(S(5,age*.65f),c,Vector2.one*diameter*1.18f,new Color(1,.68f,.3f,1),-age*19,true);Draw(S(5,-age*.8f),c+new Vector2(-7,6),Vector2.one*diameter,White(1),age*27,true);Draw(S(5,age*1.7f),c+new Vector2(-diameter*.09f,diameter*.08f),Vector2.one*diameter*.64f,new Color(1,1,.8f,.65f+collapse*.35f),-age*15,true,true);
   for(int i=0;i<22;i++){float a=i*2.4f+age*(.6f+i%3*.2f);bool back=Mathf.Sin(a)>0;var p=c+new Vector2(Mathf.Cos(a)*diameter*.56f,Mathf.Sin(a)*diameter*.26f);Feather(p,38+i%4*13,a*Mathf.Rad2Deg-90,.85f,back,i%4==0);}
   for(int i=0;i<18;i++){float u=Mathf.Repeat(age*.7f+i*.137f,1),a=i*2.4f;F(FireStyleModule.Ember,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(115+u*95),Vector2.one*(14+i%3*5),(1-u)*.8f,i*25,i%2==0,true);}
   for(int ring=0;ring<3;ring++)Draw(S(3,age*.28f+ring*.15f),c,new Vector2(diameter*1.55f,diameter*(.55f+ring*.12f)),new Color(1,.94f,.64f,.62f),ring*57+age*(ring%2==0?17:-24),ring==0,true);
   if(collapse>0)F(FireStyleModule.Core,c,Vector2.one*(140-collapse*50),collapse,0,false,true);
  }
  void PhoenixFlight(){float start=End-.94f,charge=End-.34f,contact=End-.075f;float u=Mathf.Clamp01((age-charge)/(contact-charge));float ease=u*u;var home=source+new Vector2(-45,85);var p=Vector2.Lerp(home,target,ease);float appear=Mathf.Clamp01((age-start)/.15f);float compress=Mathf.Clamp01((age-contact)/.075f);float scale=appear*(1-compress*.72f);float angle=-17+u*20;float flap=Mathf.Sin(Mathf.Clamp01((age-start)/.6f)*Mathf.PI)*.85f;
   if(u>0){var d=p-home;for(int i=0;i<10;i++){float k=(i+1)/11f;var q=Vector2.Lerp(home,p,k);F(FireStyleModule.Tongue,q,new Vector2(75*(1-k)+35,d.magnitude*.30f),(.3f+.6f*k)*(1-compress),Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90,true);Feather(q+new Vector2(Mathf.Sin(i*3)*40,0),90+80*k,-35,.6f*(1-compress),true,true);}}
   Phoenix(p,scale*.70f,angle,flap,1-compress*.5f);
  }
  Vector2 Rotate(Vector2 p,float angle){float a=angle*Mathf.Deg2Rad;return new Vector2(p.x*Mathf.Cos(a)-p.y*Mathf.Sin(a),p.x*Mathf.Sin(a)+p.y*Mathf.Cos(a));}
  void FlameRibbon(Vector2 p,Vector2 size,float angle,float alpha,bool back=false){
   Draw(S(2,age*.85f),p,size*1.2f,new Color(1,.42f,.12f,alpha*.65f),angle,back);
   Draw(S(2,age*1.15f+.14f),p,size,White(alpha),angle,back);
   Draw(S(2,age*1.5f+.3f),p,Vector2.Scale(size,new Vector2(.38f,.92f)),new Color(1,1,.84f,alpha*.65f),angle,back,true);
  }
  void Phoenix(Vector2 p,float scale,float angle,float flap,float alpha){
   // Connected flame currents imply swept wings. Gaps and flicker keep it an energy apparition.
   for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++){
    float u=i/4f;var local=new Vector2(side*(25+u*132),20+u*55+flap*u*24+Mathf.Sin(age*17+i)*5);
    float sweep=angle-side*(32+u*40);FlameRibbon(p+Rotate(local*scale,angle),new Vector2(95-u*16,145-u*22)*scale,sweep,alpha*(1-u*.22f));
    if(i>1)Draw(S(2,age*.9f+i*.12f),p+Rotate((local+new Vector2(-side*15,-28))*scale,angle),new Vector2(54,135)*scale,White(alpha*.30f),sweep+side*14,true);
   }
   for(int i=0;i<3;i++)FlameRibbon(p+Rotate(new Vector2((i-1)*22,-72-i%2*17)*scale,angle),new Vector2(72,180)*scale,angle+(i-1)*19,alpha*.65f,true);
   FlameRibbon(p,new Vector2(100,185)*scale,angle,alpha);
   FlameRibbon(p+Rotate(new Vector2(8,78)*scale,angle),new Vector2(62,86)*scale,angle-25,alpha);
   for(int i=0;i<14;i++){float u=Mathf.Repeat(age*1.8f+i*.137f,1);var local=new Vector2(Mathf.Sin(i*2.4f)*(45+u*110),40-u*180);F(FireStyleModule.Ember,p+Rotate(local*scale,angle),new Vector2(10,22)*scale,alpha*(1-u),angle+i*23,false,true);}
  }
  void Explosion(float t){float h=t<.06f?0:t-.06f;float expand=1-Mathf.Pow(1-Mathf.Clamp01(h/.27f),3);float fade=Mathf.Clamp01((1.02f-t)/.48f);if(t<.12f)Draw(null,new Vector2(0,95),new Vector2(480,660),new Color(1,.86f,.48f,(1-t/.12f)*.58f),0,false,true);
   if(t<.85f){for(int ring=0;ring<3;ring++)Draw(S(3,h*.7f+ring*.14f),target,new Vector2(110+expand*(620+ring*35),100+expand*(280+ring*90)),new Color(1,.94f,.72f,fade*.85f),ring*48+h*17,ring==0,true);F(FireStyleModule.Burst,target,Vector2.one*(110+expand*530),fade,-13,true,false,h/.85f);Draw(S(0,age*2),target,Vector2.one*(60+expand*390),new Color(1,.92f,.65f,fade),age*30);F(FireStyleModule.Burst,target+new Vector2(-8,10),Vector2.one*(80+expand*420),fade,29,false,true,h/.65f);if(t<.19f)Draw(S(0,age),target,Vector2.one*(140+expand*180),new Color(1,1,.93f,1-t/.19f),0,false,true);
    Draw(S(3,t),target,Vector2.one*(140+expand*560),White(fade),t*40,false,true);F(FireStyleModule.Shockwave,target-new Vector2(0,65),new Vector2(180+expand*560,50+expand*180),fade,0,false,true,h/.85f);}
   for(int i=0;i<24;i++){float a=i*2.399f;var dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.85f);var p=target+dir*(25+h*(180+i%4*60));Feather(p,(70+i%5*12)*(1-h*.5f),-a*Mathf.Rad2Deg+90,fade,i%3==0,i%2==0);}
   for(int i=0;i<42;i++){float a=i*2.399f;var p=target+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(30+h*(190+i%5*45))+new Vector2(0,-90*h*h);F(FireStyleModule.Ember,p,new Vector2(14,27)*(1+i%3*.3f),fade,i*23,i%3==0,true);}
   for(int i=0;i<8;i++){float a=i*2.4f;var p=target+new Vector2(Mathf.Cos(a)*150,Mathf.Sin(a)*115)*expand;if(t>.13f)F(FireStyleModule.Smoke,p,Vector2.one*(90+h*80),fade*.44f,i*40,true);}
   float returnTime=Mathf.Max(.06f,skill.completionHealDelay);if(t<returnTime){float u=Mathf.Clamp01(t/returnTime);for(int i=0;i<9;i++){var p=Vector2.Lerp(target,playerCenter,u)+new Vector2(Mathf.Sin(u*Mathf.PI)*(i-4)*16,Mathf.Sin(u*Mathf.PI)*55);Feather(p,42-i%3*7,-165,1,false,true);}}
  }
  void Healing(float t){float fade=1-t/.5f;Draw(S(3,t),playerCenter,Vector2.one*(80+t*260),new Color(1,1,.72f,fade),0,false,true);F(FireStyleModule.Core,playerCenter,Vector2.one*(110-t*100),fade,0,false,true);for(int i=0;i<10;i++){float a=i*.628f+t*5;Feather(playerCenter+new Vector2(Mathf.Cos(a)*85,Mathf.Sin(a)*42+t*65),42,-a*Mathf.Rad2Deg,fade,i%2==0,true);}}
 }
}
