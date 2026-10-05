using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Weather is presentation only. All contacts are supplied by real Skill damage cues.
 public sealed class CatastropheVfx:MonoBehaviour {
  BattleView view;LightningStylePrototype paint;SkillData skill;Graphic enemy;
  LightningMeshLayer light;BattleVfxHudVisibility lightMaterials;
  readonly float[] contacts=new float[64];float age=100,last=-100;Vector2 origin,target,frozen;bool holding;
  public bool Configured{get;private set;}public bool DamageFromSkill{get;set;}public int ScheduledHits{get;set;}public int HitsShown{get;private set;}
  public bool FinalHit=>HitsShown>0&&HitsShown%Mathf.Max(1,skill.hitCount)==0;
  public bool Clean=>(paint==null||paint.Clean)&&(light==null||light.Count==0);public int PeakQuads=>paint==null?0:paint.PeakQuads;public int Dropped=>paint==null?0:paint.DroppedLayers;
  public bool WeatherActive=>Configured&&age<End+.95f;
  float End {get {if(skill==null)return 0;int n=Mathf.Max(skill.hitCount,ScheduledHits)-1,c=Mathf.Max(1,skill.hitCount);return (n/c)*(skill.hitTimeOffsets[c-1]+skill.hitInterval)+skill.hitTimeOffsets[n%c];}}
  static readonly Color Navy=new Color(.025f,.045f,.11f),Cloud=new Color(.12f,.20f,.32f),Blue=new Color(.20f,.44f,.69f),Cyan=new Color(.40f,.88f,1),Pale=new Color(.78f,.96f,1);
  public void Initialize(BattleView owner){view=owner;}
  public void Configure(SkillData data,Graphic enemyGraphic){Cancel();skill=data;enemy=enemyGraphic;Configured=data!=null&&data.StableId=="skill_catastrophe";if(Configured&&paint==null){paint=gameObject.AddComponent<LightningStylePrototype>();paint.style=Resources.Load<LightningSkillLibrary>("LightningSkills/Library").style;paint.Initialize(view);
   lightMaterials=new BattleVfxHudVisibility();var go=new GameObject("Catastrophe emissive highlights",typeof(RectTransform),typeof(Canvas),typeof(CanvasRenderer),typeof(LightningMeshLayer));var rect=go.GetComponent<RectTransform>();rect.SetParent(view.frame,false);rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;rect.sizeDelta=new Vector2(480,850);var canvas=go.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=32701;light=go.GetComponent<LightningMeshLayer>();light.raycastTarget=false;light.material=lightMaterials.additive;}}
  public void Cast(){if(!Configured)return;Cancel();age=0;view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out origin);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));paint.Play(LightningSample.HeavyThunderImpact,origin,target);}
  public void Hit(){if(!Configured)return;if(age>=100)Cast();contacts[HitsShown%contacts.Length]=age;HitsShown++;last=age;if(FinalHit){frozen=enemy.rectTransform.anchoredPosition;holding=true;}}
  public void Cancel(){age=100;last=-100;HitsShown=0;holding=false;paint?.Clear();if(light!=null){light.Begin();light.End();}if(Configured||paint!=null)LegendaryScreenDimming.Set(this,Color.clear);}
  void OnDisable()=>Cancel();
  void OnDestroy(){Cancel();lightMaterials?.Dispose();if(light!=null)Destroy(light.gameObject);}
  static float F(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));
  static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}
  static float R(int n){uint x=(uint)(n*747796405+2891336453u);x=((x>>((int)(x>>28)+4))^x)*277803737u;return ((x>>22)^x)/4294967295f;}
  public void Step(BattleModel b,float dt){if(!Configured)return;if(b==null||b.Result!=BattleResult.Fighting){Cancel();return;}age+=Mathf.Max(0,dt);paint.BeginLayers(age);lightMaterials.Update(view);light.Begin();int seed=(int)(age*24);float end=End;
   if(WeatherActive){float fade=Mathf.Min(Mathf.Clamp01(age/.18f),F(age,end+.15f,end+.95f));float compression=Mathf.Clamp01((age-(end-.85f))/.60f);bool silence=age>end-.14f&&age<end;
    LegendaryScreenDimming.Set(this,new Color(.018f,.04f,.11f,.32f*fade));Weather(fade,silence,compression);Summon(fade,seed);
    // Left, right and distant upper approaches. Each winds through the target, then joins the final vortex.
    int count=Mathf.Max(skill.hitCount,ScheduledHits),cycles=Mathf.CeilToInt(count/(float)skill.hitCount);
    for(int cycle=0;cycle<cycles;cycle++){float offset=cycle*(skill.hitTimeOffsets[skill.hitCount-1]+skill.hitInterval);for(int i=0;i<3;i++){float contact=offset+skill.hitTimeOffsets[i*2],t=age-contact;if(t<-.65f||t>1.18f)continue;float k=Mathf.Clamp01((t+.65f)/.65f);Vector2 start=target+new Vector2(i==0?-340:i==1?350:100,i==2?235:-90);Vector2 p;
      if(t<=0)p=Vector2.Lerp(start,target,Mathf.SmoothStep(0,1,k));else {float orbit=t*4+i*2.1f;float radius=135*F(age,end-.75f,end-.15f);p=target+new Vector2(Mathf.Sin(orbit)*radius,Mathf.Cos(orbit)*radius*.28f);}
      float size=(i==2?Mathf.Lerp(.36f,1.10f,k):i==0?1.02f:1.28f);float alpha=Mathf.Min(Mathf.Clamp01(k*3),F(t,.73f,1.18f))*fade;Hurricane(p,size,alpha,age*(i==1?-1:1),i,false,seed);
    }}
    if(compression>0&&age<end){float radius=Mathf.Lerp(225,85,compression);for(int j=0;j<4;j++){float a=age*10+j*1.57f;var p=target+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.55f);Ribbon(p,target,Cloud,7,fade,0);paint.Bolt(p,target,.45f,fade*.65f,seed+j,0,true);}Hurricane(target,.95f-compression*.18f,fade,silence?end-.14f:age,5,true,seed);Ellipse(target,85*(1-compression*.25f),62,A(Navy,.9f),1);if(silence)paint.Bolt(new Vector2(0,450),target+Vector2.up*185,1.1f,.7f,seed,0,true);}
    for(int i=0;i<Mathf.Min(HitsShown,contacts.Length);i++){float t=age-contacts[i];if(t<0||t>1)continue;bool final=(i+1)%skill.hitCount==0;bool thunder=i%skill.hitCount%2==1||final;float local=final?Mathf.Max(0,t-.065f):t;
     if(final){if(t<.055f)LegendaryScreenDimming.Set(this,new Color(.77f,.94f,1,.25f*F(t,.02f,.055f)));Hurricane(target,Mathf.Lerp(.8f,1.80f,Mathf.Clamp01(local/.16f)),F(local,.18f,.65f),end+local,8,true,seed);if(t<.23f){Strike(new Vector2(-40,475),target,4.5f,F(t,.11f,.23f),t<.065f?311:seed,1,true);Strike(new Vector2(220,390),target,1.7f,F(t,.10f,.23f),seed+3,2,true);Strike(new Vector2(-220,415),target,1.6f,F(t,.10f,.23f),seed+5,0,true);}paint.Impact(target,local,2.9f,37,true);LightBurst(target,local,1.8f);Ring(target,(55+local*640),.46f,9,A(Pale,F(local,.10f,.50f)),2);Debris(target,local,1.8f,60);paint.Impulse(t,17,.22f);}
     else if(thunder){if(t<.19f){float side=i%3==0?-200:i%3==1?190:0;Strike(target+new Vector2(side,230),target,1.9f,F(t,.07f,.19f),seed+i*37,1,true);}paint.Impact(target,t,1.3f,i*31,true);LightBurst(target,t,.8f);paint.Impulse(t,5,.10f);}
     else {Ring(target,(35+t*330),.45f,8,A(Cyan,F(t,.07f,.30f)),1);paint.Impact(target,t,.80f,i*31,false);Debris(target,t,1,28);paint.Impulse(t,4,.10f);}
    }
   }else LegendaryScreenDimming.Set(this,Color.clear);
   // Reuses the art-direction's residual arcs; lifetime is the real existing status, never a VFX timer.
   if(b.EnemyParalyzed)paint.Residual(target,.22f,1,seed);
   paint.EndLayers();light.End();
   float since=age-last;if(since<.075f&&HitsShown>0)enemy.color=Color.Lerp(enemy.color,Pale,F(since,.025f,.075f));
   // VFX/target pose hold only: no global timeScale, input lock or cooldown pause.
   if(holding&&since<.065f)enemy.rectTransform.anchoredPosition=frozen;else holding=false;
  }
  void Weather(float alpha,bool silence,float compression){float power=silence?.20f:1;for(int layer=0;layer<3;layer++){int count=layer==0?65:layer==1?90:32;float speed=layer==0?390:layer==1?680:1150;for(int i=0;i<count;i++){float y=450-Mathf.Repeat(R(i*17+layer*89)*900+age*speed,900),x=-300+Mathf.Repeat(R(i*71+layer*131)*640-age*speed*.32f,640);var p=new Vector2(x,y);float length=(layer==0?13:layer==1?24:48)*(0.65f+R(i+19));var dir=new Vector2(-.34f,-1).normalized;dir=Vector2.Lerp(dir,(target-p).normalized,compression*.45f);paint.Line(p,p-dir*length,layer==2?2.4f:1.4f,A(layer==0?Blue:layer==1?Cyan:Pale,alpha*power*(layer==2?.45f:.26f)),layer);}}
   for(int band=0;band<2;band++)for(int i=0;i<9;i++){float x=-315+Mathf.Repeat(i*81+age*(band==0?50:-38),630);float y=band==0?360+Mathf.Sin(i*1.4f+age)*18:-280+Mathf.Sin(i*1.1f+age)*20;Ellipse(new Vector2(x,y),105,32,A(Navy,alpha*.40f),0);Ellipse(new Vector2(x-9,y+13),88,22,A(Cloud,alpha*.17f),0);}
  }
  void Summon(float alpha,int seed){if(age>.75f)return;float k=age/.75f;view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,view.frame,out var center);for(int j=0;j<3;j++){Ring(center,65+k*160+j*12,.35f,5-j,A(Cyan,alpha*F(k,.4f,1)),j==0?0:2);var a=age*12+j*2.1f;var p=center+new Vector2(Mathf.Cos(a)*100,Mathf.Sin(a)*40);paint.Bolt(p,p+Vector2.up*45,.55f,F(k,.6f,1),seed+j,2,true);if(k>.25f)Ribbon(p,Vector2.Lerp(p,target,(k-.25f)/.75f),Cyan,3,F(k,.5f,1),0);}}
  // A turbulent cloud volume, not continuous coils. The column extends beyond the screen.
  void Hurricane(Vector2 p,float scale,float alpha,float time,int identity,bool large,int seed){
   if(alpha<=0)return;float w=115*scale;Vector2 floor=p-Vector2.up*(90*Mathf.Min(scale,1.2f));float h=Mathf.Max(300,view.frame.rect.yMax+120-floor.y);
   float pulse=Mathf.Pow(Mathf.Max(0,Mathf.Sin(time*17+identity*3.7f)),18);float contactLight=HitsShown>0?F(age-last,.025f,.13f):0;float illumination=Mathf.Max(pulse*.6f,contactLight);
   // Overlapping, uneven shadow masses make one broad storm body with a dark interior.
   for(int i=0;i<20;i++){float u=(i+.5f)/20;var c=floor+new Vector2(Mathf.Sin(u*11+time*2+identity)*w*.06f,u*h);CloudMass(c,w*(.85f+R(i+identity*31)*.15f),h*.075f,0,A(Navy,alpha*.46f),0,i+identity*23);}
   // Billows orbit at different radii/speeds. Foreground faces are broad, softer, and partially transparent.
   for(int depth=0;depth<2;depth++)for(int i=0;i<104;i++){
    int key=i+identity*139;float u=Mathf.Repeat(R(key*17)+time*(.018f+R(key+9)*.025f),1);float theta=R(key*31)*6.283f+time*(2.5f+R(key+18)*3.5f)+u*2;
    float z=Mathf.Sin(theta);if((z>0?1:0)!=depth)continue;float radius=w*(.73f+R(key+32)*.30f);var c=floor+new Vector2(Mathf.Cos(theta)*radius,u*h+z*w*.15f);
    float rx=(34+R(key+13)*46)*scale*(.55f+Mathf.Abs(z)*.65f),ry=(18+R(key+63)*23)*scale;float tilt=(.13f+R(key+3)*.35f)*Mathf.Cos(theta);
    Color face=Color.Lerp(Cloud,new Color(.48f,.62f,.72f),Mathf.Max(0,z)*(.60f+R(key)*.35f));face=Color.Lerp(face,Cyan,illumination*Mathf.Max(0,z)*.40f);
    float opacity=alpha*(depth==0?.35f:.46f+Mathf.Max(0,z)*.28f);CloudMass(c,rx,ry,tilt,A(face,opacity),depth==0?0:2,key);
    if(z>.2f){CloudMass(c+new Vector2(-rx*.15f,ry*.42f),rx*.80f,ry*.36f,tilt,A(Color.Lerp(new Color(.36f,.52f,.65f),Pale,illumination*.70f),opacity*.60f),2,key+41);}
   }
   // Short torn pressure sheets: no complete rings, no constant pitch, no permanently white outline.
   for(int i=0;i<23;i++){
    int key=i+identity*53;float u=Mathf.Repeat(R(key*23)+time*(.025f+R(key+1)*.03f),1),theta=R(key*37)*6.283f+time*(3.5f+R(key+5)*3);
    float sweep=.38f+R(key+8)*1.0f,r=w*(.83f+R(key+9)*.20f),thick=(12+R(key+10)*24)*scale;int segments=14;
    for(int j=0;j<segments;j++){float t=j/(float)segments,v=(j+1)/(float)segments,aa=theta+t*sweep,bb=theta+v*sweep;float z=Mathf.Sin((aa+bb)*.5f);float taper=Mathf.Pow(Mathf.Sin((t+.5f/segments)*Mathf.PI),.6f);float shear=(t-.5f)*(24+R(key+14)*42)*scale;
     var q=floor+new Vector2(Mathf.Cos(aa)*r,u*h+Mathf.Sin(aa)*r*.20f+shear);var e=floor+new Vector2(Mathf.Cos(bb)*r,u*h+Mathf.Sin(bb)*r*.20f+shear+sweep*2);
     float tear=.65f+R(key*29+j)*.35f;var offset=Vector2.up*(thick*taper*tear);Color color=Color.Lerp(new Color(.30f,.48f,.60f),Cyan,illumination*.65f);float opacity=alpha*(z>0?.44f:.19f)*taper;
     paint.Quad(q-offset*.55f,q+offset*.45f,e+offset*.30f,e-offset*.50f,A(color,opacity),z>0?2:0);
     if(z>.45f&&illumination>.1f&&j%4!=0)light.Line(q+offset*.25f,e+offset*.25f,1.5f,A(Pale,alpha*illumination*.26f*taper));
    }
   }
   // Intermittent internal discharges light nearby cloud faces instead of outlining the whole cylinder.
   if(illumination>.12f)for(int i=0;i<2;i++){float a=time*5+i*2.1f;var from=floor+new Vector2(Mathf.Cos(a)*w*.50f,(.25f+i*.40f)*h);var to=from+new Vector2(Mathf.Sin(a+1)*w*.6f,60*scale);paint.Bolt(from,to,(large?.70f:.45f)*scale,alpha*illumination,seed+identity*41+i,1,true);}
   // Ground spray travels sideways and upward; scattered wisps replace the former solid base ring.
   for(int i=0;i<22;i++){int key=i+identity*71;float a=time*(3+R(key)*3)+i*2.39996f;float r=w*(.85f+R(key+17)*.50f);var c=floor+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*.14f+R(key+6)*16);CloudMass(c,(28+R(key+18)*23)*scale,(6+R(key+21)*9)*scale,.12f,A(Color.Lerp(Cloud,new Color(.44f,.59f,.68f),.70f),alpha*.40f),Mathf.Sin(a)>0?2:0,key);if(i%2==0){var d=new Vector2(-Mathf.Sin(a),.4f);paint.Line(c,c+d*18*scale,2,A(Pale,alpha*.22f),2);}}
  }
  // Jagged, elongated billows retain pixel edges but never read as repeated circular beads.
  void CloudMass(Vector2 center,float rx,float ry,float tilt,Color color,int depth,int seed){
   const int n=16;for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;float ra=.78f+R(seed*41+i)*.22f,rb=.78f+R(seed*41+(i+1)%n)*.22f;var q=new Vector2(Mathf.Cos(a)*rx*ra,Mathf.Sin(a)*ry*ra);var r=new Vector2(Mathf.Cos(b)*rx*rb,Mathf.Sin(b)*ry*rb);q.y+=q.x*tilt;r.y+=r.x*tilt;paint.Quad(center,center+q,center+r,center,color,depth);}
  }
  // Only luminous edges pass above the full-screen grade. The HUD and scene still darken together.
  void Strike(Vector2 from,Vector2 to,float scale,float alpha,int seed,int depth,bool branches){paint.Bolt(from,to,scale,alpha,seed,depth,branches);LitPath(from,to,scale,alpha,seed,branches);}
  void LitPath(Vector2 from,Vector2 to,float scale,float alpha,int seed,bool branches){var delta=to-from;var normal=new Vector2(-delta.y,delta.x).normalized;var prev=from;for(int i=1;i<=12;i++){float k=i/12f;var p=Vector2.Lerp(from,to,k)+normal*((R(seed*31+i*17)-.5f)*44*scale*Mathf.Sin(k*Mathf.PI));light.Line(prev,p,18*scale,A(Blue,alpha*.10f));light.Line(prev,p,6*scale,A(Cyan,alpha*.28f));light.Line(prev,p,1.7f*scale,A(Color.white,alpha));if(branches&&i>2&&i<11&&i%3==0){var end=p+normal*((i%2==0?1:-1)*(22+R(i+seed)*44))*scale+delta.normalized*28*scale;LitPath(p,end,scale*.32f,alpha*.85f,seed+i,false);}prev=p;}}
  void LightBurst(Vector2 p,float t,float size){float a=F(t,.025f,.15f);if(a<=0)return;light.Diamond(p,25*size,45*size,A(Cyan,a*.55f));light.Diamond(p,13*size,24*size,A(Color.white,a));for(int i=0;i<16;i++){float angle=i*2.39996f;var d=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)*.72f);light.Line(p+d*14*size,p+d*(42+t*240)*size,3*size,A(Pale,a*.8f));}}
  void Ellipse(Vector2 p,float x,float y,Color c,int depth){for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;paint.Quad(p,p+new Vector2(Mathf.Cos(a)*x,Mathf.Sin(a)*y),p+new Vector2(Mathf.Cos(b)*x,Mathf.Sin(b)*y),p,c,depth);}}
  void Ring(Vector2 p,float r,float aspect,float width,Color c,int depth){for(int i=0;i<48;i++){float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;paint.Line(p+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*aspect),p+new Vector2(Mathf.Cos(b)*r,Mathf.Sin(b)*r*aspect),width,c,depth);}}
  void Ribbon(Vector2 a,Vector2 b,Color c,float width,float alpha,int depth){var last=a;for(int i=1;i<=16;i++){float t=i/16f;var p=Vector2.Lerp(a,b,t)+new Vector2(Mathf.Sin(t*Mathf.PI)*35,Mathf.Sin(t*Mathf.PI*2)*16);paint.Line(last,p,width,A(c,alpha),depth);last=p;}}
  void Debris(Vector2 p,float t,float size,int count){if(t>.55f)return;for(int i=0;i<count;i++){float a=i*2.39996f;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.65f);var q=p+d*(60+t*(250+R(i)*400))*size;paint.Line(q,q-d*(8+R(i+7)*22)*size,2+R(i+19)*3,A(i%3==0?Pale:Cyan,F(t,.14f,.5f)),i%3);}}
 }
}
