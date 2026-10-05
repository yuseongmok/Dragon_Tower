using UnityEngine;
namespace DragonTower {
 // Presentation clock only. Actual combat cues supply every contact; no gameplay writes.
 public sealed class LightSkillVfx:MonoBehaviour {
  BattleView view;LightSkillLibrary library;LightStylePrototype painter;SkillData skill;LightSkillKind kind;
  readonly float[] hits=new float[128];float age=100;Vector2 origin,target;
  public bool DamageFromSkill{get;set;}public int ScheduledHits{get;set;}public int HitsShown{get;private set;}public bool Configured{get;private set;}
  public int ActiveImages=>painter==null?0:painter.ActiveCount;public int PeakImages=>painter==null?0:painter.PeakQuads;public int DroppedLayers=>painter==null?0:painter.DroppedLayers;public Vector2 LastOrigin=>origin;
  int Count=>Mathf.Max(skill==null?1:skill.hitCount,ScheduledHits);
  float End=>skill==null?0:skill.initialHitDelay+(Count-1)*skill.hitInterval;
  public bool FinalImpact=>kind==LightSkillKind.BigBang&&HitsShown>=Count;
  public bool Active=>Configured&&age<End+1.3f;
  public void Initialize(BattleView owner){view=owner;library=Resources.Load<LightSkillLibrary>("LightSkills/Library");}
  public void Configure(SkillData data){Clear();skill=data;Configured=library!=null&&library.Find(data,out kind);if(Configured&&painter==null){painter=gameObject.AddComponent<LightStylePrototype>();painter.style=library.style;painter.Initialize(view);}}
  public void Cast(){if(!Configured)return;Clear();view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out origin);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));age=0;painter.Play(LightSample.HolyImpact,origin,target);Render();}
  public void Hit(bool first){if(!Configured)return;if(!Active)Cast();if(HitsShown<hits.Length)hits[HitsShown++]=age;Render();}
  public void Step(BattleModel battle,float delta){if(!Configured)return;if(battle==null||battle.Result!=BattleResult.Fighting){Clear();return;}age+=Mathf.Max(0,delta);Render();}
  public void Clear(){age=100;HitsShown=0;painter?.Clear();}void OnDisable()=>Clear();
  static float Fade(float t,float start,float end)=>Mathf.Clamp01((end-t)/(end-start));
  static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}
  static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));
  void Render(){if(painter==null)return;painter.BeginLayers(age);if(Active){switch(kind){case LightSkillKind.Flash:Flash();break;case LightSkillKind.Holy:Holy();break;case LightSkillKind.Sanctuary:Sanctuary();break;case LightSkillKind.BigBang:BigBang();break;}}painter.EndLayers();}
  void Flash(){if(HitsShown==0)return;float t=age-hits[0];painter.Circle(target,74,new Vector3(12,-18,0),.30f+t*2,Fade(t,.13f,.44f),0,0);painter.Impact(target,t,.72f);painter.Impulse(t,2.7f,.075f);}
  void Holy(){if(HitsShown==0)return;float t=age-hits[0];var ground=target+Vector2.down*66;painter.Circle(ground,134,new Vector3(67,7,0),.3f+t*1.8f,Fade(t,End+.14f,End+.6f),1,0);
   for(int i=0;i<HitsShown;i++){float h=age-hits[i];Column(ground,target+Vector2.up*(174+i*20),h,46+i*8,true);painter.Impact(target,h,i==Count-1?1.2f:.64f);painter.Impulse(h,i==Count-1?5:2,.09f);}RiseFragments(ground,t,End+.65f,1);
  }
  void Column(Vector2 from,Vector2 to,float t,float width,bool up){if(t<0||t>.5f)return;var s=library.style;float f=Fade(t,.10f,.48f);Vector2 dir=(to-from).normalized,n=new Vector2(-dir.y,dir.x);var end=Vector2.Lerp(from,to,Mathf.Clamp01((t+.024f)/.065f));
   for(int l=0;l<5;l++){float w=width*(1-l*.19f);Color c=l==0?s.gold:l==1?s.pale:l==2?s.ivory:s.core;for(int j=0;j<10;j++){float a=j*.1f,b=(j+1)*.1f;float wa=w*(.8f+.2f*Mathf.Sin(a*Mathf.PI)),wb=w*(.8f+.2f*Mathf.Sin(b*Mathf.PI));var p=Vector2.Lerp(from,end,a);var q=Vector2.Lerp(from,end,b);painter.Quad(p-n*wa,p+n*wa,q+n*wb,q-n*wb,A(c,f*(l==0?.35f:.75f)),l==4?2:1);}}
   for(int i=0;i<18;i++){float k=Mathf.Repeat(t*2.7f+i*.061f,1);var p=Vector2.Lerp(from,end,k)+n*((i%2==0?-1:1)*(width+6+i%4*7));painter.Line(p-dir*25,p,2,A(i%5==0?s.lavender:s.ivory,f*.85f),2);}painter.Star(from,width*.7f,f,2);
  }
  void RiseFragments(Vector2 p,float t,float life,float scale){var s=library.style;if(t>life)return;for(int i=0;i<28;i++){float a=i*2.39996f;float elapsed=Mathf.Max(0,t-i%4*.018f);var pos=p+new Vector2(Mathf.Cos(a)*(30+elapsed*70)*scale,elapsed*(140+i%5*32)*scale);float alpha=Fade(t,life*.48f,life);if(i%4==0)painter.Glyph(pos,0,Quaternion.Euler(0,0,t*(i%2==0?55:-55)),a,5*scale,i,alpha,1);else painter.Star(pos,(i%5==0?7:3)*scale,alpha,2);}}

  void Sanctuary(){if(HitsShown==0)return;float t=age-hits[0];var sky=target+new Vector2(0,162);float fade=Fade(t,End+.14f,End+.58f);
   painter.Circle(sky,148,new Vector3(62,-17,8),.30f+t*1.6f,fade,1,0);painter.Ring(sky,32+Mathf.Min(1,t/.1f)*17,Quaternion.Euler(62,-17,8),-t*2,4,library.style.core,fade,2,48);
   for(int i=0;i<HitsShown;i++){float h=age-hits[i];Column(sky,target+Vector2.down*64,h,62+i*5,false);painter.Impact(target+Vector2.down*28,h,i==Count-1?1.22f:.7f);painter.Star(target,25,Fade(h,.02f,.1f),2);painter.Impulse(h,i==Count-1?5:2,.09f);}
   var s=library.style;for(int i=0;i<22;i++){float k=Mathf.Repeat(t*2+i*.047f,1);var q=sky+new Vector2((i%2==0?-1:1)*(74+i%4*12),-k*260);painter.Line(q+Vector2.up*18,q,2,A(i%5==0?s.cyan:s.ivory,fade*.8f),2);}
  }
  void BigBang(){if(HitsShown==0)return;var s=library.style;float t=age-hits[0],end=Mathf.Max(.1f,End);bool final=HitsShown>=Count;float after=final?age-hits[HitsShown-1]:-1;var sky=target+Vector2.up*90;
   painter.Circle(sky+Vector2.up*40,173,new Vector3(57,-14,4),t*2+.12f,final?Fade(after,.02f,.30f):1,2,0);
   if(!final){float k=Mathf.Clamp01(t/end),fall=Mathf.Clamp01((k-.79f)/.21f);var center=Vector2.Lerp(sky,target,fall*fall);float radius=(26+64*Mathf.SmoothStep(0,1,Mathf.Min(1,k/.7f)))*(1-.13f*Mathf.Clamp01((k-.69f)/.1f));
    if(fall>0){for(int j=3;j>0;j--){var ghost=Vector2.Lerp(center,sky,j*.22f);painter.Disc(ghost,radius*(1-j*.15f),Quaternion.identity,s.pale,.16f,0);}painter.Line(sky,center,radius*.58f,A(s.ivory,.45f),2);for(int j=0;j<10;j++){float x=(j-4.5f)*23;painter.Line(sky+new Vector2(x,25),center+new Vector2(x*.5f,-15),2,A(s.ivory,.7f),2);}}
    Sphere(center,radius,k);for(int i=0;i<32;i++){float cycle=Mathf.Repeat(t*1.5f+i*.03125f,1),a=i*2.39996f;var dir=D(a);var q=center+dir*(radius+90*(1-cycle));painter.Line(q+dir*9,q,2,A(i%6==0?s.lavender:s.pale,Mathf.Sin(cycle*Mathf.PI)*.85f),i%2==0?0:2);painter.Star(q,3,Mathf.Sin(cycle*Mathf.PI),2);}
    for(int i=0;i<HitsShown;i++){float h=age-hits[i];painter.Star(target+new Vector2((i%2==0?-1:1)*27,0),29,Fade(h,.02f,.09f),2);painter.Ring(target,28+h*140,Quaternion.Euler(52,0,0),0,2,s.pale,Fade(h,.02f,.13f),1,40);}
   }else{float local=Mathf.Max(0,after-.045f);painter.Impact(target,local,2.02f);painter.Impulse(after,9,.14f);painter.Disc(target,225,Quaternion.identity,s.ivory,Fade(after,.04f,.19f)*.33f,2);
    for(int j=0;j<24;j++){float a=j*Mathf.PI/12;var dir=D(a);float r=115+Mathf.Clamp01(local/.24f)*190;var n=D(a+Mathf.PI*.5f);painter.Quad(target+dir*48,target+dir*75-n*9,target+dir*r,target+dir*75+n*9,A(j%3==0?s.ivory:s.gold,Fade(local,.07f,.34f)),j%2==0?0:1);}
    for(int j=0;j<12;j++){var q=target+D(j*2.39996f)*(105+local*170);painter.Glyph(q,0,Quaternion.Euler(0,0,j*30+local*45),j,7,j,Fade(local,.24f,.8f),1);}
   }
  }
  void Sphere(Vector2 p,float radius,float charge){var s=library.style;const int step=4;for(int y=-108;y<108;y+=step)for(int x=-108;x<108;x+=step){float nx=(x+2)/radius,ny=(y+2)/radius,r2=nx*nx+ny*ny;if(r2>=1)continue;float z=Mathf.Sqrt(1-r2),light=-nx*.37f+ny*.4f+z*.76f;Color c=light<.12f?Color.Lerp(s.deep,s.gold,.6f):light<.38f?s.gold:light<.64f?s.pale:light<.86f?s.ivory:s.core;float core=(nx+.16f)*(nx+.16f)+(ny-.14f)*(ny-.14f);if(core<.22f)c=Color.Lerp(s.ivory,s.core,charge);var q=p+new Vector2(x,y);painter.Quad(q,q+new Vector2(0,step),q+new Vector2(step,step),q+new Vector2(step,0),A(c,.96f),1);}
   for(int j=0;j<3;j++){var rot=Quaternion.Euler(53+j*11,j*27,j*59);painter.Ring(p,radius*(.92f+j*.045f),rot,age*(j%2==0?1.5f:-1.2f),j==0?2:1.5f,j==0?s.ivory:j==1?s.gold:s.lavender,.65f,2,72,.76f);for(int i=0;i<5;i++)painter.Glyph(p,radius*.96f,rot,i*Mathf.PI*.4f+age*(j%2==0?1.5f:-1.2f),4,i,.8f,2);}
   painter.Disc(p+new Vector2(-radius*.17f,radius*.16f),radius*.48f,Quaternion.identity,s.core,.35f,2);
  }
 }
}
