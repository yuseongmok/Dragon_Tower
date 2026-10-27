using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  void RenderSpearJudgment(BattleModel b,Vector2 center,Vector2 ground,float clock){
   bool side=b.BossDirection!=BossBladeDirection.Front;
   int sign=b.BossDirection==BossBladeDirection.Left?-1:1;
   int shot=Mathf.Max(0,b.BossShotNumber-1);
   Vector2 hit=ground+new Vector2(side?sign*145:0,0);
   float[] origins={0,210,-210,-210,160,210};
   Vector2 origin=new Vector2(origins[shot%6],95);
   Vector2 direction=(hit-origin).normalized;
   float u=b.BossPhaseProgress;
   if(b.BossPreparation){ floor.transform.SetAsLastSibling();rear.transform.SetAsLastSibling();
    // The whole trajectory is visible before the fast strike; the safe center stays empty.
    rear.Line(origin,hit,8,A(ink,.7f));
    rear.Line(origin,hit,2,A(side?purple:gold,.35f+.5f*u));
    for(int j=0;j<9;j++){
     float t=Mathf.Repeat(clock*.6f+j/9f,1);
     var p=Vector2.Lerp(origin,hit,t);
     rear.Line(p-direction*12,p,4,A(side?purple:ivory,.5f));
    }
    Telegraph(ground,u,side?purple:gold,side);
    Ring(floor,hit,30+35*(1-u),.35f,4,side?purple:gold);
   }
   if(b.BossState==BossSkillState.Travel){
    var end=Vector2.Lerp(origin,hit,Mathf.Clamp01(u/.86f));
    var color=side?purple:gold;
    front.Line(origin,end,42,A(ink,.85f));
    front.Line(origin,end,30,A(color,.85f));
    front.Line(origin,end,16,ivory);
    front.Line(origin,end,5,Color.white);
    var n=new Vector2(-direction.y,direction.x);
    for(int k=0;k<14;k++){
     var p=Vector2.Lerp(origin,end,k/14f);
     float offset=(k%2==0?1:-1)*(22+k%3*4);
     front.Line(p+n*offset,p-direction*38+n*(offset*.7f),4,color);
    }
   }
   float age=(float)(b.Time-b.BossImpactAt);
   if(b.BossSkillHasImpacted&&age>=0&&age<.75f){
    float t=age/.75f,a=1-t;
    hit=ground+new Vector2(b.BossLastImpactDirection==BossBladeDirection.Front?0:b.BossLastImpactDirection==BossBladeDirection.Left?-145:145,0);
    var color=b.BossLastImpactDirection==BossBladeDirection.Front?gold:purple;
    if(age<.10f){front.Line(origin,hit,28*(1-age/.1f),A(ivory,.8f));}
    Burst(hit,t,color,true);
    for(int k=0;k<28;k++){
     float angle=k*2.39996f;
     var p=hit+new Vector2(Mathf.Cos(angle)*(15+175*t),Mathf.Abs(Mathf.Sin(angle))*(35+140*Mathf.Sin(t*Mathf.PI))-t*40);
     var v=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
     front.Diamond(p,6*a,14*a,A(ink,a));
     front.Quad(p-v*12*a,p+new Vector2(-3,8)*a,p+v*12*a,p+new Vector2(3,-8)*a,A(color,a));
     front.Line(p,p+v*9*a,2,A(ivory,a));
    }
   }
  }
 }
}

