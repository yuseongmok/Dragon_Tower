using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  // Ordered entry points avoid the old shared center intersection. Different offsets
  // and landing depths give each spear its own direction while preserving echo paths.
  static readonly float[] EchoEntryX={-350,-155,-130,35,70,245,350};
  static readonly float[] EchoLandingY={-35,35,-70,0,55,-45,20};
  void RenderSpearEcho(BattleModel b,Vector2 ground,float clock){
   bool side=b.BossDirection!=BossBladeDirection.Front;
   Color color=side?purple:gold;
   float u=b.BossPhaseProgress,age=(float)(b.Time-b.BossImpactAt);
   bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;
   bool echo=b.BossShotNumber==2;
   // Identical lane geometry for both volleys; the telegraph explicitly recolors the echo.
   rear.transform.SetAsLastSibling();front.transform.SetAsLastSibling();floor.transform.SetAsLastSibling();
   for(int i=0;i<7;i++){
    float x=(i-3)*70;
    var hit=ground+new Vector2(x,EchoLandingY[i]);
    var origin=new Vector2(EchoEntryX[i],60);
    var dir=(hit-origin).normalized;var n=new Vector2(-dir.y,dir.x);
    if(echo||b.BossState==BossSkillState.Interval||b.BossState==BossSkillState.Recovery){
     float f=b.BossState==BossSkillState.Recovery?1-u:1;
     rear.Line(origin,hit,11,A(side?purple:gold,.16f*f));
     rear.Line(origin,hit,2,A(ivory,.3f*f));
    }
    if(prep){
     rear.Line(origin,hit,7,A(ink,.8f));rear.Line(origin,hit,3,A(color,.45f+.5f*u));
     for(int j=0;j<10;j++){var q=Vector2.Lerp(origin,hit,Mathf.Repeat(clock*.75f+j*.1f,1));rear.Line(q-dir*14,q,4,A(color,.8f));}
     Ring(floor,hit,20+20*(1-u),.3f,3,color,24);
    }
    if(travel){
     var end=Vector2.Lerp(origin,hit,Mathf.Clamp01(u/.8f));
     front.Line(origin,end,29,A(color,.4f));front.Line(origin,end,17,color);
     front.Line(origin,end,8,A(ivory,.95f));front.Line(origin,end,3,Color.white);
     for(int j=0;j<8;j++){var q=Vector2.Lerp(origin,end,j/8f);front.Line(q+n*17,q-dir*26+n*12,3,color);}
    }
    if(b.BossSkillHasImpacted&&age>=0&&age<.8f){
     float f=1-age/.8f;var c=b.BossLastImpactDirection==BossBladeDirection.Front?gold:purple;
     if(age<.1f)front.Line(origin,hit,22*(1-age/.1f),A(ivory,.8f));
     Ring(front,hit,18+age*130,.3f,5*f,A(c,f),32);
     for(int j=0;j<14;j++){
      float a=j*2.39996f;var q=hit+new Vector2(Mathf.Cos(a)*age*150,Mathf.Abs(Mathf.Sin(a))*Mathf.Sin(age/.8f*Mathf.PI)*90-age*30);
      front.Diamond(q,3*f,11*f,A(c,f));front.Line(q,q+Polar(a,8*f),2,A(ivory,f));
     }
    }
   }
   if(prep)Telegraph(ground,u,color,side);
  }
 }
}
