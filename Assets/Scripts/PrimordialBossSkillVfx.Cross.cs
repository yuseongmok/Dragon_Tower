using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  void RenderRotatingCross(BattleModel b,Vector2 center,Vector2 target,float clock){
   int shot=Mathf.Max(0,b.BossShotNumber-1);
   bool side=b.BossDirection!=BossBladeDirection.Front;
   bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;
   float u=b.BossPhaseProgress;
   float sweep=b.BossState==BossSkillState.Warning?0:prep?u*.65f:travel?.65f+u*.35f:1;
   float theta=Mathf.Atan2(target.y-center.y,target.x-center.x)+Mathf.PI*.5f-(shot+sweep)*Mathf.PI*.5f;
   Color c=side?purple:gold;
   float fade=b.BossState==BossSkillState.Recovery?1-u:1;
   Circle(center,178,-clock,c,.85f*fade);
   Circle(center,138,clock*.7f,ivory,.55f*fade);
   for(int i=0;i<12;i++){
    float a=theta+i*Mathf.PI/6;
    var p=center+Polar(a,160);
    Glyph(rear,p,i,10,A(c,fade));
    rear.Line(p,p+Polar(a-.3f,20),3,A(ivory,.5f*fade));
   }
   // All four arms advance clockwise by one quarter-turn per scheduled impact.
   // Thin preparation lines become solid only for the final approach.
   float width=travel?22:4;
   float opacity=(travel?.9f:.35f)*fade;
   for(int arm=0;arm<4;arm++){
    var dir=Polar(theta+arm*Mathf.PI*.5f,1);
    var n=new Vector2(-dir.y,dir.x);
    var start=center+dir*45;var end=center+dir*1000;
    if(side){
     // Split rails leave a visible rest lane where the player stands.
     for(int sign=-1;sign<=1;sign+=2){
      var offset=n*sign*32;
      front.Line(start+offset,end+offset,width+7,A(ink,opacity));
      front.Line(start+offset,end+offset,width,A(purple,opacity));
      front.Line(start+offset,end+offset,travel?4:2,A(ivory,opacity));
     }
    }else{
     front.Line(start,end,width+12,A(ink,opacity));
     front.Line(start,end,width,A(gold,opacity));
     front.Line(start,end,travel?9:2,A(ivory,opacity));
    }
    for(int j=0;j<10;j++){
     float t=Mathf.Repeat(clock*.65f+j*.1f,1);
     var p=center+dir*(70+t*600);
     rear.Diamond(p+n*45,3,6,A(c,.45f*fade));
    }
   }
   // The decision marker remains readable above characters and HUD.
   if(prep||travel){floor.transform.SetAsLastSibling();Telegraph(target,travel?1:u,c,side);}
   float age=(float)(b.Time-b.BossImpactAt);
   if(b.BossSkillHasImpacted&&age>=0&&age<.45f){
    var point=side?target+Vector2.right*145:target;
    Burst(point,age/.45f,c,false);
   }
   if(b.BossState==BossSkillState.Recovery){
    for(int i=0;i<24;i++){float a=i*2.39996f;var p=center+Polar(a,170+90*u);front.Diamond(p,5*(1-u),13*(1-u),A(c,1-u));}
   }
  }
  void RenderContinuousCross(BattleModel b,Vector2 center,Vector2 target,float clock){
   int arms=b.ActiveBossSkill.eightRayCross?8:4;float angularStep=Mathf.PI*2/arms;
   int shot=Mathf.Max(0,b.BossShotNumber-1);
   bool side=b.BossDirection!=BossBladeDirection.Front;
   bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;
   float u=b.BossPhaseProgress;
   float sweep=prep?0:travel?u:1;
   float theta=Mathf.Atan2(target.y-center.y,target.x-center.x)+angularStep-b.CrossRotationQuarter*angularStep;
   Color c=gold;
   float fade=b.BossState==BossSkillState.Recovery?1-u:1;
   Circle(center,178,-clock,c,.85f*fade);
   Circle(center,138,clock*.7f,ivory,.55f*fade);
   for(int i=0;i<12;i++){
    float a=theta+i*Mathf.PI/6;
    var p=center+Polar(a,160);
    Glyph(rear,p,i,10,A(c,fade));
    rear.Line(p,p+Polar(a-.3f,20),3,A(ivory,.5f*fade));
   }
   // All four arms advance clockwise by one quarter-turn per scheduled impact.
   // Thin preparation lines become solid only for the final approach.
   float width=travel?(arms==8?16:22):4;
   float opacity=(travel?.9f:.35f)*fade;
   for(int arm=0;arm<arms;arm++){
    var dir=Polar(theta+arm*angularStep,1);
    var n=new Vector2(-dir.y,dir.x);
    var start=center+dir*45;var end=center+dir*1000;
    Color armColor=arm%2==0?gold:purple;
    front.Line(start,end,width+12,A(ink,opacity));
    front.Line(start,end,width,A(armColor,opacity));
    front.Line(start,end,travel?7:2,A(ivory,opacity));
    for(int j=0;j<10;j++){
     float t=Mathf.Repeat(clock*.65f+j*.1f,1);
     var p=center+dir*(70+t*600);
     rear.Diamond(p+n*45,3,6,A(armColor,.45f*fade));
    }
   }
   // A visible emission hub covers the gap and joins all four rays.
   Ring(front,center,43,1,22,A(ink,fade),48);
   Ring(front,center,47,1,5,A(gold,fade),48,theta);
   Ring(front,center,33,1,3,A(purple,fade),12,-theta);
   for(int k=0;k<8;k++){
    float angle=theta+k*Mathf.PI/4;var p=center+Polar(angle,39);
    Glyph(front,p,k,6,A(k%2==0?gold:purple,fade));
   }
   front.Diamond(center,20,20,A(ink,fade));
   front.Diamond(center,14,14,A(gold,fade));
   front.Diamond(center,6,9,A(ivory,fade));
   for(int arm=0;arm<arms;arm++){
    float angle=theta+arm*angularStep;var p=center+Polar(angle,48);
    Ring(front,p,8+2*Mathf.Sin(clock*15),.8f,3,A(arm%2==0?gold:purple,fade),12,theta);
   }
   // The decision marker remains readable above characters and HUD.
   if(prep||travel){floor.transform.SetAsLastSibling();Telegraph(target,travel?1:u,side?purple:gold,side);}
   float age=(float)(b.Time-b.BossImpactAt);
   if(b.BossSkillHasImpacted&&age>=0&&age<.45f){
    var point=side?target+Vector2.right*145:target;
    Burst(point,age/.45f,c,false);
   }
   if(b.BossState==BossSkillState.Recovery){
    for(int i=0;i<24;i++){float a=i*2.39996f;var p=center+Polar(a,170+90*u);front.Diamond(p,5*(1-u),13*(1-u),A(c,1-u));}
   }
  }
 }
}
