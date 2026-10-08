using UnityEngine;
namespace DragonTower {
 public sealed partial class WaterSkillVfx {
  partial void RenderAdvanced(){if(kind==WaterSkillKind.Surge)Surge();else RenderRemaining();}
  partial void RenderRemaining();
  Vector2 SurgePath(float t){if(t<.16f)return target+new Vector2(Mathf.Lerp(-150,150,t/.16f),0);float a=Mathf.Clamp01((t-.16f)/.68f)*Mathf.PI*2;return target+new Vector2(Mathf.Cos(a)*150,Mathf.Sin(a)*76);}
  void Surge(){if(HitsShown==0)return;float t=age-hits[0];float f=Fade(t,.78f,1.04f);var center=SurgePath(t);
   // One full oblique orbit after the transverse impact. Older sections become a broken wake.
   for(int j=35;j>=0;j--){float lag=j*.012f,ta=t-lag;if(ta<0)continue;var a=SurgePath(ta);var b=SurgePath(Mathf.Max(0,ta-.015f));float alpha=f*(1-j/38f);var dir=(a-b).normalized;var n=new Vector2(-dir.y,dir.x);float width=(19+9*Mathf.Sin(ta*19))*alpha;int depth=a.y>target.y?0:2;
    p.Quad(a+n*width,a-n*width,b-n*width,b+n*width,A(library.style.deep,alpha*.7f),depth);p.Line(b,a,width*1.25f,A(library.style.cyan,alpha*.85f),depth);p.Line(b+n*width*.35f,a+n*width*.35f,3,A(library.style.pale,alpha),depth);if(j%3==0)p.Bubble(a+n*Mathf.Sin(ta*31)*width,3,alpha,depth);
   }
   p.Mass(center,43,t*3,f,center.y>target.y?0:2);p.Disc(center+new Vector2(10,8),12,.75f,library.style.pale,f*.8f,2);
   for(int i=0;i<HitsShown;i++){float h=age-hits[i];SharpBurst(target+new Vector2((i-1)*22,0),h,i==Count-1?1.15f:.65f);if(i==Count-1)p.Impulse(h,4,.095f);}
  }
 }
}
