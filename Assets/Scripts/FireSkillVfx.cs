using UnityEngine;
namespace DragonTower {
 // Art only. Hit() is called at the existing damage cue, never on a visual timer.
 public sealed partial class FireSkillVfx:MonoBehaviour {
  BattleView view; FireSkillLibrary library; FireStylePrototype painter; SkillData skill; FireSkillKind kind;
  Vector2 origin,target,ground; float age=100,lastHit=100; readonly float[] hits=new float[32];
  public int ScheduledHits{get;set;} int Count=>Mathf.Max(skill==null?1:skill.hitCount,ScheduledHits);
  public bool DamageFromSkill{get;set;} public int HitsShown{get;private set;}
  public bool Configured{get;private set;} public int ActiveImages=>painter==null?0:painter.ActiveCount;
  public int DroppedLayers=>painter==null?0:painter.DroppedLayers; public int PeakImages=>painter==null?0:painter.PeakImages; public bool BurnShown=>painter!=null&&painter.BurnShown;
  public Vector2 LastOrigin=>origin; public bool Active=>Configured&&age<End+.95f;
  float End=>skill==null?0:skill.initialHitDelay+Mathf.Max(0,Count-1)*skill.hitInterval;
  public void Initialize(BattleView owner){view=owner;library=Resources.Load<FireSkillLibrary>("FireSkills/Library");}
  public void Configure(SkillData data){Clear();skill=data;Configured=library!=null&&library.Find(data,out kind);if(Configured&&painter==null){painter=gameObject.AddComponent<FireStylePrototype>();painter.style=library.style;painter.Initialize(view);}if(painter!=null)painter.SetUpperOverlay(Configured&&kind==FireSkillKind.Purgatory);}
  void Locate(){view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out origin);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));ground=target+new Vector2(0,-65);}
  public void Cast(){if(!Configured)return;Clear();Locate();age=0;lastHit=100;for(int i=0;i<hits.Length;i++)hits[i]=-1;Render(false);}
  public void Hit(bool first){if(!Configured)return;if(first||!Active)Cast();if(HitsShown<hits.Length)hits[HitsShown++]=age;lastHit=0;Render(false);}
  public void Step(BattleModel battle,float dt){if(!Configured)return;if(battle==null||battle.Result!=BattleResult.Fighting){Clear();return;}age+=dt;lastHit+=dt;Render(battle.EnemyBurning);Feedback();}
  public void Clear(){RestoreFeedback();age=100;lastHit=100;HitsShown=0;if(painter!=null)painter.Clear();}
  void OnDisable()=>Clear();
  void Render(bool burn){if(painter==null)return;painter.BeginLayers(age,target,ground);if(Active){if(kind==FireSkillKind.Shot)RenderShot();else if(kind==FireSkillKind.Meteor)RenderMeteor();else if(kind==FireSkillKind.Purgatory)RenderPurgatory();else RenderBurst();}painter.Status(burn);painter.EndLayers();}
  void L(FireStyleModule m,Vector2 p,Vector2 size,float a=1,float angle=0,bool back=false,bool glow=false,float phase=-1){painter.Layer(m,phase<0?age:phase,p,size,a,angle,back,glow,phase>=0);}
  void Flame(Vector2 p,Vector2 size,float phase,float alpha=1,float angle=0,bool back=false)=>painter.LayeredFlame(p,size,phase,alpha,back,1,angle);
  void Trail(Vector2 from,Vector2 to,float width,float alpha,int segments=7){var d=to-from;float angle=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90;for(int j=0;j<segments;j++){float u=(j+.5f)/segments;Flame(Vector2.Lerp(from,to,u),new Vector2(width*(.2f+.8f*u),d.magnitude/segments*1.9f),age+j*.13f,alpha*u,angle+180);}}
  void Impact(float h,float scale,int count,bool final){if(h<0||h>.85f)return;float expand=1-Mathf.Pow(1-Mathf.Clamp01(h/.19f),3),fade=Mathf.Clamp01((.65f-h)/.28f);
   L(FireStyleModule.Burst,target,new Vector2(1,.83f)*scale*(.35f+expand),fade,-12,true,false,h/.65f);
   L(FireStyleModule.Burst,target+new Vector2(0,8),Vector2.one*scale*(.22f+expand*.82f),fade,23,false,false,h/.58f);
   if(h<.10f)L(FireStyleModule.Burst,target,Vector2.one*scale*.76f,1-h/.10f,0,false,true,h/.1f);
   L(FireStyleModule.Shockwave,ground,new Vector2(scale*1.5f,scale*.35f)*(.4f+expand),fade*.85f,0,false,true,h/.65f);
   for(int i=0;i<5;i++){float a=i*2.4f;var p=target+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.7f)*scale*(.15f+h*.4f);if(h<.4f)Flame(p,new Vector2(scale*.24f,scale*.44f),age+i*.17f,Mathf.Clamp01((.4f-h)/.18f),a*Mathf.Rad2Deg-90,i%2==0);if(h>.07f)L(FireStyleModule.Smoke,p+new Vector2(0,h*32),Vector2.one*scale*(.23f+h*.16f),fade*.5f,i*30,true);}
   for(int i=0;i<count;i++){float a=i*2.399f;bool back=i%3==0;float t=h/.8f;var p=target+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.7f)*(10+h*scale*(.7f+i%4*.15f))+new Vector2(0,30*h-65*h*h);L(FireStyleModule.Ember,p,new Vector2(16,30)*(back?.6f:1+t)*(final?1.2f:1),1-t,i*31,back,!back);}
   if(h>.24f)for(int i=0;i<3;i++)Flame(ground+new Vector2((i-1)*39,15),new Vector2(34,54),age+i*.2f,fade*.8f);
  }
  void RenderBurst(){float h=HitsShown>0?age-hits[0]:-1;var d=target-origin;float angle=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg-90;
   // Zero-delay data: a full connection streak and contact core appear in the damage frame.
   // For data with a genuine cast lead-in, this same module travels before contact.
   if(h<.067f){float t=h<0?Mathf.Clamp01(age/Mathf.Max(.001f,skill.initialHitDelay)):1;var p=Vector2.Lerp(origin,target,t);Trail(origin,p,46,h<0?1:1-h/.067f);Flame(p,new Vector2(85,110),age,1,angle+180);}
   for(int i=0;i<HitsShown;i++)Impact(age-hits[i],170,26,false);
  }
 }
}




