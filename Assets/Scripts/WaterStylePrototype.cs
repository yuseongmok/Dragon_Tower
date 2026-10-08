using UnityEngine;
namespace DragonTower {
 // Isolated, pooled geometry study. No damage, status, skill or character mutations.
 public sealed class WaterStylePrototype:MonoBehaviour {
  public WaterVfxStyle style; LightningMeshLayer[] layers;BattleVfxHudVisibility materials;BattleView view;Vector2 source,target;float age=10;WaterSample sample;
  public int ActiveCount{get;private set;}public int PeakQuads{get;private set;}public int DroppedLayers{get;private set;}public bool Active=>age<style.duration;public bool Clean=>ActiveCount==0;
  static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));
  static float R(int i)=>Mathf.Repeat(Mathf.Sin(i*127.1f+31.7f)*43758.5453f,1);
  static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}
  static float Fade(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));
  public void Initialize(BattleView owner){if(layers!=null)return;view=owner;materials=new BattleVfxHudVisibility();layers=new LightningMeshLayer[3];for(int i=0;i<3;i++){var g=new GameObject("Water depth "+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=g.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=new Vector2(480,850);layers[i]=g.GetComponent<LightningMeshLayer>();layers[i].raycastTarget=false;layers[i].material=materials.alpha;if(i==0)r.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());}BattleVfxHudVisibility.BringHudForward(view);}
  public void Play(WaterSample kind,Vector2 origin,Vector2 hit){Clear();sample=kind;source=origin;target=hit;age=0;PeakQuads=DroppedLayers=0;}
  public void Clear(){Restore();age=10;ActiveCount=0;if(layers!=null)foreach(var l in layers)if(l!=null){l.Begin();l.End();}}
  void OnDisable()=>Clear();void OnDestroy(){Clear();materials?.Dispose();if(layers!=null)foreach(var l in layers)if(l!=null)Destroy(l.gameObject);}
  RectTransform arena;Vector2 arenaHome;bool shifted;
  void Restore(){if(shifted&&arena!=null)arena.anchoredPosition=arenaHome;shifted=false;if(layers!=null)foreach(var l in layers)if(l!=null)l.rectTransform.anchoredPosition=Vector2.zero;}
  public void BeginLayers(float time){Restore();age=time;materials.Update(view);foreach(var l in layers)l.Begin();}
  public void EndLayers(){ActiveCount=0;foreach(var l in layers){ActiveCount+=l.Count;DroppedLayers+=l.Overflow;l.End();}PeakQuads=Mathf.Max(PeakQuads,ActiveCount);}
  public void Impulse(float t,float strength,float duration){if(t<0||t>=duration)return;if(arena==null)arena=(RectTransform)view.frame.Find("Arena");if(arena==null)return;Restore();float k=1-t/duration;var offset=new Vector2(Mathf.Sin(t*165)*strength,Mathf.Cos(t*139)*strength*.65f)*k;arenaHome=arena.anchoredPosition;arena.anchoredPosition+=offset;shifted=true;foreach(var l in layers)l.rectTransform.anchoredPosition=offset;}
  public void Line(Vector2 a,Vector2 b,float width,Color color,int depth=1)=>layers[depth].Line(a,b,width,color);
  public void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color,int depth=1)=>layers[depth].Quad(a,b,c,d,color);
  Vector2 E(Vector2 p,float r,float a,float tilt)=>p+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*tilt);
  public void Disc(Vector2 p,float r,float tilt,Color c,float alpha,int depth,int segments=48){for(int i=0;i<segments;i++){float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;layers[depth].Quad(p,E(p,r,a,tilt),E(p,r,b,tilt),p,A(c,alpha));}}
  // Open, broken ribbons have their own phase and width; they are not tinted copies.
  public void Flow(Vector2 p,float radius,float tilt,float t,float alpha,int depth,int seed,float width,float span){for(int i=0;i<40;i++){float u=i/40f,v=(i+1)/40f;float a=t+seed*2.39996f+u*span,b=t+seed*2.39996f+v*span;float ra=radius*(1+.08f*Mathf.Sin(a*3+t*1.7f+seed)),rb=radius*(1+.08f*Mathf.Sin(b*3+t*1.7f+seed));float w=width*Mathf.Sin(u*Mathf.PI);layers[depth].Quad(E(p,ra-w,a,tilt),E(p,ra+w,a,tilt),E(p,rb+w,b,tilt),E(p,rb-w,b,tilt),A(seed%3==0?style.aqua:seed%3==1?style.cyan:style.pale,alpha));if(i%4==0)layers[depth].Line(E(p,ra+w,a,tilt),E(p,rb+w,b,tilt),2,A(style.foam,alpha*.8f));}}
  public void Bubble(Vector2 p,float r,float alpha,int depth){for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;layers[depth].Line(E(p,r,a,1),E(p,r,b,1),1.6f,A(i<6?style.pale:style.azure,alpha));}layers[depth].Diamond(p+new Vector2(-r*.3f,r*.4f),1.5f,2,A(style.foam,alpha));}
  public void Droplets(Vector2 p,float t,float scale,float alpha,int count){for(int i=0;i<count;i++){float life=.55f+R(i+7)*.75f;if(t<0||t>life)continue;float a=i*2.39996f;var velocity=D(a)*(70+R(i)*160)*scale;var q=p+velocity*t+Vector2.down*(90*t*t);float size=(2+R(i+3)*5)*scale,fade=alpha*Fade(t,life*.65f,life);int depth=i%4==0?0:2;layers[depth].Line(q-velocity.normalized*size*1.6f,q,size*1.2f,A(style.azure,fade));Disc(q,size,.9f,style.azure,fade,depth,12);Disc(q+Vector2.up*size*.2f,size*.7f,.85f,style.aqua,fade,depth,12);layers[depth].Line(q+new Vector2(-size*.25f,0),q+new Vector2(-size*.25f,size),1.6f,A(style.foam,fade));}}
  public void Mass(Vector2 p,float r,float t,float alpha,int depth=1){for(int band=8;band>=1;band--){float k=band/8f;var c=band>6?style.deep:band>4?style.azure:style.cyan;for(int i=0;i<56;i++){float a=i*Mathf.PI/28,b=(i+1)*Mathf.PI/28;float ra=r*k*(1+.08f*Mathf.Sin(a*3+t*4)+.035f*Mathf.Cos(a*7-t*6)),rb=r*k*(1+.08f*Mathf.Sin(b*3+t*4)+.035f*Mathf.Cos(b*7-t*6));layers[depth].Quad(p+new Vector2(-r*.12f,r*.12f),E(p,ra,a,.86f),E(p,rb,b,.86f),p,A(c,alpha*(band==8?.65f:.82f)));}}
   Disc(p+new Vector2(-r*.27f,r*.31f),r*.27f,.46f,style.aqua,alpha*.45f,depth);Disc(p+new Vector2(-r*.35f,r*.36f),r*.15f,.36f,style.pale,alpha*.65f,depth);
   for(int i=0;i<6;i++)Flow(p+new Vector2(Mathf.Sin(t*2+i)*r*.08f,Mathf.Cos(t*3+i)*r*.07f),r*(.35f+i*.11f),.65f+.12f*Mathf.Sin(i),t*(i%2==0?2.1f:-1.4f),alpha*(i<3?.7f:.95f),depth,i,3+i*.9f,1.8f);
   for(int i=0;i<9;i++){float u=Mathf.Repeat(t*.4f+i*.113f,1);Bubble(p+new Vector2((R(i)-.5f)*r*1.4f,(u-.5f)*r*1.5f),2+R(i+9)*4,alpha*Mathf.Sin(u*Mathf.PI),Mathf.Min(2,depth+1));}}
  public void Whirlpool(Vector2 p,float r,float t,float alpha){float open=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.35f));r*=open;
   for(int band=10;band>=1;band--){float k=band/10f;Disc(p+Vector2.down*(1-k)*25,r*k,.34f,band<4?style.deep:band<7?style.azure:style.cyan,alpha*(band==10?.4f:.86f),0);}
   for(int arm=0;arm<6;arm++)for(int j=0;j<46;j++){float u=j/46f,v=(j+1)/46f;float a=arm*Mathf.PI/3+t*(1.5f+arm*.06f)+u*5.8f,b=arm*Mathf.PI/3+t*(1.5f+arm*.06f)+v*5.8f;float ra=r*(1-u*.84f),rb=r*(1-v*.84f);var x=E(p,ra,a,.34f)-Vector2.up*u*23;var y=E(p,rb,b,.34f)-Vector2.up*v*23;layers[0].Line(x,y,(3+7*(1-u))*open,A(arm%2==0?style.aqua:style.azure,alpha));if(j%5<2)layers[0].Line(x+Vector2.up*2,y+Vector2.up*2,2,A(style.pale,alpha*.85f));}
   for(int i=0;i<44;i++){float a=i*Mathf.PI/22-t*.8f;var q=E(p,r*(1+.025f*Mathf.Sin(i+t*4)),a,.34f);int depth=Mathf.Sin(a)<0?2:0;layers[depth].Diamond(q,4+R(i)*5,2+R(i+9)*3,A(style.foam,alpha));}
   for(int i=0;i<12;i++){float u=Mathf.Repeat(t*.6f+i*.083f,1);Bubble(E(p,r*(.9f-u*.7f),i*2.4f+t,.34f)+Vector2.up*u*32,2+R(i)*4,alpha*Mathf.Sin(u*Mathf.PI),2);}}
  public void Impact(Vector2 p,float t,float scale){if(t<0||t>1.7f)return;float expand=1-Mathf.Pow(1-Mathf.Clamp01(t/.26f),3);float fade=Fade(t,.18f,.72f);
   Mass(p,scale*(14+expand*65),t*2,fade*.8f);
   for(int i=0;i<13;i++){float a=i*2.39996f;Vector2 dir=D(a),n=D(a+1.57f);float reach=scale*(25+expand*(60+R(i)*70));float w=scale*(7+R(i+4)*10)*fade;int depth=i%4==0?0:2;
    Vector2 Last=Vector2.zero;for(int j=0;j<18;j++){float u=j/18f,v=(j+1)/18f;Vector2 P(float k)=>p+dir*(22+k*reach)+n*(Mathf.Sin(k*3.8f+i*.7f)*reach*.24f*k)+Vector2.down*t*t*35*k;var x=P(u);var y=P(v);float wa=w*(1-u*.72f),wb=w*(1-v*.72f);layers[depth].Quad(x-n*wa,x+n*wa,y+n*wb,y-n*wb,A(style.azure,fade*.78f));layers[depth].Quad(x-n*wa*.25f,x+n*wa*.68f,y+n*wb*.68f,y-n*wb*.25f,A(style.cyan,fade*.85f));if(j>6)layers[depth].Line(x+n*wa*.65f,y+n*wb*.65f,3,A(style.pale,fade));Last=y;}
    Disc(Last,w*.32f,1,style.aqua,fade,depth);Disc(Last+Vector2.up*2,w*.17f,1,style.foam,fade,depth);
   }
   for(int i=0;i<5;i++)Flow(p+Vector2.down*15,scale*(35+expand*(105+i*4)),.38f,t*(i%2==0?1:-1),Fade(t,.25f,.85f),2,i,4-i*.5f,2.7f);
   Droplets(p,t,scale,1,48);for(int i=0;i<14;i++){float k=t-.2f-i*.014f;if(k>0&&k<1.3f)Bubble(p+new Vector2((R(i)-.5f)*190*scale,k*65-20),3+R(i+9)*7,Fade(k,.5f,1.3f)*.6f,0);}
   for(int i=0;i<3;i++)Flow(p+Vector2.down*40,scale*(60+i*15),.22f,t*.6f,Fade(t,.6f,1.6f)*.25f,0,i,2,4);
  }
  public void Step(BattleModel model,float dt){if(layers==null)return;if(model==null||model.Result!=BattleResult.Fighting){Clear();return;}age+=Mathf.Max(0,dt);materials.Update(view);foreach(var l in layers)l.Begin();if(Active){float fade=Fade(age,2.3f,style.duration);
   if(sample==WaterSample.LivingWater)Mass(target,72,age,fade);
   else if(sample==WaterSample.Whirlpool)Whirlpool(target+Vector2.down*55,155,age,fade);
   else if(sample==WaterSample.PressureShot){float t=age-style.pressureCharge,u=Mathf.Clamp01(t/style.shotTravel);if(t<style.shotTravel){var p=Vector2.Lerp(source,target,u);Mass(p,24+12*u,age*3,Fade(t,style.shotTravel-.03f,style.shotTravel));if(t>0){var dir=(target-source).normalized;for(int i=1;i<9;i++)Flow(p-dir*i*9,28-i*2,.48f,age*8+i,1-i/9f,1,i,3,3);Disc(p+Vector2.up*3,8,1,style.pale,.95f,2);Droplets(p-dir*20,t*.5f,.35f,.8f,14);}}Impact(target,t-style.shotTravel,.9f);}
   else {if(age<.65f)Mass(target,Mathf.Lerp(80,22,Mathf.Clamp01(age/.65f)),age*4,1);Impact(target,age-.65f,1.45f);}
  }ActiveCount=0;foreach(var l in layers){ActiveCount+=l.Count;DroppedLayers+=l.Overflow;l.End();}PeakQuads=Mathf.Max(PeakQuads,ActiveCount);}
 }
}
