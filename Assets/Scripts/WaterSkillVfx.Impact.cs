using UnityEngine;
namespace DragonTower {
 public sealed partial class WaterSkillVfx {
  static float Noise(int i)=>Mathf.Repeat(Mathf.Sin(i*127.1f+31.7f)*43758.5453f,1);
  // Short torn water sheets break into detached ballistic spray, never curling tentacles.
  void SharpBurst(Vector2 center,float t,float scale,bool upward=false){if(t<0||t>1.45f)return;var s=library.style;float flash=Fade(t,.025f,.13f),sheet=Fade(t,.025f,.21f);
   p.Disc(center,scale*(15+t*210),upward?.35f:.7f,s.pale,flash,2,32);
   for(int i=0;i<19;i++){float a=upward?(.25f+Noise(i)*2.64f):i*2.39996f;Vector2 dir=D(a),n=new Vector2(-dir.y,dir.x);float reach=(32+Noise(i+3)*80)*scale*Mathf.Clamp01(.3f+t/.07f);var root=center+dir*(12+t*80)*scale;var tip=center+dir*reach;float width=(5+Noise(i+7)*11)*scale*sheet;
    p.Quad(root-n*width,root+n*width,tip,tip,A(s.azure,sheet*.85f),i%4==0?0:2);p.Quad(root-n*width*.35f,root+n*width*.65f,tip,tip,A(s.aqua,sheet),2);p.Line(root+n*width*.45f,tip,2*scale,A(s.foam,sheet),2);
   }
   for(int i=0;i<86;i++){float born=Noise(i+92)*.085f,life=.4f+Noise(i+44)*.85f,u=t-born;if(u<0||u>life)continue;float angle=upward?.3f+Noise(i+20)*2.54f:i*2.39996f;Vector2 velocity=D(angle)*(95+Noise(i)*250)*scale;var q=center+velocity*u+Vector2.down*(upward?165:125)*u*u;Vector2 dir=(velocity+Vector2.down*250*u).normalized,n=new Vector2(-dir.y,dir.x);float size=(1.6f+Noise(i+6)*4)*scale,alpha=Fade(u,life*.45f,life);float length=size*(1.5f+2*Fade(u,0,.18f));int depth=i%5==0?0:2;
    p.Quad(q-dir*length,q+n*size,q+dir*size*.8f,q-n*size,A(s.azure,alpha*.9f),depth);p.Quad(q-dir*length*.65f,q+n*size*.65f,q+dir*size*.7f,q-n*size*.35f,A(s.aqua,alpha),depth);p.Line(q-dir*length*.3f,q+dir*size*.6f,Mathf.Max(1,size*.45f),A(s.foam,alpha),depth);
   }
   for(int i=0;i<3;i++)p.Flow(center,scale*(24+t*(230+i*20)),upward?.22f:.48f,i+t,Fade(t,.06f,.34f),i==0?0:2,i,3-i*.6f,3.8f);
  }
 }
}
