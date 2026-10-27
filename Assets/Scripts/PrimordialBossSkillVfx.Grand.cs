using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  // Presentation only: all phases and impacts are read from the replay clock.
  void GrandDetail(BattleModel b,Vector2 center,Vector2 target,Color c,float clock,float fade){
   var s=b.ActiveBossSkill;bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;
   float u=b.BossPhaseProgress;bool finale=s.oncePerEncounter;
   bool seal=s.attackVfx=="contract_seal",debris=s.attackVfx=="contract_debris";
   bool breath=s.attackVfx=="contract_breath"||finale&&b.BossShotNumber==4;
   bool side=s.directionalShots>0&&b.BossDirection!=BossBladeDirection.Front;
   if(seal){
    float hp=b.EnemyShieldHP/(float)Mathf.Max(1,b.EnemyShieldMaxHP);
    for(int i=0;i<12;i++){
     float a=i*Mathf.PI/6+clock*.045f;var p=center+Polar(a,178);
     bool lit=prep||i<Mathf.CeilToInt(hp*12);
     var col=A(lit?c:stone,fade*(lit?.9f:.25f));
     var t=Polar(a+Mathf.PI*.5f,22);var n=Polar(a,14);
     rear.Quad(p-t-n,p-t+n,p+t+n,p+t-n,A(ink,.9f*fade));
     rear.Line(p-t+n,p+t+n,5,col);
     rear.Diamond(p,8,18,col);rear.Diamond(p,3,9,A(ivory,.8f*fade));
    }
   }
   if(prep){
    // Energy converges from the arena edges, leaving the central actor readable.
    for(int i=0;i<18;i++){
     float t=Mathf.Repeat(clock*.55f+i*.137f,1);
     var start=new Vector2(i%2==0?-230:230,-160-(i%9)*48);
     var end=breath?center: new Vector2(start.x*.6f,-28);
     var p=Vector2.Lerp(start,end,t);
     rear.Line(p,p+(end-start).normalized*18,3,A(c,t*.65f));
     rear.Diamond(p,3,6,A(ivory,t*.65f));
    }
   }
   if(travel&&breath){
    // Twisting segmented ribbons add depth without a full-screen flash.
    var start=center+new Vector2(-65,-25);
    var end=Vector2.Lerp(start,target,u);
    var d=end-start;var n=new Vector2(-d.y,d.x).normalized;
    for(int sign=-1;sign<=1;sign+=2)for(int i=0;i<20;i++){
     float t=i/20f,z=(i+1)/20f;
     var p=Vector2.Lerp(start,end,t)+n*(sign*(25+30*t)+Mathf.Sin(t*24-clock*16)*9);
     var q=Vector2.Lerp(start,end,z)+n*(sign*(25+30*z)+Mathf.Sin(z*24-clock*16)*9);
     front.Line(p,q,7,A(c,.8f));front.Line(p,q,2,A(ivory,.9f));
    }
   }
   if(travel&&!seal&&!debris&&!breath&&!side){
    var home=new Vector2((Mathf.Max(0,b.BossShotNumber-1)%3-1)*150,100);
    var p=Vector2.Lerp(home,target,u*u);
    var d=(target-home).normalized;
    for(int sign=-1;sign<=1;sign+=2){
     var n=new Vector2(-d.y,d.x)*sign;
     front.Line(p-d*40+n*22,p-d*185+n*35,10,A(c,.45f));
     front.Line(p-d*70+n*24,p-d*155+n*29,3,A(ivory,.8f));
     for(int i=0;i<5;i++)front.Diamond(p-d*(45+i*29)+n*(30+i*6),4,12,A(c,1-i*.15f));
    }
   }
   float age=(float)(b.Time-b.BossImpactAt);
   if(b.BossSkillHasImpacted&&age>=0&&age<.85f){
    float t=age/.85f,alpha=1-t;
    var hit=target;
    if(side)hit.x+=b.BossLastImpactDirection==BossBladeDirection.Left?-145:145;
    for(int i=0;i<12;i++){
     float a=i*Mathf.PI/6;
     var p=hit+Polar(a,35+175*t,.3f);
     var q=p+Vector2.up*(25+(i%3)*20)*Mathf.Sin(t*Mathf.PI);
     floor.Line(hit+Polar(a,25,.3f),p,4*alpha,A(ink,alpha));
     floor.Line(hit+Polar(a,40,.3f),p,2*alpha,A(c,alpha*.8f));
     front.Diamond(q,5*alpha,18*alpha,A(c,alpha));
     front.Diamond(q+Vector2.up*5,2*alpha,8*alpha,A(ivory,alpha));
    }
    Ring(floor,hit,40+190*t,.36f,9*alpha,A(ink,alpha));
    Ring(floor,hit,40+190*t,.36f,4*alpha,A(c,alpha));
   }
  }
 }
}
