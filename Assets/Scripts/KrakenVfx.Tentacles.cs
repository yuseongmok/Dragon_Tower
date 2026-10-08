using UnityEngine;
namespace DragonTower {public sealed partial class KrakenVfx {
 static readonly Vector2[] Roots={new Vector2(-160,23),new Vector2(165,30),new Vector2(-204,-8),new Vector2(203,-13),new Vector2(-145,-48),new Vector2(158,-46)};
 Vector2 Root(int i)=>ground+Roots[i];
 Vector2 Tangent(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float t){float k=1-t;return (3*k*k*(b-a)+6*k*t*(c-b)+3*t*t*(d-c)).normalized;}
 Vector2 Bezier(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float t){float k=1-t;return k*k*k*a+3*k*k*t*b+3*k*t*t*c+t*t*t*d;}
 PersistentAttackBeat Motion(int limb,out float relative){PersistentAttackBeat best=default;relative=100;float distance=100;foreach(var beat in plan.attacks){if(beat.limb!=limb)continue;float dt=age-beat.time;if(dt>=-.52f&&dt<=.44f&&Mathf.Abs(dt)<distance){distance=Mathf.Abs(dt);best=beat;relative=dt;}}return best;}
 void Tentacle(int i){float spawn=.58f+i*.085f,emerge=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-spawn)/.4f));float sink=Mathf.Clamp01((age-plan.duration+i*.045f)/(.55f+i*.02f));float growth=emerge*(1-Mathf.SmoothStep(0,1,sink));if(growth<=0)return;
  float side=i%2==0?-1:1;var a=Root(i);float breadth=i<2?25:i<4?34:43;int depth=i<2?0:i<4?1:2;float alpha=i<2?.83f:i<4?1:.90f;
  Vector2 b=a+new Vector2(side*(i<2?45:60),130),c=a+new Vector2(side*(i<2?20:40),i<2?300:i<4?270:220),tip=a+new Vector2(-side*(i<2?105:i<4?65:20),i<2?250:i<4?215:170);
  float sway=Mathf.Sin(age*1.8f+i*1.7f)*16;c.x+=sway;tip.x+=Mathf.Sin(age*1.5f+i)*19;tip.y+=Mathf.Cos(age*1.7f+i*.7f)*12;
  float rel;var beat=Motion(i,out rel);bool attack=rel<1;float strike=0;
  if(attack){float wind=Mathf.SmoothStep(0,1,Mathf.Clamp01((rel+.52f)/.29f));strike=rel<0?Mathf.SmoothStep(0,1,Mathf.Clamp01((rel+.23f)/.23f)):F(rel,.035f,.40f);Vector2 hit=target;Vector2 bentB=b,bentC=c;
   if(beat.motion==PersistentAttackMotion.Sweep){tip+=new Vector2(side*45,15)*wind;bentB=a+new Vector2(-side*80,135);bentC=target+new Vector2(side*130,-5);hit=target+new Vector2(-side*55,0);}
   else if(beat.motion==PersistentAttackMotion.Wrap){bentB=target+new Vector2(-side*200,-85);bentC=target+new Vector2(-side*200,120);hit=target+new Vector2(side*25,28);}
   else {tip+=Vector2.up*70*wind;bentB=a+new Vector2(side*105,160);bentC=target+new Vector2(beat.motion==PersistentAttackMotion.DiagonalSlam?side*130:side*18,225);hit=target+new Vector2(beat.motion==PersistentAttackMotion.DiagonalSlam?-side*22:0,-5);}
   b=Vector2.Lerp(b,bentB,strike);c=Vector2.Lerp(c,bentC,strike);tip=Vector2.Lerp(tip,hit,strike);
  }
  // Retract geometrically into the aperture; no stationary silhouette fade-out.
  b=a+(b-a)*growth;c=a+(c-a)*growth;tip=a+(tip-a)*growth;
  for(int pass=0;pass<5;pass++)for(int j=0;j<38;j++){float u=j/38f,v=(j+1)/38f;var x=Bezier(a,b,c,tip,u);var y=Bezier(a,b,c,tip,v);var dir=Tangent(a,b,c,tip,u);var nextDir=Tangent(a,b,c,tip,v);var n=new Vector2(-dir.y,dir.x);var nextN=new Vector2(-nextDir.y,nextDir.x);float w=breadth*Mathf.Pow(1-u,.68f)+1,z=breadth*Mathf.Pow(1-v,.68f)+1;w*=growth;z*=growth;
   if(pass==0)water.Quad(x-n*w,x+n*w,y+nextN*z,y-nextN*z,A(profile.shadow,alpha),depth);
   if(pass==1)water.Quad(x-n*w*.8f,x+n*w*.74f,y+nextN*z*.74f,y-nextN*z*.8f,A(profile.body,alpha),depth);
   if(pass==2)water.Quad(x-n*w*.4f,x+n*w*.66f,y+nextN*z*.66f,y-nextN*z*.4f,A(profile.face,alpha*.93f),depth);
   if(pass==3)water.Quad(x+n*w*.05f,x+n*w*.40f,y+nextN*z*.40f,y+nextN*z*.05f,A(Color.Lerp(profile.face,profile.style.cyan,.3f),alpha*.9f),depth);
   if(pass!=4)continue; float shimmer=.5f+.5f*Mathf.Sin(u*17-age*3+i);water.Line(x+n*w*.53f,y+nextN*z*.53f,Mathf.Max(2,w*.17f),A(profile.style.aqua,alpha*(.25f+.4f*shimmer)),depth);
   if(j%5<2)water.Line(x+n*w*.77f,y+nextN*z*.77f,2,A(profile.style.pale,alpha*.55f),depth);
   if(j%4==1&&j<34&&j>2){float exposure=.3f+strike*.7f;var p=x-n*w*.20f;Cup(p,dir,n,w*.42f,exposure,depth,alpha);}
  }
  if(age-spawn>=0&&age-spawn<.45f){float t=age-spawn;water.Flow(a,20+t*160,.3f,t*4,F(t,.1f,.45f),2,i,5,5);water.Droplets(a,t,.7f,F(t,.15f,.45f),12);}
  if(attack&&rel>-.17f&&rel<.12f){float f=F(rel,.01f,.12f);water.Flow(target,50+strike*45,.38f,age*8+i,f*.75f,2,i,6,2.5f);water.Droplets(tip,Mathf.Max(0,rel+.05f),.5f,f,10);}
 }
 void Cup(Vector2 p,Vector2 tangent,Vector2 normal,float radius,float exposure,int depth,float alpha){for(int k=0;k<12;k++){float a=k*Mathf.PI/6,b=(k+1)*Mathf.PI/6;Vector2 X(float t,float r)=>p+tangent*Mathf.Cos(t)*r+normal*Mathf.Sin(t)*r*(.28f+exposure*.4f);water.Quad(X(a,radius),X(b,radius),X(b,radius*.55f),X(a,radius*.55f),A(k<6?profile.cupLight:profile.cups,alpha*.85f),depth);}water.Line(p-tangent*radius*.35f,p+tangent*radius*.35f,radius*.45f,A(profile.shadow,alpha),depth);}
}}

