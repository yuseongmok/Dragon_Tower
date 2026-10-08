using UnityEngine;
namespace DragonTower {
 // Observes existing damage cues; never schedules or applies gameplay damage.
 public sealed partial class WaterSkillVfx:MonoBehaviour {
  BattleView view;WaterSkillLibrary library;WaterStylePrototype p;SkillData skill;WaterSkillKind kind;
  readonly float[] hits=new float[128];float age=100;Vector2 origin,target;
  public bool DamageFromSkill{get;set;}public int ScheduledHits{get;set;}public int HitsShown{get;private set;}public bool Configured{get;private set;}
  public int ActiveImages=>p==null?0:p.ActiveCount;public int PeakImages=>p==null?0:p.PeakQuads;public int DroppedLayers=>p==null?0:p.DroppedLayers;public Vector2 LastOrigin=>origin;
  int Count=>Mathf.Max(skill==null?1:skill.hitCount,ScheduledHits);
  float End=>skill==null?0:skill.initialHitDelay+(Count-1)*skill.hitInterval;
  public bool FinalImpact=>kind==WaterSkillKind.Splash&&HitsShown>=Count;
  public bool Active=>Configured&&age<End+1.7f;
  public void Initialize(BattleView owner){view=owner;library=Resources.Load<WaterSkillLibrary>("WaterSkills/Library");}
  public void Configure(SkillData data){Clear();skill=data;Configured=library!=null&&library.Find(data,out kind);if(Configured&&p==null){p=gameObject.AddComponent<WaterStylePrototype>();p.style=library.style;p.Initialize(view);}}
  public void Cast(){if(!Configured)return;Clear();view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out origin);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));age=0;p.Play(WaterSample.PressureShot,origin,target);Render();}
  public void Hit(bool first){if(!Configured)return;if(!Active)Cast();if(HitsShown<hits.Length)hits[HitsShown++]=age;Render();}
  public void Step(BattleModel model,float dt){if(!Configured)return;if(model==null||model.Result!=BattleResult.Fighting){Clear();return;}age+=Mathf.Max(0,dt);Render();}
  public void Clear(){age=100;HitsShown=0;p?.Clear();}void OnDisable()=>Clear();
  static float Fade(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));
  static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}
  static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));
  void Render(){if(p==null)return;p.BeginLayers(age);if(Active){if(kind==WaterSkillKind.Aqua)Aqua();else RenderAdvanced();}p.EndLayers();}
  partial void RenderAdvanced();
  // Internal ribbons advect down the stream, with independent transverse phase.
  void Stream(Vector2 a,Vector2 b,float width,float time,float alpha,int depth=1){var s=library.style;var dir=(b-a).normalized;var n=new Vector2(-dir.y,dir.x);
   for(int j=0;j<28;j++){float u=j/28f,v=(j+1)/28f;var x=Vector2.Lerp(a,b,u);var y=Vector2.Lerp(a,b,v);float w=width*(.8f+.16f*Mathf.Sin(u*17-time*23)),z=width*(.8f+.16f*Mathf.Sin(v*17-time*23));p.Quad(x-n*w,x+n*w,y+n*z,y-n*z,A(s.deep,alpha*.8f),depth);p.Quad(x-n*w*.65f,x+n*w*.6f,y+n*z*.6f,y-n*z*.65f,A(s.cyan,alpha*.86f),depth);
    for(int k=0;k<3;k++){float off=Mathf.Sin(u*13-time*(14+k*5)+k*2)*width*.43f,off2=Mathf.Sin(v*13-time*(14+k*5)+k*2)*width*.43f;p.Line(x+n*off,y+n*off2,width*(k==0?.22f:.1f),A(k==0?s.pale:s.aqua,alpha*.9f),depth);}if(j%4==0)p.Line(x+n*w*.8f,y+n*z*.8f,2,A(s.foam,alpha),depth);}
  }
  void Aqua(){if(HitsShown==0)return;float t=age-hits[0],f=Fade(t,.04f,.18f);Stream(origin,target,12,t,f);p.Mass(target,28,t*5,f);p.Disc(target+new Vector2(-5,5),10,.8f,library.style.pale,f,2);SharpBurst(target,t,.8f);}
 }
}
