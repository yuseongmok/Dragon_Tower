using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed class AscensionVfx:MonoBehaviour {
  BattleView view;LightningStylePrototype paint;SkillData skill;float clock,castAt=-100,startAt=-100,endAt=-100,attackAt=-100,dodgeAt=-100;bool empowered;
  readonly float[] hitAt=new float[256];readonly int[] patterns=new int[256];int hitCount;
  readonly Vector2[] trail=new Vector2[12];int trailCount;Vector2 lastTip;
  public bool Configured{get;private set;}public int HitsShown=>hitCount;public int ActiveQuads=>paint==null?0:paint.ActiveCount;public bool Clean=>!empowered&&ActiveQuads==0;public int PeakQuads=>paint==null?0:paint.PeakQuads;public int Dropped=>paint==null?0:paint.DroppedLayers;
  public void Initialize(BattleView owner){view=owner;}
  public void Configure(SkillData data){Cancel();skill=data;Configured=data!=null&&data.attackEmpowerDuration>0;if(Configured&&paint==null){var lib=Resources.Load<LightningSkillLibrary>("LightningSkills/Library");paint=gameObject.AddComponent<LightningStylePrototype>();paint.style=lib.style;paint.Initialize(view);}}
  public void Cast(){if(!Configured)return;Cancel();clock=0;castAt=0;paint.Play(LightningSample.HeavyThunderImpact,Vector2.zero,Vector2.zero);}
  public void StartForm(){empowered=true;startAt=clock;endAt=-100;}
  public void EndForm(){empowered=false;endAt=clock;}
  public void Attack(){if(empowered)attackAt=clock;}
  public void Dodge(){if(empowered)dodgeAt=clock;}
  public void ExtraHit(int pattern){if(!Configured)return;int i=hitCount++%hitAt.Length;hitAt[i]=clock;patterns[i]=pattern;}
  public void Cancel(){empowered=false;clock=0;castAt=startAt=endAt=attackAt=dodgeAt=-100;hitCount=trailCount=0;paint?.Clear();if(Configured||paint!=null)LegendaryScreenDimming.Set(this,Color.clear);}
  void OnDisable()=>Cancel();
  Vector2 Anchor(DragonVisualAnchor a){view.TryGetCharacterAnchor(a,view.frame,out var p);return p;}
  Vector2 Socket(string name,Vector2 fallback)=>view.TryGetCharacterAttachment(name,view.frame,out var p)?p:fallback;
  static float Fade(float t,float a,float b)=>Mathf.Clamp01((b-t)/(b-a));
  public void Step(BattleModel battle,float dt){if(!Configured)return;if(battle==null||battle.Result!=BattleResult.Fighting){Cancel();return;}clock+=Mathf.Max(0,dt);empowered=battle.AttackEmpowered;paint.BeginLayers(clock);var ground=Anchor(DragonVisualAnchor.GroundPosition);var right=Socket("RightWing",Anchor(DragonVisualAnchor.CharacterCenter));var left=Socket("LeftWing",right+Vector2.left*55);var body=(left+right)*.5f+new Vector2(22,-30);var tip=Socket("SpearTip",Anchor(DragonVisualAnchor.SkillOrigin));var grip=Socket("WeaponTrailStart",body);var target=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));int seed=(int)(clock*26);
   if(castAt>=0&&startAt<0&&clock-castAt<skill.attackEmpowerDelay+.1f){float k=Mathf.Clamp01((clock-castAt)/Mathf.Max(.01f,skill.attackEmpowerDelay));LegendaryScreenDimming.Set(this,new Color(.01f,.02f,.10f,.42f*k));for(int i=0;i<8;i++){float a=i*.785f;var p=tip+new Vector2(Mathf.Cos(a)*100,170+Mathf.Sin(a)*32);paint.Bolt(p,Vector2.Lerp(p,tip,k),.8f,k,seed+i,0,true);}paint.Impact(tip,0,.30f+k*.3f,seed,false);}
   float arrival=clock-startAt;
   if(startAt>=0&&arrival<.6f){LegendaryScreenDimming.Set(this,new Color(.015f,.025f,.10f,.42f*Fade(arrival,.08f,.45f)));if(arrival<.16f){paint.Bolt(new Vector2(tip.x-90,440),tip,3.3f,Fade(arrival,.08f,.16f),arrival<.045f?7:seed,2,true);paint.Bolt(new Vector2(tip.x+110,420),body,1.6f,Fade(arrival,.07f,.15f),seed+8,0,true);}paint.Impact(body,Mathf.Max(0,arrival-.045f),2.2f,seed,true);paint.Impulse(arrival,10,.15f);}
   else if(startAt>=0)LegendaryScreenDimming.Set(this,Color.clear);
   if(empowered){Aura(left,right,body,tip,grip,ground,seed);if(trailCount==0)lastTip=tip;for(int i=trail.Length-1;i>0;i--)trail[i]=trail[i-1];trail[0]=lastTip;lastTip=tip;trailCount=Mathf.Min(trailCount+1,trail.Length);
    float swing=clock-attackAt;if(swing<.26f){for(int i=0;i<trailCount;i++){var c=paint.style.cyan;c.a=(1-i/(float)trail.Length)*Fade(swing,.12f,.26f)*.7f;paint.Line(trail[i],tip,8-i*.5f,c,i%2==0?0:2);}paint.Bolt(grip,tip+(tip-grip).normalized*45,1.3f,Fade(swing,.13f,.26f),seed,2,true);}
    float dodge=clock-dodgeAt;if(dodge<.25f)for(int i=0;i<3;i++)paint.Bolt(body-new Vector2(30+i*20,-15),body-new Vector2(60+i*22,55),.55f,Fade(dodge,.07f,.25f),seed+i,0,true);
   }
   if(endAt>=0){float t=clock-endAt;if(t<.15f){float k=1-t/.15f;paint.Bolt(Vector2.Lerp(body,left,k),body,1.1f,k,seed,1,true);paint.Bolt(Vector2.Lerp(body,tip,k),body,1.6f,k,seed+1,2,true);paint.Bolt(Vector2.Lerp(body,right,k),body,1.1f,k,seed+2,1,true);}if(t>=.15f&&t<.8f){paint.Impact(body,t-.15f,1.7f,seed,true);paint.Impulse(t-.15f,6,.11f);}}
   for(int n=0;n<Mathf.Min(hitCount,hitAt.Length);n++){int i=(hitCount-1-n)%hitAt.Length;float t=clock-hitAt[i];if(t>.7f)continue;float x=patterns[i]==0?-185:patterns[i]==1?185:0;if(t<.14f){var top=target+new Vector2(x+(i%3-1)*14,310);paint.Bolt(top,target,1.6f,Fade(t,.07f,.14f),seed+i*37,1,true);}if(n<5||t<.12f)paint.Impact(target,t,1.35f,i*17,false);if(n<3)paint.Residual(target,t,1.2f,seed+i);for(int j=0;j<(n<4?4:0);j++){float a=j*1.57f;paint.Bolt(target+Vector2.down*65,target+Vector2.down*65+new Vector2(Mathf.Cos(a)*100,Mathf.Sin(a)*26),.45f,Fade(t,.07f,.32f),seed+j,0,true);}}
   paint.EndLayers();
  }
  void Aura(Vector2 left,Vector2 right,Vector2 body,Vector2 tip,Vector2 grip,Vector2 ground,int seed){
   // Wide feather-like branches originate at the moving wing sockets, not the screen.
   for(int side=0;side<2;side++){var socket=side==0?left:right;float sign=side==0?-1:1;var root=body+new Vector2(sign*12,20);var apex=socket+new Vector2(sign*32,44);var edge=socket+new Vector2(sign*56,-32);var fill=paint.style.blue;fill.a=.24f;paint.Quad(root,apex,edge,root,fill,0);paint.Bolt(root,apex,.9f,1,seed+side*51,0,false);paint.Bolt(apex,edge,.85f,1,seed+side*53,2,true);for(int j=0;j<5;j++){var end=Vector2.Lerp(apex,edge,j/4f)+new Vector2(sign*(j%2==0?8:0),0);paint.Bolt(root,end,.55f,.95f,seed+j+side*61,0,false);paint.Line(Vector2.Lerp(root,end,.4f),end,2,paint.style.pale,2);}}

   var direction=(tip-grip).normalized;paint.Bolt(grip-direction*40,tip+direction*30,1.25f,1,seed+81,2,true);var normal=new Vector2(-direction.y,direction.x);paint.Bolt(grip+normal*10,tip+normal*4,.65f,.75f,seed+16,0,true);paint.Line(tip-direction*20,tip+direction*22,5,paint.style.core,2);
   for(int i=0;i<5;i++){var p=body+new Vector2(Mathf.Sin(i*2.4f+clock*3)*40,Mathf.Cos(i*2.4f)*52);paint.Bolt(p,p+new Vector2(18,27),.38f,1,seed+i,2,true);}
   var eye=right+new Vector2(24,38);paint.Line(eye-Vector2.right*3,eye+Vector2.right*3,3,paint.style.core,2);
   for(int i=0;i<28;i++){float a=i*Mathf.PI/14,b=(i+1)*Mathf.PI/14;var p=ground+new Vector2(Mathf.Cos(a)*91,Mathf.Sin(a)*24);var q=ground+new Vector2(Mathf.Cos(b)*91,Mathf.Sin(b)*24);paint.Line(p,q,2,paint.style.cyan,0);if(i%7==0)paint.Bolt(p,p+new Vector2(20,12),.35f,.8f,seed+i,0,true);}
  }
 }
}
