using UnityEngine;
namespace DragonTower {
 // Isolated geometric art prototype: never emits combat events or modifies battle state.
 public sealed class LightStylePrototype:MonoBehaviour {
  public LightVfxStyle style;
  LightningMeshLayer[] layers;BattleVfxHudVisibility materials;BattleView view;Vector2 source,target;float age=10;LightSample sample;
  public int ActiveCount{get;private set;}public int PeakQuads{get;private set;}public int DroppedLayers{get;private set;}
  public bool Active=>age<3;public bool Clean=>ActiveCount==0;
  public void Initialize(BattleView owner){if(layers!=null)return;view=owner;materials=new BattleVfxHudVisibility();layers=new LightningMeshLayer[3];for(int i=0;i<3;i++){var go=new GameObject("Light geometric depth "+i,typeof(RectTransform),typeof(UnityEngine.CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=new Vector2(480,850);layers[i]=go.GetComponent<LightningMeshLayer>();layers[i].raycastTarget=false;layers[i].material=i==2?materials.additive:materials.alpha;if(i==0)r.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());}BattleVfxHudVisibility.BringHudForward(view);}
  public void Play(LightSample kind,Vector2 origin,Vector2 hit){Clear();sample=kind;source=origin;target=hit;age=0;PeakQuads=DroppedLayers=0;}
  public void Clear(){Restore();age=10;ActiveCount=0;if(layers!=null)foreach(var l in layers)if(l!=null){l.Begin();l.End();}}
  void OnDisable()=>Clear();void OnDestroy(){Clear();materials?.Dispose();if(layers!=null)foreach(var l in layers)if(l!=null)Destroy(l.gameObject);}
  RectTransform arena;Vector2 arenaHome;bool shifted;
  void Restore(){if(shifted&&arena!=null)arena.anchoredPosition=arenaHome;shifted=false;if(layers!=null)foreach(var l in layers)if(l!=null)l.rectTransform.anchoredPosition=Vector2.zero;}
  public void BeginLayers(float time){Restore();age=time;materials.Update(view);foreach(var l in layers)l.Begin();}
  public void EndLayers(){ActiveCount=0;foreach(var l in layers){ActiveCount+=l.Count;DroppedLayers+=l.Overflow;l.End();}PeakQuads=Mathf.Max(PeakQuads,ActiveCount);}
  public void Impulse(float t,float strength,float duration){if(t<0||t>=duration)return;if(arena==null)arena=(RectTransform)view.frame.Find("Arena");if(arena==null)return;Restore();float k=1-t/duration;var offset=new Vector2(Mathf.Sin(t*165)*strength,Mathf.Cos(t*139)*strength*.65f)*k;arenaHome=arena.anchoredPosition;arena.anchoredPosition+=offset;shifted=true;foreach(var l in layers)l.rectTransform.anchoredPosition=offset;}
  public void Line(Vector2 a,Vector2 b,float width,Color c,int depth=1)=>layers[depth].Line(a,b,width,c);
  public void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color,int depth=1)=>layers[depth].Quad(a,b,c,d,color);
  static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}
  static float F(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));
  static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));
  static float R(int i)=>Mathf.Repeat(Mathf.Sin(i*127.1f+31.7f)*43758.5453f,1);
  Vector2 Project(Vector2 center,Vector2 local,Quaternion rotation){Vector3 p=rotation*new Vector3(local.x,local.y,0);float perspective=620/(620+p.z);return center+new Vector2(p.x,p.y)*perspective;}
  void Edge(Vector2 a,Vector2 b,float width,Color color,float alpha,int depth){layers[depth].Line(a,b,width+3,A(style.deep,alpha*.7f));layers[depth].Line(a,b,width,A(color,alpha));}
  public void Ring(Vector2 center,float radius,Quaternion q,float spin,float width,Color color,float alpha,int depth,int segments=96,float reveal=1){for(int i=0;i<segments*reveal;i++){float a=spin+i*Mathf.PI*2/segments,b=spin+(i+1)*Mathf.PI*2/segments;Edge(Project(center,D(a)*radius,q),Project(center,D(b)*radius,q),width,color,alpha,depth);}}
  public void Glyph(Vector2 center,float radius,Quaternion q,float angle,float size,int variant,float alpha,int depth){Vector2 radial=D(angle),tangent=D(angle+Mathf.PI*.5f),p=radial*radius;Vector2 P(float x,float y)=>Project(center,p+tangent*x*size+radial*y*size,q);Edge(P(0,-1),P(0,1),2,style.ivory,alpha,depth);Edge(P(-.6f,.35f),P(0,.9f),2,style.pale,alpha,depth);Edge(P(0,.9f),P(.6f,.35f),2,style.pale,alpha,depth);if(variant%2==0)Edge(P(-.65f,-.4f),P(.65f,-.4f),2,style.pale,alpha,depth);else{Edge(P(-.5f,-.7f),P(.5f,.2f),2,style.pale,alpha,depth);Edge(P(-.5f,.2f),P(.5f,-.7f),2,style.pale,alpha,depth);}}
  public void Circle(Vector2 center,float radius,Vector3 tilt,float time,float alpha,int tier,int depth){if(time<0||alpha<=0)return;float open=Mathf.SmoothStep(0,1,Mathf.Clamp01(time/.30f));float r=radius*(.28f+.72f*open);var q=Quaternion.Euler(tilt);float spin=time*.55f;Radiance(center,r,tilt,time,alpha*open,depth);
   Ring(center,r,q,spin,3,style.gold,alpha,depth,96,open);Ring(center,r*.955f,q,spin,1.5f,style.ivory,alpha*.8f,depth,96,open);
   Ring(center,r*.76f,q,-spin,2,style.pale,alpha,depth,80,open);if(tier>0)Ring(center,r*.69f,q,-spin,1.5f,style.gold,alpha*.8f,depth,80,open);
   int n=tier==0?6:tier==1?12:18;for(int i=0;i<n;i++){float lit=Mathf.Clamp01((time-.14f-i*.017f)*13);Glyph(center,r*.85f,q,spin+i*Mathf.PI*2/n,Mathf.Max(3,r*.055f),i,alpha*lit,depth);}
   int points=tier==0?3:6;for(int i=0;i<points;i++){float a=-spin+i*Mathf.PI*2/points,b=-spin+(i+2)*Mathf.PI*2/points;Edge(Project(center,D(a)*r*.64f,q),Project(center,D(b)*r*.64f,q),1.6f,style.pale,alpha*.8f*open,depth);}
   if(tier==2){Ring(center,r*1.12f,q,spin*.6f,1,style.gold,alpha*.65f,depth,112,open);for(int i=0;i<8;i++){float a=spin*.6f+i*Mathf.PI/4;Vector2 p=Project(center,D(a)*r*1.10f,q);layers[depth].Diamond(p,4,7,A(style.ivory,alpha));}}
   float active=Mathf.Clamp01((time-.35f)*4);Ring(center,r*.22f,q,spin*1.4f,2,style.ivory,alpha*active,depth,32);
   var c=Project(center,Vector2.zero,q);Star(c,5+8*active,alpha*active,depth);for(int i=0;i<16;i++){float cycle=Mathf.Repeat(time*1.1f+i*.061f,1);var p=Project(center,D(i*2.39996f+spin)*r*(1.32f-cycle),q);layers[depth].Diamond(p,1.8f,3,A(i%3==0?style.ivory:style.pale,alpha*Mathf.Sin(cycle*Mathf.PI)));}
  }
  public void Star(Vector2 p,float size,float alpha,int depth){layers[depth].Diamond(p,size*.42f,size,A(style.gold,alpha));layers[depth].Diamond(p,size,size*.22f,A(style.pale,alpha));layers[depth].Diamond(p,size*.17f,size*.8f,A(style.core,alpha));}
  public void Disc(Vector2 p,float radius,Quaternion q,Color color,float alpha,int depth){for(int band=5;band>=1;band--){float outer=radius*band/5f,inner=radius*(band-1)/5f;for(int i=0;i<40;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;layers[depth].Quad(Project(p,D(a)*inner,q),Project(p,D(a)*outer,q),Project(p,D(b)*outer,q),Project(p,D(b)*inner,q),A(color,alpha*(1-band/6f)));}}}
  void Radiance(Vector2 center,float radius,Vector3 tilt,float time,float alpha,int depth){var q=Quaternion.Euler(tilt);Disc(center,radius*.96f,q,style.gold,alpha*.13f,depth);for(int i=0;i<64;i++){float a=i*Mathf.PI/32+time*.55f,b=a+.028f;var p=Project(center,D(a)*radius*.955f,q);var e=Project(center,D(b)*radius*.955f,q);layers[2].Line(p,e,8,A(style.pale,alpha*.10f));layers[2].Line(p,e,2,A(style.ivory,alpha*(.4f+.4f*Mathf.Sin(time*3+i*.15f))));if(i%4==0)layers[depth].Line(Project(center,D(a)*radius*.68f,q),Project(center,D(a)*radius*.73f,q),3,A(style.pale,alpha*.75f));}Disc(center,24,q,style.pale,alpha*.8f,2);Disc(center,12,q,style.core,alpha,2);}
  public void Beam(Vector2 a,Vector2 b,float t){if(t<0||t>style.beamDuration+.18f)return;float fade=F(t,style.beamDuration*.65f,style.beamDuration+.18f);Vector2 direction=(b-a).normalized,normal=new Vector2(-direction.y,direction.x);float reach=Mathf.Clamp01(t/.065f);var end=Vector2.Lerp(a,b,reach);float width=(24+7*Mathf.Sin(t*38))*fade;
   for(int layer=0;layer<5;layer++){float w=width*(1-layer*.19f);Color c=layer==0?style.deep:layer==1?style.gold:layer==2?style.pale:layer==3?style.ivory:style.core;for(int i=0;i<12;i++){float u=i/12f,v=(i+1)/12f;float wa=w*(.58f+.42f*Mathf.Sin(u*Mathf.PI)),wb=w*(.58f+.42f*Mathf.Sin(v*Mathf.PI));layers[1].Quad(Vector2.Lerp(a,end,u)-normal*wa,Vector2.Lerp(a,end,u)+normal*wa,Vector2.Lerp(a,end,v)+normal*wb,Vector2.Lerp(a,end,v)-normal*wb,A(c,fade*(layer==0?.45f:1)));}}
   layers[2].Line(a+normal*(width*.73f+2),end+normal*(width*.65f+2),1.7f,A(style.cyan,fade*.55f));layers[2].Line(a-normal*(width*.73f+2),end-normal*(width*.65f+2),1.7f,A(style.lavender,fade*.6f));
   for(int i=0;i<30;i++){float k=Mathf.Repeat(t*3+i*.037f,1);var p=Vector2.Lerp(a,end,k)+normal*((i%2==0?1:-1)*(width+5+R(i)*20));layers[2].Line(p-direction*(6+R(i+5)*16),p,2,A(i%4==0?style.pale:style.ivory,fade*.7f));}Star(a,26,fade,2);Star(end,22,fade,2);
  }
  public void Impact(Vector2 p,float t,float scale){if(t<0||t>1.2f)return;float flash=F(t,.045f,.16f);Disc(p,75*scale,Quaternion.identity,style.pale,F(t,.06f,.35f)*.9f,2);Disc(p,35*scale,Quaternion.identity,style.ivory,F(t,.12f,.29f),2);Star(p,76*scale,flash,2);for(int i=0;i<14;i++){float a=i*Mathf.PI/7;float k=Mathf.Clamp01(t/.17f);float r=(35+k*(90+R(i)*50))*scale;Vector2 dir=D(a),side=D(a+1.57f);layers[1].Quad(p+dir*20*scale,p+dir*45*scale-side*7*scale,p+dir*r,p+dir*45*scale+side*7*scale,A(i%3==0?style.ivory:style.pale,F(t,.10f,.40f)));}
   float radius=(20+Mathf.Clamp01(t/.30f)*135)*scale;Ring(p,radius,Quaternion.Euler(53,-10,8),0,5,style.gold,F(t,.13f,.58f),1);Ring(p+Vector2.up*4,radius*.88f,Quaternion.Euler(53,-10,8),0,2,style.ivory,F(t,.13f,.54f),2);Ring(p,radius*.66f,Quaternion.identity,0,1.5f,style.pale,F(t,.08f,.38f),1);
   for(int i=0;i<40;i++){float life=.45f+R(i+9)*.55f;if(t>life)continue;float a=i*2.39996f;Vector2 dir=D(a);var q=p+dir*(28+t*(90+R(i)*170))*scale;float f=F(t,life*.55f,life);if(i%5==0){var rot=Quaternion.Euler(0,0,t*55);Glyph(q,0,rot,a,5*scale,i,f, i%2==0?0:2);}else{layers[2].Diamond(q,2*scale,4*scale,A(i%7==0?style.cyan:i%6==0?style.pink:style.ivory,f));if(i%3==0)layers[1].Line(q-dir*12*scale,q,2,A(style.gold,f*.7f));}}
  }
  public void Step(BattleModel model,float dt){if(layers==null)return;if(model==null||model.Result!=BattleResult.Fighting){Clear();return;}age+=Mathf.Max(0,dt);materials.Update(view);foreach(var l in layers)l.Begin();if(Active){float fade=F(age,2.1f,2.85f);
   if(sample==LightSample.MagicCircle)Circle(target,121,new Vector3(27,-18,0),age,fade,1,1);
   else if(sample==LightSample.PerspectiveCircles){Circle(target+new Vector2(-105,92),65,new Vector3(34,47,-22),age,fade*.65f,0,0);Circle(target+new Vector2(111,76),73,new Vector3(-20,-54,24),age-.12f,fade*.75f,1,0);Circle(target+new Vector2(-125,-75),94,new Vector3(35,52,-17),age-.24f,fade,1,1);Circle(target+new Vector2(93,-116),111,new Vector3(58,-28,16),age-.36f,fade,2,1);}
   else if(sample==LightSample.LightBeam){Circle(source,59,new Vector3(17,-34,0),age,F(age,.94f,1.5f),1,1);Beam(source,target,age-style.charge);Impact(target,age-style.charge-.065f,.85f);}
   else{Circle(target,139,new Vector3(52,12,13),age,F(age,style.charge,style.charge+.23f),2,0);float t=age-style.charge;Impact(target,t,1.35f);if(t>=0&&t<.11f)Circle(target,78,new Vector3(0,0,t*90),age,F(t,.03f,.11f),2,2);}
  }ActiveCount=0;foreach(var l in layers){ActiveCount+=l.Count;DroppedLayers+=l.Overflow;l.End();}PeakQuads=Mathf.Max(PeakQuads,ActiveCount);}
 }
}

