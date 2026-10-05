using UnityEngine;
namespace DragonTower {
 // Real damage cues are the only source of impact timing. No combat writes.
 public sealed partial class LightningSkillVfx:MonoBehaviour {
  BattleView view;LightningSkillLibrary library;LightningStylePrototype painter;SkillData skill;LightningSkillKind kind;
  readonly float[] hits=new float[128];float age=100;Vector2 origin,target;
  public bool DamageFromSkill{get;set;}public int ScheduledHits{get;set;}public int HitsShown{get;private set;}public bool Configured{get;private set;}
  public int ActiveImages=>painter==null?0:painter.ActiveCount;public int PeakImages=>painter==null?0:painter.PeakQuads;public int DroppedLayers=>painter==null?0:painter.DroppedLayers;public Vector2 LastOrigin=>origin;
  int Count=>Mathf.Max(skill==null?1:skill.hitCount,ScheduledHits);
  float End=>skill==null?0:skill.initialHitDelay+(Count-1)*skill.hitInterval;
  public bool Active=>Configured&&age<End+1.05f;
  public void Initialize(BattleView owner){view=owner;library=Resources.Load<LightningSkillLibrary>("LightningSkills/Library");}
  public void Configure(SkillData data){Clear();skill=data;Configured=library!=null&&library.Find(data,out kind);if(Configured&&painter==null){painter=gameObject.AddComponent<LightningStylePrototype>();painter.style=library.style;painter.Initialize(view);}}
  public void Cast(){if(!Configured)return;Clear();view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out origin);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));age=0;painter.Play(LightningSample.LightningStrike,origin,target);Render();}
  public void Hit(bool first){if(!Configured)return;if(first&&!Active||!Active)Cast();if(HitsShown<hits.Length)hits[HitsShown++]=age;Render();}
  public void Step(BattleModel battle,float delta){if(!Configured)return;if(battle==null||battle.Result!=BattleResult.Fighting){Clear();return;}age+=Mathf.Max(0,delta);Render();}
  public void Clear(){age=100;HitsShown=0;painter?.Clear();}void OnDisable()=>Clear();
  static float Fade(float t,float start,float end)=>Mathf.Clamp01((end-t)/(end-start));
  void Render(){if(painter==null)return;painter.BeginLayers(age);if(Active){switch(kind){case LightningSkillKind.Shock:Shock();break;case LightningSkillKind.Overheat:Overheat();break;case LightningSkillKind.Plasma:Plasma();break;case LightningSkillKind.Thunder:Thunder();break;}}painter.EndLayers();}
  void Shock(){for(int i=0;i<HitsShown;i++){float t=age-hits[i];if(t>.65f)continue;painter.Impact(target,t,.90f,i*11,false);for(int j=0;j<7;j++){float a=j*.8976f;var q=target+new Vector2(Mathf.Cos(a)*82,Mathf.Sin(a)*55);var end=target+new Vector2(Mathf.Cos(a+1.1f)*72,Mathf.Sin(a+1.1f)*50);painter.Bolt(q,end,.65f,Fade(t,.045f,.24f),(int)(t*24)+j, j%2==0?0:2,true);}painter.Residual(target,t,1,(int)(t*24));painter.Impulse(t,2.5f,.075f);}}
  void Overheat(){if(HitsShown==0)return;float since=age-hits[0],end=skill.hitInterval*(Count-1);int seed=(int)(age*24);
   for(int i=0;i<6;i++){int contact=i/2;float due=contact*skill.hitInterval,reach=due<=0?1:Mathf.Clamp01((since+.025f)/(due+.025f));float fade=Fade(since,end+.065f,end+.32f);var start=origin+new Vector2((i-2.5f)*16,(i%2==0?1:-1)*18);var finish=target+new Vector2((i-2.5f)*9,(i%2==0?1:-1)*13);var bend=Vector2.Lerp(start,finish,.5f)+new Vector2((i-2.5f)*33,15-i*7);float scale=.8f+i*.055f;
    if(reach<.5f)painter.Bolt(start,Vector2.Lerp(start,bend,reach*2),scale,fade,seed+i*71,i%3==0?0:1,true);else {painter.Bolt(start,bend,scale,fade,seed+i*71,i%3==0?0:1,true);painter.Bolt(bend,Vector2.Lerp(bend,finish,(reach-.5f)*2),scale,fade,seed+i*71+9,i%3==0?0:1,true);}}
   for(int i=0;i<HitsShown;i++){float t=age-hits[i];bool final=i==Count-1;if(t<.7f){painter.Impact(target,t,final?1.3f:.65f+i*.1f,i*31,final);painter.Impulse(t,final?4:1.5f,final?.10f:.06f);}}painter.Residual(target,Mathf.Max(0,since-end),1.25f,seed);
  }
  void Plasma(){if(HitsShown==0)return;float t=age-hits[0],duration=Mathf.Max(.12f,End),k=Mathf.Clamp01(t/duration);int seed=(int)(age*24);bool final=HitsShown>=Count;float after=final?age-hits[HitsShown-1]:-1;
   if(!final){float compress=Mathf.Clamp01((k-.78f)/.22f);float radius=(40+65*k)*(1-compress*.20f);Sphere(target,radius,k);for(int j=0;j<9;j++){float a=j*2.39996f;var dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));var from=target+dir*(140-25*k);var to=target+dir*radius*.65f;painter.Bolt(from,to,.40f+.25f*k,.65f,seed+j,j%2==0?0:2,true);}Orbit(target,radius+9,age,1);for(int i=0;i<HitsShown;i++){float h=age-hits[i];if(h<.09f){var q=target+new Vector2(Mathf.Sin(i*2.4f),Mathf.Cos(i*2.4f))*radius*.6f;painter.Impact(q,h,.30f+i*.025f,i,false);}}}
   else{painter.Impact(target,after,1.65f,seed,true);for(int j=0;j<9;j++){float a=j*2.39996f;var dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));painter.Bolt(target+dir*32,target+dir*(95+after*250),.75f,Fade(after,.06f,.30f),seed+j,j%2==0?0:2,true);}Orbit(target,(82+after*160),age,Fade(after,.04f,.28f));painter.Residual(target,after,1.4f,seed);painter.Impulse(after,5,.12f);}
  }
  void Sphere(Vector2 p,float radius,float charge){var s=library.style;const int step=6;for(int y=-120;y<120;y+=step)for(int x=-120;x<120;x+=step){float nx=(x+3)/radius,ny=(y+3)/radius,r2=nx*nx+ny*ny;if(r2>=1)continue;float z=Mathf.Sqrt(1-r2),light=-nx*.35f+ny*.45f+z*.8f;Color c=light<.12f?s.navy:light<.35f?s.violet:light<.58f?s.blue:light<.83f?s.cyan:light<1.02f?s.pale:s.core;if(r2<.14f+charge*.11f)c=Color.Lerp(s.pale,s.core,charge);c.a=.86f;var q=p+new Vector2(x,y);painter.Quad(q,q+new Vector2(0,step),q+new Vector2(step,step),q+new Vector2(step,0),c,1);}}
  void Orbit(Vector2 p,float radius,float clock,float alpha){for(int j=0;j<3;j++)for(int i=0;i<24;i++){float a=i*Mathf.PI/12+clock*(j%2==0?7:-6),b=a+Mathf.PI/12;var rotation=Quaternion.Euler(0,0,j*57);Vector2 q=p+(Vector2)(rotation*new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.42f));Vector2 end=p+(Vector2)(rotation*new Vector3(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius*.42f));int depth=Mathf.Sin(a)>0?0:2;var col=j==0?library.style.violet:library.style.cyan;col.a=alpha;painter.Line(q,end,4,col,depth);col=library.style.pale;col.a=alpha*.9f;painter.Line(q,end,1.5f,col,depth);}}

  void Thunder(){if(HitsShown==0)return;float since=age-hits[0],end=End;var sky=target+new Vector2(0,228);float cloudAlpha=Mathf.Min(1,.4f+since*12)*Fade(since,end+.15f,end+.6f);Cloud(sky,cloudAlpha);int seed=(int)(age*24);
   if(since<end+.15f)for(int j=0;j<3;j++)painter.Bolt(sky+new Vector2(-155+j*60,10+j*8),sky+new Vector2(-50+j*78,-20),.6f,cloudAlpha*.65f,seed+j,0,true);
   for(int i=0;i<HitsShown;i++){float t=age-hits[i];bool final=i==Count-1;float hold=final?.045f:0,local=Mathf.Max(0,t-hold);int pathSeed=final&&t<hold?77:seed+i*83;
    if(t<.13f+hold){float x=i%3==0?-170:i%3==1?170:0;if(final)x=0;var top=sky+new Vector2(x,70);painter.Bolt(top,target+new Vector2((i%2==0?-1:1)*12,0),final?2.8f:1.5f,Fade(t,.06f+hold,.13f+hold),pathSeed,1,true);if(final){painter.Bolt(sky+new Vector2(-160,20),target-new Vector2(38,0),1.05f,Fade(t,.08f,.19f),pathSeed+19,0,true);painter.Bolt(sky+new Vector2(170,55),target+new Vector2(40,0),1.15f,Fade(t,.08f,.19f),pathSeed+39,2,true);}}
    if(t<.75f){painter.Impact(target,local,final?2.05f:1.05f,i*91,final);painter.Residual(target,local,final?1.65f:1.1f,pathSeed);painter.Impulse(t,final?9:2.5f,final?.14f:.07f);}
   }
  }
  void Cloud(Vector2 p,float alpha){for(int layer=0;layer<3;layer++)for(int i=0;i<7;i++){var center=p+new Vector2((i-3)*47,Mathf.Sin(i*2.4f)*14+layer*9);float rx=58-layer*5,ry=32-layer*4;Color c=layer==0?library.style.navy:layer==1?Color.Lerp(library.style.navy,library.style.violet,.28f):Color.Lerp(library.style.navy,library.style.blue,.24f);c.a=alpha*(layer==0?.65f:.55f);for(int j=0;j<12;j++){float a=j*Mathf.PI/6,b=(j+1)*Mathf.PI/6;var q=center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);var r=center+new Vector2(Mathf.Cos(b)*rx,Mathf.Sin(b)*ry);painter.Quad(center,q,r,center,c,0);}}}

 }
}
