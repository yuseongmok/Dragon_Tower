using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Sacred artillery presentation; all impacts are driven by real combat cues.
 public sealed class CelestialVfx:MonoBehaviour {
  Graphic enemy;BattleView view;LightStylePrototype p;SkillData skill;Image flash;Material flashMaterial;
  float clock,castAt=-100,hitAt=-100,staffAt=-100;Vector2 target,heldEnemy,normalEnemy;Vector3 heldScale,normalScale;Quaternion heldRotation,normalRotation;bool held;
  struct Portal {public Vector3 offset,tilt;public float radius;public int depth;public Portal(Vector3 o,Vector3 t,float r,int d){offset=o;tilt=t;radius=r;depth=d;}}
  static readonly Portal[] portals={
   new Portal(new Vector3(-110,116,150),new Vector3(34,42,-18),82,0),new Portal(new Vector3(115,100,120),new Vector3(39,-46,23),85,0),
   new Portal(new Vector3(0,155,55),new Vector3(68,-8,0),100,0),new Portal(new Vector3(-176,13,35),new Vector3(15,62,-16),100,0),
   new Portal(new Vector3(178,0,15),new Vector3(12,-63,19),107,1),new Portal(new Vector3(-138,-103,-65),new Vector3(36,43,-29),118,1),
   new Portal(new Vector3(142,-118,-85),new Vector3(39,-47,31),128,1),new Portal(new Vector3(-25,-148,-135),new Vector3(64,12,-10),111,1)};
  struct Mini {public bool active,hit;public int id,pattern;public float start,contact;public Vector2 target;}
  readonly Mini[] minis=new Mini[128];int miniCursor;
  public bool Configured{get;private set;}public bool DamageFromSkill{get;set;}public int MainHits{get;private set;}public int MiniHits{get;private set;}public int MiniCharges{get;private set;}
  public int LivePortals{get;private set;}public int LiveBeams{get;private set;}public int MaxBeams{get;private set;}
  public int Peak=>p==null?0:p.PeakQuads;public int Dropped=>p==null?0:p.DroppedLayers;public bool Clean=>(p==null||p.ActiveCount==0)&&!held&&(flash==null||!flash.gameObject.activeSelf);
  public Vector2 LastStaffOrigin{get;private set;}
  float FireAt=>Mathf.Max(.18f,skill.initialHitDelay-.28f);
  public void Initialize(BattleView owner)=>view=owner;
  public void Configure(SkillData s,Graphic graphic){Cancel();enemy=graphic;skill=s;Configured=s!=null&&s.celestialSignature;if(!Configured)return;if(p==null){p=gameObject.AddComponent<LightStylePrototype>();p.style=Resources.Load<LightSkillLibrary>("LightSkills/Library").style;p.Initialize(view);var go=new GameObject("Celestial holy silhouette",typeof(RectTransform),typeof(Image));flash=go.GetComponent<Image>();flash.raycastTarget=false;flashMaterial=new Material(Resources.Load<Shader>("VFX/GaleImpactSilhouette"));flash.material=flashMaterial;}flash.rectTransform.SetParent(enemy.transform,false);flash.rectTransform.anchorMin=Vector2.zero;flash.rectTransform.anchorMax=Vector2.one;flash.rectTransform.offsetMin=flash.rectTransform.offsetMax=Vector2.zero;flash.sprite=(enemy as Image)?.sprite;flash.preserveAspect=true;flash.gameObject.SetActive(false);}
  Vector2 EnemyPoint()=>view.frame.InverseTransformPoint(enemy.rectTransform.TransformPoint(Vector2.zero));
  public void Cast(){if(!Configured)return;castAt=clock;hitAt=-100;target=EnemyPoint();p.Play(LightSample.HolyImpact,Vector2.zero,target);}
  public void Hit(){if(!Configured)return;hitAt=clock;MainHits++;var r=enemy.rectTransform;heldEnemy=r.anchoredPosition;heldScale=r.localScale;heldRotation=r.localRotation;}
  public void Prime(int id,int pattern){if(!Configured)return;int slot=miniCursor++%minis.Length;minis[slot]=new Mini{active=true,id=id,pattern=pattern,start=clock,contact=-100,target=EnemyPoint()};MiniCharges++;staffAt=clock;}
  public void Extra(int id,int pattern){if(!Configured)return;for(int i=0;i<minis.Length;i++)if(minis[i].active&&minis[i].id==id){var m=minis[i];m.hit=true;m.contact=clock;m.target=EnemyPoint();minis[i]=m;MiniHits++;return;}}
  void Restore(){if(held&&view!=null){var r=enemy.rectTransform;r.anchoredPosition=normalEnemy;r.localScale=normalScale;r.localRotation=normalRotation;}held=false;}
  public void Cancel(){Restore();clock=0;castAt=hitAt=staffAt=-100;MainHits=MiniHits=MiniCharges=miniCursor=0;for(int i=0;i<minis.Length;i++)minis[i].active=false;p?.Clear();if(flash!=null)flash.gameObject.SetActive(false);if(view!=null)view.SetFloatingCastingPose(0,Vector2.zero,0);if(p!=null)LegendaryScreenDimming.Set(this,Color.clear);}
  void OnDisable()=>Cancel();void OnDestroy(){if(flash!=null)Destroy(flash.gameObject);if(flashMaterial!=null)Destroy(flashMaterial);}
  static float Fade(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));
  Vector2 PortalPoint(int i){var o=portals[i].offset;float perspective=620/(620+o.z);return target+new Vector2(o.x,o.y)*perspective;}
  public void Step(BattleModel battle,float dt){if(!Configured)return;if(battle==null||battle.Result!=BattleResult.Fighting){Cancel();return;}held=false;clock+=Mathf.Max(0,dt);float age=clock-castAt,impact=clock-hitAt;float weight=Mathf.SmoothStep(0,1,Mathf.Clamp01(age/.16f))*Fade(age,skill.initialHitDelay+.04f,skill.initialHitDelay+.30f);if(castAt<0)weight=0;view.SetFloatingCastingPose(weight,new Vector2(56,79),-12+Mathf.Sin(age*2)*8);view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out var staff);LastStaffOrigin=staff;
   LivePortals=LiveBeams=0;p.BeginLayers(clock);if(castAt>=0&&age<skill.initialHitDelay+1.35f)Main(age,impact,staff);else LegendaryScreenDimming.Set(this,Color.clear);
   for(int i=0;i<minis.Length;i++){if(!minis[i].active)continue;var m=minis[i];if(!m.hit&&!battle.IsCelestialPending(m.id)){m.active=false;minis[i]=m;continue;}float t=m.hit?clock-m.contact:-1;if(m.hit&&t>.65f){m.active=false;minis[i]=m;continue;}MiniCircle(m,t);}
   float spark=clock-staffAt;if(spark<.18f){p.Star(staff,18,Fade(spark,.04f,.18f),2);for(int i=0;i<5;i++)p.Glyph(staff+D(i*1.25f)*(15+spark*110),0,Quaternion.identity,i,3,i,Fade(spark,.03f,.18f),2);}
   bool flashing=impact>=0&&impact<.075f;flash.gameObject.SetActive(flashing);if(flashing){flash.color=new Color(1,.98f,.85f,Fade(impact,.045f,.075f));if(impact<.055f){var r=enemy.rectTransform;normalEnemy=r.anchoredPosition;normalScale=r.localScale;normalRotation=r.localRotation;r.anchoredPosition=heldEnemy;r.localScale=heldScale;r.localRotation=heldRotation;held=true;}}
   MaxBeams=Mathf.Max(MaxBeams,LiveBeams);p.EndLayers();
  }
  void Main(float age,float impact,Vector2 staff){var s=p.style;bool fired=age>=FireAt;bool exploded=hitAt>=castAt;float fade=exploded?Fade(impact,.04f,.30f):1;float dim=exploded?0:fired?.08f*Fade(age-FireAt,0,.08f):Mathf.Lerp(.08f,.25f,Mathf.Clamp01(age/FireAt));LegendaryScreenDimming.Set(this,new Color(.035f,.025f,.08f,dim));
   if(age<FireAt){p.Circle(staff,43,new Vector3(22,-27,0),age*2,Fade(age,FireAt-.10f,FireAt+.1f),1,1);p.Star(staff,12+Mathf.Clamp01(age/.3f)*19,1,2);}
   float deploySpacing=Mathf.Max(.025f,(FireAt-.32f)/(portals.Length-1));
   for(int i=0;i<portals.Length;i++){var def=portals[i];float born=.04f+i*deploySpacing,t=age-born;if(t<0)continue;LivePortals++;var center=PortalPoint(i);float scale=620/(620+def.offset.z);float radius=def.radius*scale;float freezeAt=FireAt-.07f;float artTime=(Mathf.Min(age,freezeAt)-born)*1.7f+i*.012f;float lockOn=Mathf.InverseLerp(FireAt-.25f,FireAt-.08f,age);var tilt=def.tilt+new Vector3(4*(1-lockOn),-6*(1-lockOn),0);float alpha=(def.depth==0?.79f:1)*fade;
    if(t<.23f){float travel=Mathf.Clamp01(t/.18f);var q=Vector2.Lerp(staff,center,travel);p.Line(Vector2.Lerp(staff,center,Mathf.Max(0,travel-.16f)),q,2,A(s.pale,Fade(t,.13f,.23f)),2);p.Glyph(q,0,Quaternion.Euler(0,0,t*150),i,5,i,Fade(t,.12f,.23f),2);}
    if(!exploded||impact<.24f)p.Circle(center,radius,tilt,artTime,alpha,2,def.depth);
    if(!fired){p.Line(center,target,1,A(s.pale,lockOn*.36f),def.depth);p.Disc(center,18+lockOn*14,Quaternion.Euler(tilt),s.core,lockOn*.65f,2);}else if(!exploded||impact<.12f)Laser(center,target,age-FireAt,(def.depth==0?17:26)*scale,(exploded?Fade(impact,0,.12f):1));
    if(exploded)BreakCircle(center,radius,tilt,impact,i);
   }
   if(fired&&!exploded){float k=Mathf.Clamp01((age-FireAt)/Mathf.Max(.05f,skill.initialHitDelay-FireAt));p.Disc(target,45+56*k,Quaternion.identity,s.gold,.7f,1);p.Disc(target,32+40*k,Quaternion.identity,s.ivory,1,2);p.Disc(target,22+28*k,Quaternion.identity,s.core,1,2);p.Ring(target,110-25*k,Quaternion.Euler(47,-8,0),age*2,4,s.pale,.9f,1);for(int i=0;i<20;i++){float a=i*2.4f;var q=target+D(a)*(95-35*k);p.Line(q+D(a)*18,q,2,A(i%4==0?s.lavender:s.ivory,.8f),2);}}
   if(exploded){float t=Mathf.Max(0,impact-.055f);p.Impact(target,t,2.65f);p.Impulse(impact,13,.18f);p.Disc(target,290,Quaternion.identity,s.ivory,Fade(impact,.04f,.18f)*.52f,2);for(int i=0;i<22;i++){float a=i*Mathf.PI/11;var dir=D(a);var n=D(a+1.57f);p.Quad(target+dir*45,target+dir*110-n*12,target+dir*(180+t*450),target+dir*110+n*12,A(i%3==0?s.ivory:s.pale,Fade(t,.07f,.33f)),i%2);}}
  }
  void Laser(Vector2 from,Vector2 to,float age,float width,float fade){if(age<0||fade<=0)return;LiveBeams++;var s=p.style;var dir=(to-from).normalized;var normal=new Vector2(-dir.y,dir.x);var end=Vector2.Lerp(from,to,Mathf.Clamp01((age+.01f)/.035f));for(int l=0;l<5;l++){float w=width*(1-l*.19f);var c=l==0?s.gold:l==1?s.pale:l==2?s.ivory:s.core;for(int j=0;j<8;j++){float u=j/8f,v=(j+1)/8f;var a=Vector2.Lerp(from,end,u);var b=Vector2.Lerp(from,end,v);float wa=w*(1-.4f*u),wb=w*(1-.4f*v);p.Quad(a-normal*wa,a+normal*wa,b+normal*wb,b-normal*wb,A(c,fade*(l==0?.5f:.92f)),l==4?2:1);}}p.Line(from+normal*width*.85f,end+normal*width*.5f,1.5f,A(s.lavender,fade*.7f),2);p.Line(from-normal*width*.85f,end-normal*width*.5f,1.5f,A(s.cyan,fade*.55f),2);for(int j=0;j<7;j++){float k=Mathf.Repeat(age*5+j*.14f,1);var q=Vector2.Lerp(from,end,k)+normal*((j%2==0?-1:1)*(width+7));p.Line(q-dir*16,q,2,A(s.ivory,fade*.7f),2);}}
  void BreakCircle(Vector2 center,float radius,Vector3 tilt,float t,int seed){if(t<0||t>.70f)return;var s=p.style;float fade=Fade(t,.08f,.70f);for(int j=0;j<8;j++){float a=j*Mathf.PI/4+seed*.3f;var offset=D(a)*t*(65+seed*8);var q=Quaternion.Euler(tilt+new Vector3(t*35,t*22,t*(j%2==0?80:-80)));p.Ring(center+offset,radius*(1+t*.15f),q,a,3,j%3==0?s.ivory:s.gold,fade,seed%2,64,.055f);var shard=center+D(a)*(radius*.7f+t*180);p.Glyph(shard,0,q,a,5+t*3,j,fade,seed%2);}}
  static Vector2 MiniOffset(int pattern)=>pattern==0?new Vector2(-140,18):pattern==1?new Vector2(145,12):pattern==2?new Vector2(0,143):pattern==3?new Vector2(-116,106):new Vector2(116,106);
  void MiniCircle(Mini m,float impact){var s=p.style;float t=clock-m.start;var center=m.target+MiniOffset(m.pattern);var tilt=new Vector3(m.pattern==2?62:25,m.pattern%2==0?42:-42,m.pattern*13);var q=Quaternion.Euler(tilt);float fade=m.hit?Fade(impact,.10f,.35f):Mathf.Clamp01((t+.035f)/.08f);float radius=42*(.55f+.45f*Mathf.Clamp01(t/.07f));p.Ring(center,radius,q,t,3,s.gold,fade,1,48);p.Ring(center,radius*.77f,q,-t,2,s.ivory,fade,1,40);for(int i=0;i<6;i++)p.Glyph(center,radius*.87f,q,i*Mathf.PI/3-t,3,i,fade,1);p.Star(center,13,fade,2);
   if(m.hit){Laser(center,m.target,impact,10,Fade(impact,.05f,.17f));p.Star(m.target,33,Fade(impact,.025f,.13f),2);p.Ring(m.target,15+impact*190,Quaternion.Euler(52,0,0),0,3,s.pale,Fade(impact,.08f,.36f),1,40);for(int i=0;i<9;i++){var pos=m.target+D(i*2.4f)*(15+impact*140);p.Star(pos,3,Fade(impact,.1f,.47f),2);}for(int i=0;i<4;i++)p.Glyph(center+D(i*1.57f)*(radius+impact*80),0,q,i,3,i,Fade(impact,.03f,.3f),1);}
  }
 }
}


