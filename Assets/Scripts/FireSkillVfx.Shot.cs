using UnityEngine;
namespace DragonTower {
 public sealed partial class FireSkillVfx {
  void RenderShot(){
   int n=Mathf.Min(Count,32);for(int i=0;i<n;i++){
    float due=skill.initialHitDelay+i*skill.hitInterval,h=hits[i]<0?-1:age-hits[i];
    float flight=Mathf.Max(.001f,due);float t=h<0?Mathf.Clamp01(age/flight):1;
    if(h<.07f){float alpha=h<0?1:1-h/.07f;for(int pellet=0;pellet<3;pellet++){
     float fan=((i%3)-1)*63+(pellet-1)*29;var from=origin+new Vector2((pellet-1)*14,0);var end=target+new Vector2((pellet-1)*20,(i%3-1)*14);
     var p=Vector2.Lerp(from,end,t)+new Vector2(fan*Mathf.Sin(t*Mathf.PI),0);var prev=Vector2.Lerp(from,end,Mathf.Max(0,t-.25f))+new Vector2(fan*Mathf.Sin(Mathf.Max(0,t-.25f)*Mathf.PI),0);
     float angle=Mathf.Atan2((p-prev).y,(p-prev).x)*Mathf.Rad2Deg-90;
     L(FireStyleModule.DarkFlame,p,new Vector2(42+pellet*5,95),alpha,angle+180);
     L(FireStyleModule.Tongue,p,new Vector2(35,88),alpha,angle+180);
     L(FireStyleModule.Core,p,new Vector2(24,58),alpha,angle+180,false,true);
     Trail(prev,p,25,alpha,3);
     if(i==0&&skill.initialHitDelay==0)Trail(from,end,19,alpha*.8f,5);
    }}
    if(h>=0)Impact(h,i==n-1?205:105,i==n-1?34:12,i==n-1);
   }
  }
 }
}

