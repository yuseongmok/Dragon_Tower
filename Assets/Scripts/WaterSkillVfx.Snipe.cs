using UnityEngine;
namespace DragonTower {
 public sealed partial class WaterSkillVfx {
  partial void RenderRemaining(){if(kind==WaterSkillKind.Snipe)Snipe();else RenderSplash();}
  partial void RenderSplash();
  void Snipe(){var s=library.style;float delay=skill.initialHitDelay,launch=Mathf.Max(0,delay-.06f),t=HitsShown>0?age-hits[0]:-1;float mark=HitsShown>0?Fade(t,.025f,.16f):1;float radius=Mathf.Lerp(94,62,Mathf.Clamp01(age/Mathf.Max(.01f,launch)));
   // A stable optical scope: complete ring, four crosshair bars, fine range ticks, center dot.
   for(int j=0;j<64;j++){float a=j*Mathf.PI/32,b=(j+1)*Mathf.PI/32;p.Line(target+D(a)*radius,target+D(b)*radius,6,A(s.aqua,mark),2);}
   for(int j=0;j<4;j++){var dir=D(j*Mathf.PI*.5f);p.Line(target+dir*8,target+dir*(radius+15),5,A(s.pale,mark),2);for(int k=1;k<4;k++){var n=new Vector2(-dir.y,dir.x);var q=target+dir*(18+k*10);p.Line(q-n*5,q+n*5,2,A(s.cyan,mark),2);}}p.Disc(target,4,1,s.foam,mark,2,12);
   if(age>=launch&&HitsShown==0){float u=Mathf.Clamp01((age-launch)/.06f);var q=Vector2.Lerp(origin,target,u);Vector2 dir=(target-origin).normalized,n=new Vector2(-dir.y,dir.x);Stream(Vector2.Lerp(origin,q,Mathf.Max(0,u-.55f)),q,5,age*3,1);p.Mass(q,13,age*14,1);p.Line(q-dir*17,q+dir*9,3,s.pale,2);for(int j=0;j<5;j++)p.Flow(q-dir*j*7,8,.5f,age*40+j,1-j*.15f,2,j,2,3);}
   if(t<0)return;float f=Fade(t,0,.065f);Stream(origin,target,5,t*3,f);p.Line(origin,target,2,A(s.pale,f),2);SharpBurst(target,t,1.65f);p.Disc(target,24+t*160,.75f,s.pale,Fade(t,.025f,.11f),2);p.Impulse(t,5,.10f);
  }
 }
}
