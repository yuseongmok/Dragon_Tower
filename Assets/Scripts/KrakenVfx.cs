using UnityEngine;
namespace DragonTower {
 // Presentation follows the independent model timeline, never applies damage or moves actors.
 public sealed partial class KrakenVfx:MonoBehaviour {
  BattleView view;BattleModel model;KrakenProfile profile;WaterStylePrototype water;PersistentAttackStats plan;
  Vector2 target,ground,origin;float age=100;int serial;
  struct Contact {public float time;public int index;}
  readonly Contact[] contacts=new Contact[12];int contactCount;
  public bool Active=>age<100;public bool Clean=>!Active&&(water==null||water.ActiveCount==0);
  public int HitsShown{get;private set;}public int Peak=>water==null?0:water.PeakQuads;public int DroppedLayers=>water==null?0:water.DroppedLayers;
  public bool LastWasFinal{get;private set;}public Vector2 LastOrigin=>origin;
  public void Initialize(BattleView v){view=v;profile=Resources.Load<KrakenProfile>("VFX/Kraken/Profile");}
  public void BindBattle(BattleModel b){Cancel();model=b;}
  bool Matches=>model!=null&&profile!=null&&model.PersistentSourceId==profile.skill.StableId;
  void Ensure(){if(water!=null)return;water=gameObject.AddComponent<WaterStylePrototype>();water.style=profile.style;water.Initialize(view);}
  public void Cast(){if(!Matches)return;Ensure();Cancel();serial=model.PersistentSerial;plan=model.PersistentPlan;age=0;HitsShown=0;contactCount=0;view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out origin);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));ground=target+Vector2.down*80;water.Play(WaterSample.Whirlpool,origin,target);Render();}
  public void Hit(){if(!Matches||!Active||model.PersistentSerial!=serial)return;age=(float)model.PersistentAttackElapsed;var beat=plan.attacks[model.PersistentHitIndex];LastWasFinal=model.PersistentHitIndex==plan.attacks.Length-1;contacts[contactCount++%contacts.Length]=new Contact{time=age,index=model.PersistentHitIndex};HitsShown++;Render();}
  public void Step(BattleModel b,float dt){if(!Active)return;if(b!=model||b==null||b.Result!=BattleResult.Fighting||b.PersistentSerial!=serial){Cancel();return;}age=(float)b.PersistentAttackElapsed;if(!b.PersistentAttackActive&&age<plan.duration-.001f){Cancel();return;}if(age>plan.duration+1.15f){Cancel();return;}Render();}
  public void Cancel(){age=100;water?.Clear();if(water!=null&&view!=null)LegendaryScreenDimming.Set(this,Color.clear);}
  void OnDisable(){if(Active)model?.CancelPersistentAttack();Cancel();}void OnDestroy(){Cancel();if(water!=null)Destroy(water);}
  static float F(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));static Color A(Color c,float a){c.a*=Mathf.Clamp01(a);return c;}static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));
  void Render(){water.BeginLayers(age);float close=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-plan.duration-.35f)/.75f)),fade=1-close;
   LegendaryScreenDimming.Set(this,new Color(.005f,.017f,.055f,.13f*fade));float energy=1+Mathf.Clamp01(age/plan.duration)*.16f;water.Whirlpool(ground,218*(1-close),age*energy,fade*.72f);water.Disc(ground+Vector2.down*12,132*(1-close),.29f,profile.shadow,.97f*fade,0);for(int i=0;i<3;i++)water.Flow(ground+Vector2.down*(i*5),125-i*25,.28f,-age*(1.4f+i*.2f),fade*.55f,0,i+1,3,4.8f);
   if(age<.7f)SummonThreads();if(age>.25f&&age<.95f)Shadows();
   for(int limb=0;limb<6;limb++)Tentacle(limb);
   for(int i=0;i<Mathf.Min(contactCount,contacts.Length);i++){var c=contacts[i];Impact(age-c.time,c.index);}
   int bubbles=age<1.1f||age>plan.duration-.9f?22:9;for(int i=0;i<bubbles;i++){float u=Mathf.Repeat(age*.36f+i*.113f,1);var p=ground+new Vector2(Mathf.Cos(i*2.399f+age*.2f)*(190-u*50),u*155-35);water.Bubble(p,2+i%4,fade*(1-u)*.55f,i%3==0?2:0);if(i%3==0)water.Disc(p+new Vector2(6,-8),2,.7f,profile.style.aqua,fade*(1-u)*.5f,0,8);}
   if(age>plan.duration-.8f&&age<plan.duration){float u=(age-plan.duration+.8f)/.8f;for(int i=0;i<4;i++)water.Flow(target,135*(1-u)+25+i*8,.4f,age*(5+i),u*.75f,2,i,5,4);}
   water.EndLayers();
  }
  void SummonThreads(){float t=age/.7f;for(int i=0;i<5;i++){float u=Mathf.Repeat(t+i*.16f,1);var p=Vector2.Lerp(origin,ground,u)+Vector2.up*Mathf.Sin(u*Mathf.PI)*(35+i*9);water.Disc(p,3+i%3,.7f,i%2==0?profile.cupLight:profile.style.pale,Mathf.Sin(u*Mathf.PI),2,12);} }
  void Shadows(){float f=F(age,.7f,.95f);for(int i=0;i<4;i++){float a=age*7+i*1.57f;var p=ground+new Vector2(Mathf.Cos(a)*105,Mathf.Sin(a)*25);water.Flow(p,70,.19f,a,f*.7f,0,i,18,3);}}
 }
}

