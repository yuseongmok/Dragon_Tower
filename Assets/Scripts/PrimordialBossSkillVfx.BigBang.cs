using UnityEngine;
namespace DragonTower {
 public sealed partial class PrimordialBossSkillVfx {
  LightVfxStyle bigBangStyle;
  void Orb(Vector2 p,float r,float alpha,float clock=0,float charge=1){
   if(bigBangStyle==null)bigBangStyle=Resources.Load<LightSkillLibrary>("LightSkills/Library").style;
   var s=bigBangStyle;
   // Match LightSkillVfx.Sphere shading; clip and bound the expanding mesh.
   int step=r>140?10:r>30?4:2;
   int left=Mathf.FloorToInt(Mathf.Max(-r,-240-p.x)/step)*step,right=Mathf.CeilToInt(Mathf.Min(r,240-p.x)/step)*step;
   int bottom=Mathf.FloorToInt(Mathf.Max(-r,-850-p.y)/step)*step,top=Mathf.CeilToInt(Mathf.Min(r,-p.y)/step)*step;
   for(int y=bottom;y<top;y+=step)for(int x=left;x<right;x+=step){
    float nx=(x+step*.5f)/r,ny=(y+step*.5f)/r,r2=nx*nx+ny*ny;if(r2>=1)continue;
    float z=Mathf.Sqrt(1-r2),light=-nx*.37f+ny*.4f+z*.76f;
    Color c=light<.12f?Color.Lerp(s.deep,s.gold,.6f):light<.38f?s.gold:light<.64f?s.pale:light<.86f?s.ivory:s.core;
    if((nx+.16f)*(nx+.16f)+(ny-.14f)*(ny-.14f)<.22f)c=Color.Lerp(s.ivory,s.core,charge);
    var q=p+new Vector2(x,y);front.Quad(q,q+Vector2.up*step,q+Vector2.one*step,q+Vector2.right*step,A(c,alpha*.96f));
   }
   if(r<30)return;
   for(int j=0;j<3;j++){
    var rot=Quaternion.Euler(53+j*11,j*27,j*59);float spin=clock*(j%2==0?1.5f:-1.2f);
    for(int k=0;k<55;k++){
     float a=spin+k*Mathf.PI*2/72,b=spin+(k+1)*Mathf.PI*2/72;
     Vector2 v=rot*(Vector3)Polar(a,r*(1.08f+j*.06f)),w=rot*(Vector3)Polar(b,r*(1.08f+j*.06f));
     front.Line(p+v,p+w,j==0?3:2,A(j==0?s.ivory:j==1?s.gold:s.lavender,alpha*.8f));
    }
    for(int k=0;k<5;k++){Vector2 v=rot*(Vector3)Polar(spin+k*Mathf.PI*.4f,r*1.13f);front.Diamond(p+v,3,6,A(s.core,alpha));}
   }
   if(r<150)for(int i=0;i<32;i++){
    float cycle=Mathf.Repeat(clock*1.5f+i*.03125f,1),a=i*2.39996f;var dir=Polar(a,1);
    var q=p+dir*(r+90*(1-cycle));float f=Mathf.Sin(cycle*Mathf.PI)*alpha;
    front.Line(q+dir*9,q,2,A(i%6==0?s.lavender:s.pale,f*.85f));front.Diamond(q,2,4,A(s.core,f));
   }
  }
  void RenderBigBang(BattleModel b,Vector2 center,Vector2 target,float clock){
   float u=b.BossPhaseProgress;
   bool prep=b.BossPreparation,travel=b.BossState==BossSkillState.Travel;
   float charge=b.BossState==BossSkillState.Warning?0:prep?u:1;
   float fade=b.BossState==BossSkillState.Recovery?1-u:1;
   rear.Quad(new Vector2(-240,0),new Vector2(240,0),new Vector2(240,-850),new Vector2(-240,-850),A(ink,.85f*fade));
   var core=center+Vector2.up*35;
   if(b.BossState==BossSkillState.Recovery){BigBangExplosion(core,target,u*1.2f);return;}
   Circle(core,145,-clock,gold,.8f*fade);
   for(int i=0;i<5;i++){
    float a=i*Mathf.PI*2/5+Mathf.PI*.5f;
    var p=core+Polar(a,145);
    float fill=Mathf.Clamp01(charge*5-i);
    Ring(front,p,25,1,4,A(stone,fade),24);
    for(int k=0;k<Mathf.FloorToInt(fill*24);k++){
     float x=k*Mathf.PI/12,z=(k+1)*Mathf.PI/12;
     front.Line(p+Polar(x,25),p+Polar(z,25),5,A(ivory,fade));
    }
    if(fill>=1){Orb(p,16,fade);front.Line(p,core,3,A(gold,.7f*fade));}
   }
   float radius=20+charge*65;
   if(travel)radius=Mathf.Lerp(85,620,Mathf.SmoothStep(0,1,u));
   if(b.BossState==BossSkillState.Recovery)radius=620+u*180;
   Orb(core,radius,fade,clock,charge);
   if(prep){
    // Each successful attack erodes energy; the bar shows the final reduction directly.
    var bar=target+Vector2.up*100;
    floor.transform.SetAsLastSibling();
    floor.Line(bar+Vector2.left*90,bar+Vector2.right*90,10,ink);
    floor.Line(bar+Vector2.left*90,bar+Vector2.left*90+Vector2.right*(180*b.BigBangReduction/.8f),6,cyan);
   }
  }
  void BigBangExplosion(Vector2 core,Vector2 target,float t){
   // The sphere disappears at the damage frame: a brief rupture, then hollow waves and shards.
   float flash=Mathf.Clamp01(1-t/.14f),tail=Mathf.Clamp01(1-t/1.2f);
   front.Quad(new Vector2(-240,0),new Vector2(240,0),new Vector2(240,-850),new Vector2(-240,-850),A(ivory,flash*.65f));
   for(int j=0;j<3;j++){
    float age=t-j*.08f;if(age<0)continue;
    float r=40+age*(j==0?1450:1050),f=Mathf.Clamp01(1-age/.65f);
    Ring(front,core,r,1, (j==0?24:12)*f+2,A(gold,f*.85f),96);
    Ring(front,core,r-8,1,5,A(ivory,f),96);
    Ring(front,target,30+age*850,.28f,8*f+1,A(ivory,f*.7f),80);
   }
   // Jagged radial rays erupt once; they retract to expose the arena immediately.
   for(int k=0;k<28;k++){
    float a=k*2.39996f;var dir=Polar(a,1);var n=Polar(a+Mathf.PI*.5f,1);
    float f=Mathf.Clamp01(1-t/.42f),inner=20+t*420,outer=inner+(150+(k%5)*55)*f;
    var q=core+dir*inner;
    front.Quad(q,core+dir*(inner+45)-n*(8+k%4)*f,core+dir*outer,core+dir*(inner+45)+n*(8+k%4)*f,A(k%3==0?ivory:gold,f));
   }
   // Deterministic fragments use the battle clock, so replay/resume remains stable.
   for(int k=0;k<72;k++){
    float a=k*2.39996f;var dir=Polar(a,1);float speed=190+(k%11)*39;
    var q=core+dir*(25+speed*t)+Vector2.down*(t*t*95);
    float size=(4+k%5)*tail;var x=Polar(a+t*(k%2==0?4:-5),size);var y=new Vector2(-x.y,x.x);
    front.Line(q-dir*(12+speed*.035f)*tail,q,3*tail,A(gold,tail*.55f));
    front.Quad(q-x,q+y*1.8f,q+x,q-y,A(k%4==0?ivory:gold,tail));
    front.Line(q-x,q+y*1.8f,2,A(ivory,tail));
   }
  }
 }
}
